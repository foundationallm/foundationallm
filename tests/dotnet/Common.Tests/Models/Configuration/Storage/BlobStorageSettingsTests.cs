using FoundationaLLM.Common.Models.Configuration.Storage;

namespace FoundationaLLM.Common.Tests.Models.Configuration.Storage
{
    public class BlobStorageSettingsTests
    {
        private readonly BlobStorageSettings _blobStorageSettings = new BlobStorageSettings
        {
            BlobStorageConnection = "TestConnection",
            BlobStorageContainer = "TestContainer"
        };

        [Fact]
        public void TestBlobStorageConnection()
        {
            // Assert
            Assert.Equal("TestConnection", _blobStorageSettings.BlobStorageConnection);
        }

        [Fact]
        public void TestBlobStorageContainer()
        {
            // Assert
            Assert.Equal("TestContainer", _blobStorageSettings.BlobStorageContainer);
        }
    }
}
