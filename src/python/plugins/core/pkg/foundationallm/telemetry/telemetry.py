import logging
import platform
import foundationallm

from azure.monitor.opentelemetry import configure_azure_monitor
from opentelemetry import trace
from opentelemetry.sdk.resources import SERVICE_NAME, Resource, SERVICE_INSTANCE_ID, SERVICE_VERSION, SERVICE_NAMESPACE
from opentelemetry.trace import Span, Status, StatusCode, Tracer
from foundationallm.config import Configuration

from langchain_core.globals import set_debug, set_verbose

# Custom filter to exclude trace logs
class ExcludeTraceLogsFilter(logging.Filter):
    def filter(self, record):
        filter_out = 'applicationinsights' not in record.getMessage().lower()
        filter_out = filter_out and 'response status' not in record.getMessage().lower()
        filter_out = filter_out and 'transmission succeeded' not in record.getMessage().lower()
        return filter_out

class Telemetry:
    """
    Manages logging and the recording of application telemetry.
    """

    log_level : int = logging.WARNING
    azure_log_level : int = logging.WARNING
    langchain_log_level : int = logging.NOTSET
    api_name : str = None
    telemetry_connection_string : str = None

    @staticmethod
    def configure_monitoring(config: Configuration, telemetry_connection_string: str, api_name : str):
        """
        Configures monitoring and sends logs, metrics, and events to Azure Monitor.
        Parameters
        ----------
        config : Configuration
            Configuration class used for retrieving application settings from
            Azure App Configuration.
        telemetry_connection_string : str
            The connection string used to connect to Azure Application Insights.
        """

        Telemetry.telemetry_connection_string = config.get_value(telemetry_connection_string)
        Telemetry.api_name = api_name
        resource = Resource.create(
            {
                SERVICE_NAME: f"{Telemetry.api_name}",
                SERVICE_NAMESPACE : f"FoundationaLLM",
                SERVICE_VERSION: f"{foundationallm.__version__}",
                SERVICE_INSTANCE_ID: f"{platform.node()}"
            })

        # Configure Azure Monitor defaults
        configure_azure_monitor(
            connection_string=Telemetry.telemetry_connection_string,
            disable_offline_storage=True,
            disable_metrics=True,
            disable_tracing=False,
            disable_logging=False,
            resource=resource
        )

        #Configure telemetry logging
        Telemetry.configure_logging(config)

    @staticmethod
    def get_logger(name: str, level: int = logging.INFO) -> logging.Logger:
        """
        Creates a logger by the specified name and logging level.

        Parameters
        ----------
        name : str
            The name to assign to the logger instance.
        level : int
            The logging level to assign.

        Returns
        -------
        Logger
            Returns a logger object with the specified name and logging level.
        """
        logger = logging.getLogger(name)
        logger.setLevel(level)
        return logger

    @staticmethod
    def get_tracer(name: str) -> Tracer:
        """
        Creates an OpenTelemetry tracer with the specified name.

        Parameters
        ----------
        name : str
            The name to assign to the tracer.

        Returns
        -------
        Tracer
            Returns an OpenTelemetry tracer for span creation and in-process context propagation.
        """
        return trace.get_tracer(name)

    @staticmethod
    def record_exception(span: Span, ex: Exception):
        """
        Associates an exception with an OpenTelemetry Span and logs it.

        Parameters
        ----------
        span : Span
            The OpenTelemetry span to which the execption should be associated.
        ex : Exception
            The exception that occurred.
        """
        span.set_status(Status(StatusCode.ERROR))
        span.record_exception(ex)

    @staticmethod
    def translate_log_level(log_level: str) -> int:
        """
        Translates a log level string to a logging level.

        Parameters
        ----------
        log_level : str
            The log level to translate.

        Returns
        -------
        int
            Returns the logging level for the specified log level string.
        """
        if log_level == "Debug":
            return logging.DEBUG
        elif log_level == "Trace":
            return logging.DEBUG
        elif log_level == "Information":
            return logging.INFO
        elif log_level == "Warning":
            return logging.WARNING
        elif log_level == "Error":
            return logging.ERROR
        elif log_level == "Critical":
            return logging.CRITICAL
        else:
            return logging.NOTSET

    @staticmethod
    def configure_logging(config: Configuration):

        Telemetry.log_level = Telemetry.translate_log_level(
            config.get_value("FoundationaLLM:PythonSDK:Logging:LogLevel:Default"))
        
        Telemetry.azure_log_level = Telemetry.translate_log_level(
            config.get_value("FoundationaLLM:PythonSDK:Logging:LogLevel:Azure"))

        enable_console_logging = config.get_value("FoundationaLLM:PythonSDK:Logging:EnableConsoleLogging")

        #Log output handlers
        handlers = []

        set_debug(False)
        set_verbose(False)

        #map log level to python log level - console is only added for Information or higher
        if Telemetry.log_level == logging.DEBUG:
            set_debug(True)
            set_verbose(True)
            handlers.append(logging.StreamHandler())

        #Logging configuration
        LOGGING = {
            'version': 1,
            'disable_existing_loggers': False,
            'formatters': {
                'default': {
                    'format': '[%(asctime)s] [%(levelname)s] %(name)s: %(message)s'
                },
                'standard': {
                    'format': '[%(asctime)s] [%(levelname)s] %(name)s: %(message)s'
                },
                'azure': {
                    'format': '%(name)s: %(message)s'
                },
                'error': {
                    'format': '[%(asctime)s] [%(levelname)s] %(name)s %(process)d::%(module)s|%(lineno)s:: %(message)s'
                }
            },
            'handlers': {
                'default': {
                    'level': Telemetry.log_level,
                    'formatter': 'standard',
                    'class': 'logging.StreamHandler',
                    'filters' : ['exclude_trace_logs'],
                    'stream': 'ext://sys.stdout',
                },
                'console': {
                    'level': Telemetry.log_level,
                    'formatter': 'standard',
                    'class': 'logging.StreamHandler',
                    'filters' : ['exclude_trace_logs'],
                    'stream': 'ext://sys.stdout'
                },
                "azure": {
                    'formatter': 'azure',
                    'level': Telemetry.log_level,
                    "class": "opentelemetry.sdk._logs.LoggingHandler",
                    'filters' : ['exclude_trace_logs'],
                }
            },
            'filters': {
                'exclude_trace_logs': {
                    '()': 'foundationallm.telemetry.ExcludeTraceLogsFilter',
                },
            },
            'loggers': {
                'azure': {  # Adjust the logger name accordingly
                    'level': Telemetry.azure_log_level,
                    "class": "opentelemetry.sdk._logs.LoggingHandler",
                    'filters': ['exclude_trace_logs']
                },
                '': {
                    'handlers': ['console'],
                    'level': Telemetry.log_level,
                    'filters': ['exclude_trace_logs'],
                },
            },
            "root": {
                "handlers": ["azure", "console"],
                "level": Telemetry.log_level,
            }
        }

        #remove console if prod env (cut down on duplicate log data)
        if enable_console_logging != 'true':
            LOGGING['root']['handlers'] = ["azure"]

        #set the logging configuration
        logging.config.dictConfig(LOGGING)
