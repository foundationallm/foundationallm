using FoundationaLLM.Common.Models.Hubs;

namespace FoundationaLLM.Common.Tests.Models.Hubs
{
    public class DataSourceHubRequestTests
    {
        private DataSourceHubRequest _dataSourceHubRequest = new DataSourceHubRequest();

        [Fact]
        public void DataSourceHubRequest_Contains_SessionId_Property()
        {
            // Assert
            Assert.Null(_dataSourceHubRequest.SessionId);
        }

        [Fact]
        public void DataSourceHubRequest_Contains_DataSources_Property()
        {
            // Assert
            Assert.Null(_dataSourceHubRequest.DataSources);
        }
    }
}
