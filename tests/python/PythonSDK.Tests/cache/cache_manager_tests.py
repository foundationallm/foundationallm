from datetime import datetime
import os
import sys
import pandas as pd

from foundationallm.cache import CacheManager
from foundationallm.storage import LocalStorageManager

local_storage_manager = LocalStorageManager('./.cache')
cache_manager = CacheManager(layer_one_storage_manager=local_storage_manager, layer_two_storage_manager=None)