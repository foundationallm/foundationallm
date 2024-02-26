using FoundationaLLM.Common.Models.Orchestration.DataSources;

namespace FoundationaLLM.Common.Tests.Models.Orchestration.DataSources
{
    public class DataSourceBaseTests
    {
        [Fact]
        public void DataSourceBase_DataDescription_Property_Test()
        {
            // Arrange
            var dataSource = new DataSourceBase();
            var dataDescription = "Test data description";

            // Act
            dataSource.DataDescription = dataDescription;

            // Assert
            Assert.Equal(dataDescription, dataSource.DataDescription);
        }
    }
}
