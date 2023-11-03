from foundationallm.hubs import Metadata

class CSVAuthentication(Metadata):
    """
    Authentication settings for a connection to a CSV file.
            
    If path_value_is_secret is True, the source_file_path is the secret key.
    Otherwise, the source_file_path is a SAS URL.
    
    """
    source_file_path: str
    path_value_is_secret: bool