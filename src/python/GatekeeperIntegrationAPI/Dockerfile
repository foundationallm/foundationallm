FROM python:3.11-alpine3.21
ARG FOUNDATIONALLM_VERSION
ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}
ENV APPLICATIONINSIGHTS_STATSBEAT_DISABLED_ALL=true
ENV architecture=amd64
RUN apk update
RUN apk add curl gpg gpg-agent

#Download the desired package(s)
RUN curl -O https://download.microsoft.com/download/fae28b9a-d880-42fd-9b98-d779f0fdd77f/msodbcsql18_18.5.1.1-1_$architecture.apk
RUN curl -O https://download.microsoft.com/download/7/6/d/76de322a-d860-4894-9945-f0cc5d6a45f8/mssql-tools18_18.4.1.1-1_$architecture.apk

#(Optional) Verify signature, if 'gpg' is missing install it using 'apk add gnupg':
RUN curl -O https://download.microsoft.com/download/fae28b9a-d880-42fd-9b98-d779f0fdd77f/msodbcsql18_18.5.1.1-1_$architecture.sig
RUN curl -O https://download.microsoft.com/download/7/6/d/76de322a-d860-4894-9945-f0cc5d6a45f8/mssql-tools18_18.4.1.1-1_$architecture.sig

# RUN curl https://packages.microsoft.com/keys/microsoft.asc  | gpg --import -
# RUN gpg --verify msodbcsql18_18.5.1.1-1_$architecture.sig msodbcsql18_18.5.1.1-1_$architecture.apk
# RUN gpg --verify mssql-tools18_18.4.1.1-1_$architecture.sig mssql-tools18_18.4.1.1-1_$architecture.apk

#Install the package(s)
RUN apk add --allow-untrusted msodbcsql18_18.5.1.1-1_$architecture.apk
RUN apk add --allow-untrusted mssql-tools18_18.4.1.1-1_$architecture.apk

RUN apk add --no-cache gcc git cmake && \
    apk add --no-cache --virtual .build-deps build-base python3-dev musl-dev linux-headers && \
    apk add --no-cache --virtual .msodbcdeps unixodbc unixodbc-dev && \
    apk add --no-cache --virtual .mssqltoolsdeps libgcc libstdc++ && \
    apk cache clean

WORKDIR /code
COPY ./GatekeeperIntegrationAPI/requirements.txt /code/requirements.txt
RUN pip install --no-cache-dir --upgrade -r /code/requirements.txt
COPY ./IntegrationSDK/foundationallm /code/foundationallm
COPY ./GatekeeperIntegrationAPI /code

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80"]
LABEL org.opencontainers.image.source=https://github.com/solliancenet/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
