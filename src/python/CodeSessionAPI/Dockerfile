ARG FOUNDATIONALLM_REGISTRY="ghcr.io/solliancenet/foundationallm"
ARG PYTHON_FLLM_BASE_VERSION="latest"
ARG BASE_IMAGE="${FOUNDATIONALLM_REGISTRY}/python-fllm-base:${PYTHON_FLLM_BASE_VERSION}"
FROM ${BASE_IMAGE}
ARG FOUNDATIONALLM_VERSION
ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}

COPY ./CodeSessionAPI/requirements.txt /code/requirements.txt
RUN python3 -m pip install --no-cache-dir --upgrade -r /code/requirements.txt

COPY ./CodeSessionAPI /code

RUN mkdir -p /mnt/data

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80"]
LABEL org.opencontainers.image.source=https://github.com/solliancenet/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
