FROM alpine:3.20

RUN apk update
RUN apk add --no-cache \
    ca-certificates \
    less \
    ncurses-terminfo-base \
    krb5-libs \
    libgcc \
    libintl \
    libssl1.1 \
    libstdc++ \
    tzdata \
    userspace-rcu \
    zlib \
    icu-libs \
    curl \
    bash \
    icu-libs \
    python3 \
    py3-pip \
    ipcalc

RUN ipcalc --version

RUN apk -X https://dl-cdn.alpinelinux.org/alpine/edge/main add --no-cache lttng-ust
RUN curl -L https://github.com/PowerShell/PowerShell/releases/download/v7.4.4/powershell-7.4.4-linux-musl-x64.tar.gz -o /tmp/powershell.tar.gz
RUN mkdir -p /opt/microsoft/powershell/7
RUN tar zxf /tmp/powershell.tar.gz -C /opt/microsoft/powershell/7
RUN chmod +x /opt/microsoft/powershell/7/pwsh
RUN sudo ln -s /opt/microsoft/powershell/7/pwsh /usr/bin/pwsh
RUN pwsh --version

RUN curl -fsSL https://aka.ms/install-azd.sh -o install-azd.sh
RUN bash install-azd.sh
RUN azd version


RUN apk add python3 py3-pip
RUN pip install azure-cli

ENTRYPOINT [ "pwsh" ]