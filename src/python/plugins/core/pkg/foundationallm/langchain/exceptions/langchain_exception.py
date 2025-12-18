class LangChainException(Exception):
    def __init__(self, message, code=400):
        self.message = message
        self.code = code

    def __str__(self):
        return self.message
