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

RUN dotnet restore "dotnet/Configuration/Configuration.csproj"
RUN dotnet build "dotnet/Configuration/Configuration.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Prompt/Prompt.csproj"
RUN dotnet build "dotnet/Prompt/Prompt.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Authorization/Authorization.csproj"
RUN dotnet build "dotnet/Authorization/Authorization.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/SemanticKernel/SemanticKernel.csproj"
RUN dotnet build "dotnet/SemanticKernel/SemanticKernel.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Gateway/Gateway.csproj"
RUN dotnet build "dotnet/Gateway/Gateway.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/GatewayAdapterAPI/GatewayAdapterAPI.csproj"
RUN dotnet build "dotnet/GatewayAdapterAPI/GatewayAdapterAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet/Common/Common.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Configuration/Configuration.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Prompt/Prompt.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Authorization/Authorization.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/SemanticKernel/SemanticKernel.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Gateway/Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/GatewayAdapterAPI/GatewayAdapterAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoundationaLLM.Gateway.AdapterAPI.dll"]
LABEL org.opencontainers.image.source=https://github.com/foundationallm/foundationallm
LABEL foundationallm.version=$FOUNDATIONALLM_VERSION
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
