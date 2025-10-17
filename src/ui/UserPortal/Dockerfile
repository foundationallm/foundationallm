# syntax = docker/dockerfile:1

ARG NODE_VERSION=23.11.0
ARG FOUNDATIONALLM_VERSION

FROM node:${NODE_VERSION}-alpine3.21 AS base

ARG PORT=3000

ENV NODE_ENV=production
ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}

WORKDIR /src

# Build
FROM base AS build

COPY package.json package-lock.json ./
RUN npm install --production=false

COPY . .

RUN npm run build
RUN npm prune

# Run
FROM base

ENV PORT=$PORT

COPY --from=build /src/.output /src/.output

ENTRYPOINT [ "node", ".output/server/index.mjs" ]
LABEL org.opencontainers.image.source=https://github.com/foundationallm/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
