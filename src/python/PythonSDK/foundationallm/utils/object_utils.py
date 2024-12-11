import re
from typing import Type, TypeVar, Dict
from foundationallm.langchain.exceptions import LangChainException

T = TypeVar('T')  # Generic type variable

class ObjectUtils:

    @staticmethod
    def pascal_to_snake(name):  
        # Convert PascalCase or camelCase to snake_case  
        s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', name)  
        return re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1).lower()  

    @staticmethod
    def translate_keys(obj):  
        if isinstance(obj, dict):  
            new_dict = {}  
            for key, value in obj.items():  
                new_key = ObjectUtils.pascal_to_snake(key)  
                new_dict[new_key] = ObjectUtils.translate_keys(value)  # Recursively apply to values  
            return new_dict  
        elif isinstance(obj, list):  
            return [ObjectUtils.translate_keys(item) for item in obj]  # Apply to each item in the list  
        else:  
            return obj  # Return the item itself if it's not a dict or list

    @staticmethod
    def get_object_by_id(object_id: str, objects: dict, object_type: Type[T]) -> T:
        """
        Generic method to retrieve an object of a specified type from a dictionary by its ID.

        Args:
            object_id (str): The ID of the object to retrieve.
            objects (dict): A dictionary containing object data.
            object_type (Type[T]): The type of the object to construct.

        Returns:
            T: An instance of the specified type.

        Raises:
            LangChainException: If the object ID is invalid or the object cannot be constructed.
        """
        if not object_id:
            raise LangChainException("Invalid object ID.", 400)

        object_data = objects.get(object_id)
        if not object_data:
            raise LangChainException(f"Object with ID '{object_id}' not found in the dictionary.", 400)

        try:
            return object_type(**object_data)
        except Exception as e:
            raise LangChainException(f"Failed to construct object of type '{object_type.__name__}': {str(e)}", 400)
