import json
from typing import Optional
from foundationallm.context import UserIdentity

class Context:
    def __init__(self, user_identity: Optional[UserIdentity] = None):
        """Init"""
        if (user_identity!=None):
            self.user_identity = json.loads(user_identity, object_hook=UserIdentity.from_json) or None
        else:
            self.user_identity = None
        
    def get_upn(self) -> str:
        if (self.user_identity!=None):
            return self.user_identity.upn or ''
        else:
            return ''