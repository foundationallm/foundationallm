FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.19 AS base
ARG FOUNDATIONALLM_VERSION
WORKDIR /app

ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.19 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "dotnet/Common/Common.csproj"
RUN dotnet build "dotnet/Common/Common.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Core/Core.csproj"
RUN dotnet build "dotnet/Core/Core.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/CoreWorker/CoreWorker.csproj"
RUN dotnet build "dotnet/CoreWorker/CoreWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet/Common/Common.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Core/Core.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/CoreWorker/CoreWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoundationaLLM.Core.Worker.dll"]
LABEL org.opencontainers.image.source=https://github.com/foundationallm/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
