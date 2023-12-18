using FoundationaLLM.Vectorization.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Services.ContentSources
{
    public class BlobStorageContentSourceService : ContentSourceServiceBase, IContentSourceService
    {
        public BlobStorageContentSourceService()
        {
        }

        /// <inheritdoc/>
        public async Task<BinaryData> ReadFileAsync(string filePath)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
