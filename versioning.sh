#!/bin/bash

# Determine the raw tag
RAW_TAG="${GITHUB_REF#refs/tags/}"
echo "The raw tag is: '$RAW_TAG'"

# Extract version from branch name (format: 1.0.0 or 1.0.0-alpha1 or 1.0.0-beta1
# or 1.0.0-rc1 or 1.0.0-poc1 or 1.0.0-base1)
# Build the regex in readable pieces so it's easier to maintain.
VERSION_REGEX='^(nuget-|pypi-|context_api-|user_portal-|core_api-)?'
VERSION_REGEX+='([0-9]+\.[0-9]+\.[0-9]+'
# allow optional pre-release like -rc485 or -rc485.1 (numeric, optional ".<num>")
VERSION_REGEX+='(-(alpha|beta|rc|poc|base)[1-9]+(\.[0-9]+)?)?'
VERSION_REGEX+=')$'

if [[ "$RAW_TAG" =~ $VERSION_REGEX ]]; then
  VERSION="${BASH_REMATCH[2]}"
else
  VERSION="0.0.0"
fi

if [ "$VERSION" = "0.0.0" ]; then
  echo "Error: could not infer a valid version."
  exit 1
fi

# Convert .NET versioning to Python versioning
PYTHON_VERSION="$VERSION"
if [[ "$VERSION" =~ ([0-9]+\.[0-9]+\.[0-9]+)-alpha([1-9]+)(\.[0-9]+)?$ ]]; then
  PYTHON_VERSION="${BASH_REMATCH[1]}a${BASH_REMATCH[2]}${BASH_REMATCH[3]}"
elif [[ "$VERSION" =~ ([0-9]+\.[0-9]+\.[0-9]+)-beta([1-9]+)(\.[0-9]+)?$ ]]; then
  PYTHON_VERSION="${BASH_REMATCH[1]}b${BASH_REMATCH[2]}${BASH_REMATCH[3]}"
elif [[ "$VERSION" =~ ([0-9]+\.[0-9]+\.[0-9]+)-rc([1-9]+)(\.[0-9]+)?$ ]]; then
  PYTHON_VERSION="${BASH_REMATCH[1]}rc${BASH_REMATCH[2]}${BASH_REMATCH[3]}"
elif [[ "$VERSION" =~ ([0-9]+\.[0-9]+\.[0-9]+)-poc([1-9]+)(\.[0-9]+)?$ ]]; then
  PYTHON_VERSION="${BASH_REMATCH[1]}+poc${BASH_REMATCH[2]}${BASH_REMATCH[3]}"
elif [[ "$VERSION" =~ ([0-9]+\.[0-9]+\.[0-9]+)-base([1-9]+)(\.[0-9]+)?$ ]]; then
  PYTHON_VERSION="${BASH_REMATCH[1]}+base${BASH_REMATCH[2]}${BASH_REMATCH[3]}"
fi

# Output versions for GitHub Actions
echo "dotnet_version=$VERSION" >> "$GITHUB_OUTPUT"
echo "python_version=$PYTHON_VERSION" >> "$GITHUB_OUTPUT"

echo "NuGet Version: $VERSION"
echo "Python Version: $PYTHON_VERSION"

# Replace the version placeholders in the .csproj files for .NET projects
for csproj in $(find . -name '*.csproj'); do
  sed -i "s/<Version>0.0.0<\/Version>/<Version>$VERSION<\/Version>/" "$csproj"
  sed -i "s/<FileVersion>0.0.0<\/FileVersion>/<FileVersion>${VERSION%-*}<\/FileVersion>/" "$csproj"
  sed -i "s/<AssemblyVersion>0.0.0<\/AssemblyVersion>/<AssemblyVersion>${VERSION%-*}<\/AssemblyVersion>/" "$csproj"
done

# Replace the version in pyproject.toml files for Python projects
for toml in $(find . -name 'pyproject.toml'); do
  sed -i "s/^version = \".*\"/version = \"$PYTHON_VERSION\"/" "$toml"
done
