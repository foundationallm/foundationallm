# FoundationaLLM Agent Plugins Experimental

This package provides the experimental FoundationaLLM agent plugins.

To package and publish the package run the following commands (in the folder where `pyproject.toml` is located):

```cmd
python -m pip install --upgrade build
python -m pip install --upgrade twine

python -m build
python -m twine upload dist/*
```