import json
import datetime as _dt
import decimal as _dec
import uuid as _uuid

try:
    import numpy as _np
    try:
        # NumPy 2.x style dtypes
        from numpy.dtypes import ObjectDType as _ObjectDType
    except ImportError:
        _ObjectDType = None
except ImportError:
    _np = None
    _ObjectDType = None

try:
    import pandas as _pd
except ImportError:
    _pd = None


class ExtendedJSONEncoder(json.JSONEncoder):
    """
    JSON encoder that knows how to handle:
      - datetime, date, time
      - Decimal
      - UUID
      - set, frozenset
      - bytes, bytearray
      - NumPy: ndarray, generic scalars, numbers, dtypes (incl. ObjectDType)
      - pandas: Series, DataFrame, Index, Timestamp, NA/NaT
    """

    def default(self, obj):
        # --- datetime, date, time ---
        if isinstance(obj, (_dt.datetime, _dt.date, _dt.time)):
            return obj.isoformat()

        # --- Decimal ---
        if isinstance(obj, _dec.Decimal):
            return float(obj)

        # --- UUID ---
        if isinstance(obj, _uuid.UUID):
            return str(obj)

        # --- sets ---
        if isinstance(obj, (set, frozenset)):
            return list(obj)

        # --- bytes ---
        if isinstance(obj, (bytes, bytearray)):
            return obj.decode("utf-8", errors="replace")

        # --- NumPy types ---
        if _np is not None:
            # Handle NumPy dtypes, including ObjectDType
            if isinstance(obj, _np.dtype):
                # Special-case object dtype
                if obj == _np.dtype("O"):
                    # or return str(obj) if you prefer "object"
                    return "object"
                # for other dtypes, serialize the canonical string
                return obj.str or str(obj)

            # Handle ObjectDType *class* (NumPy 2.x)
            if _ObjectDType is not None and isinstance(obj, _ObjectDType):
                # Again, choose whatever representation you like:
                return "object"

            # NumPy scalars (np.int64, np.float32, etc.)
            if isinstance(obj, _np.generic):
                return obj.item()

            # NumPy arrays
            if isinstance(obj, _np.ndarray):
                return obj.tolist()

        # --- pandas types ---
        if _pd is not None:
            # Timestamp, NaT
            if isinstance(obj, _pd.Timestamp):
                if obj is _pd.NaT:
                    return None
                return obj.isoformat()

            # pandas NA scalar
            if obj is _pd.NA:
                return None

            # Series -> dict
            if isinstance(obj, _pd.Series):
                return obj.to_dict()

            # Index -> list
            if isinstance(obj, _pd.Index):
                return obj.tolist()

            # DataFrame -> records
            if isinstance(obj, _pd.DataFrame):
                return obj.to_dict(orient="records")

        # Fallback
        return super().default(obj)