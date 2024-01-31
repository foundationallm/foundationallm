# Vectorization

Foundationa**LLM** (FLLM) provides utilities and services to support vectorizing your data in batch and on-demand modalities. Vectorization is a multi-step process, starting with loading your data, splitting (or chunking) the data as required, performing vector embeddings, and storing the vectors into a vector index so [an agent](../agents/index.md) can later retrieve relevant information through a vector search. Use the links below to learn more about configuring vectorization in FLLM:

- [Vectorization API](vectorization-api.md)
- [Vectorization Worker](vectorization-worker.md)

To get started with the Vectorization API, import the [provided Postman collection.](./FoundationaLLM.Vectorization.API.postman_collection.json) Ensure to set the Vectorization API key (`X-API-KEY` header) on the **Authorization** tab of the collection and the Base URL (`baseUrl` collection variable) on the **Variables** tab.
