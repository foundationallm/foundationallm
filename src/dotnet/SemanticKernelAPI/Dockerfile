FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.21 AS base
ARG FOUNDATIONALLM_VERSION

RUN apk update
RUN apk add icu-libs icu-data-full

WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.21 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "dotnet/Common/Common.csproj"
RUN dotnet build "dotnet/Common/Common.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/SemanticKernel/SemanticKernel.csproj"
RUN dotnet build "dotnet/SemanticKernel/SemanticKernel.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/SemanticKernelAPI/SemanticKernelAPI.csproj"
RUN dotnet build "dotnet/SemanticKernelAPI/SemanticKernelAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet/Common/Common.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/SemanticKernel/SemanticKernel.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/SemanticKernelAPI/SemanticKernelAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoundationaLLM.SemanticKernel.API.dll"]
LABEL org.opencontainers.image.source=https://github.com/foundationallm/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
