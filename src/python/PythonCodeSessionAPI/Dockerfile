ARG FOUNDATIONALLM_REGISTRY="ghcr.io/solliancenet/foundationallm"
ARG PYTHON_FLLM_BASE_VERSION="latest"
ARG BASE_IMAGE="${FOUNDATIONALLM_REGISTRY}/fllm-python-base:${PYTHON_FLLM_BASE_VERSION}"
FROM ${BASE_IMAGE}
ARG FOUNDATIONALLM_VERSION
ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}

FROM alpine:3.21 AS antiword
RUN apk add --no-cache antiword

FROM ${BASE_IMAGE}
COPY --from=antiword /usr/bin/antiword /usr/bin/antiword
COPY --from=antiword /usr/share/antiword /usr/share/antiword
ENV ANTIWORDHOME=/usr/share/antiword

WORKDIR /code
COPY ./PythonCodeSessionAPI/requirements.txt /code/requirements.txt
RUN python3 -m pip install --no-cache-dir --upgrade -r /code/requirements.txt

COPY ./PythonCodeSessionAPI /code

RUN mkdir -p /mnt/data

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80"]
LABEL org.opencontainers.image.source=https://github.com/foundationallm/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
