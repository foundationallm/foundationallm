import pytest
from foundationallm.config import Configuration
from foundationallm.hubs.data_source import DataSourceRepository
@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def data_source_repository(test_config):
    return DataSourceRepository(config=test_config)


class DataSourceRepositoryTests:
    def test_anomaly_data_source_deserializes_properly(self, data_source_repository):
        ds = data_source_repository.get_metadata_by_name("anomaly-ds")
        print(ds)
        assert ds.name == "anomaly-ds"