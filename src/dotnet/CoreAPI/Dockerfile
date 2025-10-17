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

# Copy only project files first and restore. This keeps the restore layer cached
# unless project references change.
COPY dotnet/Common/Common.csproj dotnet/Common/
COPY dotnet/Core/Core.csproj dotnet/Core/
COPY dotnet/CoreAPI/CoreAPI.csproj dotnet/CoreAPI/

COPY dotnet/Agent/Agent.csproj dotnet/Agent/
COPY dotnet/AIModel/AIModel.csproj dotnet/AIModel/
COPY dotnet/Attachments/Attachment.csproj dotnet/Attachments/
COPY dotnet/Authorization/Authorization.csproj dotnet/Authorization/
COPY dotnet/AzureAI/AzureAI.csproj dotnet/AzureAI/
COPY dotnet/AzureOpenAI/AzureOpenAI.csproj dotnet/AzureOpenAI/
COPY dotnet/Configuration/Configuration.csproj dotnet/Configuration/
COPY dotnet/Context/Context.csproj dotnet/Context/
COPY dotnet/Conversation/Conversation.csproj dotnet/Conversation/
COPY dotnet/Prompt/Prompt.csproj dotnet/Prompt/

RUN dotnet restore "dotnet/Common/Common.csproj"
RUN dotnet restore "dotnet/Core/Core.csproj"
RUN dotnet restore "dotnet/CoreAPI/CoreAPI.csproj"

RUN dotnet restore "dotnet/Agent/Agent.csproj"
RUN dotnet restore "dotnet/AIModel/AIModel.csproj"
RUN dotnet restore "dotnet/Attachments/Attachment.csproj"
RUN dotnet restore "dotnet/Authorization/Authorization.csproj"
RUN dotnet restore "dotnet/AzureAI/AzureAI.csproj"
RUN dotnet restore "dotnet/AzureOpenAI/AzureOpenAI.csproj"
RUN dotnet restore "dotnet/Configuration/Configuration.csproj"
RUN dotnet restore "dotnet/Context/Context.csproj"
RUN dotnet restore "dotnet/Conversation/Conversation.csproj"
RUN dotnet restore "dotnet/Prompt/Prompt.csproj"

# Now copy the rest of the source and build/publish
COPY . .

RUN dotnet build "dotnet/Common/Common.csproj" -c Release -o /app/build
RUN dotnet build "dotnet/Core/Core.csproj" -c Release -o /app/build
RUN dotnet build "dotnet/CoreAPI/CoreAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet/Common/Common.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Core/Core.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/CoreAPI/CoreAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoundationaLLM.Core.API.dll"]
LABEL org.opencontainers.image.source=https://github.com/foundationallm/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
