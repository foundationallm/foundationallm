from abc import ABC, abstractmethod

class StorageManagerBase(ABC):
    """
    The StorageManagerBase class is responsible for defining how to
    interact with a storage account.
    """
    @abstractmethod
    def file_exists(self, path) -> bool:
        """
        Checks whether a specified path exists in blob storage.

        Parameters
        ----------
        path : str
            The path the check for existence.

        Returns
        -------
        bool
            Returns true of the specified path exists. Otherwise, false.
        """
        pass

    @abstractmethod
    def read_file_content(self, path) -> bytes:
        """
        Retrieves the contents of a specified file in bytes.

        Parameters
        ----------
        path : str
            The path to the blob being retrieved.
        read_into_stream : boolean
            Flag indicating whether to read the file content into a byte stream. Default is True.

        Returns
        -------
        bytes
            Returns the bytes, or a stream of bytes, representing the content of the specified file
            or None if the file does not exist.
        """
        pass

    @abstractmethod
    def write_file_content(self, path, content):
        """
        Writes data to a specified file.

        Parameters
        ----------
        path : str
            The path to the blob to which data is being written.
        content
            The data being written into the blob.
        overwrite : boolean
            Indicates whether the content of the blob should be overwritten by the incoming content.
            Default is True.
        lease
            The lease to use for accessing the blob. Default is None.
        """
        pass

    @abstractmethod
    def delete_file(self, path):
        """
        Deletes the specified blob.

        Parameters
        ----------
        path : str
            The path to the blob to be deleted.
        """
        pass
