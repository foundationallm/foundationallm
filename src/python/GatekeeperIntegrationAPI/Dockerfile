# ARG FOUNDATIONALLM_REGISTRY="ghcr.io/solliancenet/foundationallm"
# ARG PYTHON_FLLM_BASE_VERSION="latest"
# ARG BASE_IMAGE="${FOUNDATIONALLM_REGISTRY}/fllm-python-base:${PYTHON_FLLM_BASE_VERSION}"
# FROM ${BASE_IMAGE}
FROM python:3.11-slim
ARG FOUNDATIONALLM_VERSION
ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}
ENV APPLICATIONINSIGHTS_STATSBEAT_DISABLED_ALL=true

RUN python3 -m pip install --no-cache-dir --upgrade setuptools==78.1.1

WORKDIR /code
COPY ./GatekeeperIntegrationAPI/requirements.txt /code/requirements.txt
RUN pip install --no-cache-dir --upgrade -r /code/requirements.txt

COPY ./IntegrationSDK/foundationallm /code/foundationallm
COPY ./GatekeeperIntegrationAPI /code

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80"]
LABEL org.opencontainers.image.source=https://github.com/foundationallm/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
