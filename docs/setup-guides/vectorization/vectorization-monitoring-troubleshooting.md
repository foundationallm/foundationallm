# Monitoring and troubleshooting vectorization

The typical steps you have to perform when monitoring and troubleshooting vectorization in FoundationaLLM (FLLM) are:

1. Check the configuration of the Vectorization API and Vectorization Worker. For more details, see [Configuring vectorization](vectorization-configuration.md).
2. Check the status of the Vectorization API and Vectorization Worker(s). Ensure the services have started and initialized successfully.
3. Check the logs of the Vectorization API and Vectorization Worker(s) for errors. By default, the logs are written to the Azure App Insights Log Analytics Workspace deployed by FLLM.
4. Check the definitions of the vectorization profiles used in the vectorization requests. For more details, see [Managing vectorization profiles](vectorization-profiles.md). Ensure all the required app configuration elements are present and have the correct values.
5. Check the state of the vectorization requests. By default, the vectorization requests are stored in the `vectorization-state` container of the Azure Storage account deployed by FLLM.