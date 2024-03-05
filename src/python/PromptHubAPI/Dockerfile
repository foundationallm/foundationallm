FROM python:3.11
ARG FOUNDATIONALLM_VERSION
ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}
ENV APPLICATIONINSIGHTS_STATSBEAT_DISABLED_ALL=true

WORKDIR /code
COPY ./PromptHubAPI/requirements.txt /code/requirements.txt
RUN pip install --no-cache-dir --upgrade -r /code/requirements.txt
COPY ./PythonSDK/foundationallm /code/foundationallm
COPY ./PromptHubAPI /code

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80", "--forwarded-allow-ips", "*", "--proxy-headers"]
LABEL org.opencontainers.image.source=https://github.com/solliancenet/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
