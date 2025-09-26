#!/bin/bash

# Determine the raw tag
RAW_TAG="${GITHUB_REF#refs/tags/}"
echo "The raw tag is: '$RAW_TAG'"

PACKAGE_PLATFORM=""

# Extract version from branch name (format: 1.0.0 or 1.0.0-alpha1 or 1.0.0-beta1 or 1.0.0-rc1)
if [[ "$RAW_TAG" =~ ^((nuget|pypi)-)?([0-9]+\.[0-9]+\.[0-9]+(?:-(?:alpha|beta|rc)[1-9][0-9]{2})?)$ ]]; then
  VERSION="${BASH_REMATCH[3]}"
  PACKAGE_PLATFORM="${BASH_REMATCH[2]}"
else
  VERSION="0.0.0"
fi

if [ "$VERSION" = "0.0.0" ]; then
  echo "Error: could not infer a valid version."
  exit 1
fi

# Convert .NET versioning to Python versioning
PYTHON_VERSION="$VERSION"
if [[ "$VERSION" =~ ([0-9]+\.[0-9]+\.[0-9]+)-alpha([0-9]+)$ ]]; then
  PYTHON_VERSION="${BASH_REMATCH[1]}a${BASH_REMATCH[2]}"
elif [[ "$VERSION" =~ ([0-9]+\.[0-9]+\.[0-9]+)-beta([0-9]+)$ ]]; then
  PYTHON_VERSION="${BASH_REMATCH[1]}b${BASH_REMATCH[2]}"
elif [[ "$VERSION" =~ ([0-9]+\.[0-9]+\.[0-9]+)-rc([0-9]+)$ ]]; then
  PYTHON_VERSION="${BASH_REMATCH[1]}rc${BASH_REMATCH[2]}"
fi

# Output versions for GitHub Actions
echo "dotnet_version=$VERSION" >> "$GITHUB_OUTPUT"
echo "python_version=$PYTHON_VERSION" >> "$GITHUB_OUTPUT"
echo "package_platform=$PACKAGE_PLATFORM" >> "$GITHUB_OUTPUT"

echo "NuGet Version: $VERSION"
echo "Python Version: $PYTHON_VERSION"
echo "Package Platform: $PACKAGE_PLATFORM"

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
