#!/usr/bin/env bash
set -euox pipefail

REPOSITORY="certbot-app"
CONTEXT="."

SHA=$(git rev-parse --short HEAD)
docker buildx build --rm -t "${REPOSITORY}:${SHA}" "${CONTEXT}"
docker tag "${REPOSITORY}:${SHA}" "${REPOSITORY}:latest"

docker images "${REPOSITORY}"