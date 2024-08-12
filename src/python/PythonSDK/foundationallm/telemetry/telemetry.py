import logging
from azure.monitor.opentelemetry import configure_azure_monitor
from azure.monitor.opentelemetry.exporter import AzureMonitorTraceExporter
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode, Tracer
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.resources import SERVICE_NAME, Resource
from opentelemetry.sdk.trace.export import (
    BatchSpanProcessor,
    ConsoleSpanExporter,
)

from foundationallm.config import Configuration

from opentelemetry._logs import (
    get_logger_provider,
    set_logger_provider,
)

from opentelemetry.sdk._logs import (
    LoggerProvider,
    LoggingHandler,
)

class Telemetry:
    """
    Manages logging and the recording of application telemetry.
    """

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

        """
        configure_azure_monitor(
            connection_string=config.get_value(telemetry_connection_string),
            disable_offline_storage=True
        )
        """

        trace.set_tracer_provider(
            TracerProvider(
                    resource=Resource.create({SERVICE_NAME: f"[FoundationaLLM]/{api_name}"})
                )
            )

        azure_exporter = AzureMonitorTraceExporter(
            connection_string=config.get_value(telemetry_connection_string)
        )

        # Create a BatchSpanProcessor and add the exporter to it
        azure_span_processor = BatchSpanProcessor(azure_exporter)

        # add to the tracer
        trace.get_tracer_provider().add_span_processor(azure_span_processor)

        logger_provider = LoggerProvider(
            resource=Resource.create(
                {
                    SERVICE_NAME: api_name
                }
            )
        )

        set_logger_provider(logger_provider)

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
