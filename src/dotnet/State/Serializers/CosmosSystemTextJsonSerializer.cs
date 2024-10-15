using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace FoundationaLLM.State.Serializers
{
    /// <summary>
    /// Custom serializer for Cosmos DB that uses System.Text.Json.
    /// </summary>
    public class CosmosSystemTextJsonSerializer : CosmosSerializer
    {
        private readonly JsonObjectSerializer systemTextJsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosSystemTextJsonSerializer"/> class.
        /// </summary>
        /// <param name="jsonSerializerOptions"></param>
        public CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions) =>
            this.systemTextJsonSerializer = new JsonObjectSerializer(jsonSerializerOptions);

        /// <inheritdoc/>
        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (stream.CanSeek
                    && stream.Length == 0)
                {
                    return default!;
                }

                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                return (T)this.systemTextJsonSerializer.Deserialize(stream, typeof(T), default)!;
            }
        }

        /// <inheritdoc/>
        public override Stream ToStream<T>(T input)
        {
            MemoryStream streamPayload = new MemoryStream();
            this.systemTextJsonSerializer.Serialize(streamPayload, input, typeof(T), default);
            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}
