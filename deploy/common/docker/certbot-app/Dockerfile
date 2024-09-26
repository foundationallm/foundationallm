FROM mcr.microsoft.com/vscode/devcontainers/base:0-bullseye

ARG INSTALL_ZSH="true"
ARG UPGRADE_PACKAGES="true"
ARG USERNAME=certbot
ARG USER_GID=1001
ARG USER_UID=1001

COPY library-scripts/*.sh /tmp/library-scripts/

RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && bash /tmp/library-scripts/common-debian.sh "${INSTALL_ZSH}" "${USERNAME}" "${USER_UID}" "${USER_GID}" "${UPGRADE_PACKAGES}" "true" "true" \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*

RUN bash /tmp/library-scripts/azcli-debian.sh
RUN bash /tmp/library-scripts/install-certbot.sh
RUN bash /tmp/library-scripts/pwsh-debian.sh

RUN rm -rf /tmp/library-scripts

RUN mkdir /app
COPY Function-Library.ps1 /app/Function-Library.ps1
COPY New-LetsEncryptCertificates.ps1 /app/New-LetsEncryptCertificate.ps1

ENTRYPOINT ["pwsh", "/app/New-LetsEncryptCertificate.ps1"]