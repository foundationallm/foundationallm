import zipfile
import os
import re
from argparse import ArgumentParser
from pathlib import Path

package_name = 'foundationallm_agent_plugins'

# Using the official Semantic Versioning regex from https://semver.org/ (https://regex101.com/r/Ly7O1x/3/)
SEMVER_REGEX = '^(?P<major>0|[1-9]\d*)\.(?P<minor>0|[1-9]\d*)\.(?P<patch>0|[1-9]\d*)(?:-(?P<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?P<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$'
semver_validator = re.compile(SEMVER_REGEX)

def version(value):
    '''Check if the value is a valid Semantic Version.'''
    if not semver_validator.match(value):
        raise ValueError(f'{value} is not a valid Semantic Version')
    return value

parser = ArgumentParser()
parser.add_argument('-v', '--version', help='The version to package', dest='version', required=True, type=version)
parser.add_argument('-c', '--config', choices=['Release', 'Debug'], help='The configuration to package (Debug or Release)', dest='config', required=True)
args = parser.parse_args()

print('Current working directory:', os.getcwd())
print('Version:', args.version)
print('Configuration:', args.config)

manifest_path = Path('package/foundationallm_manifest.json')
manifest_text = manifest_path.read_text(encoding='utf-8')
updated_manifest = manifest_text.replace('__VERSION__', args.version)

package_manifest_path = Path(f'src/{package_name}/foundationallm_manifest.json')
package_manifest_path.write_text(updated_manifest, encoding='utf-8')
print(f'Package manifest: {package_manifest_path}')

package_root = Path(f'src/{package_name}')

if args.config == 'Debug':
    with zipfile.ZipFile(f'{package_name}_debug-{args.version}.zip', mode='w') as zip_pkg:
        for f in package_root.rglob('*.py'):
            print(f'Adding {f} to the package...')
            zip_pkg.write(f, f.relative_to('src'))
        zip_pkg.write(package_manifest_path, package_manifest_path.relative_to('src'))
else:
    with zipfile.PyZipFile(f'{package_name}-{args.version}.zip', mode='w') as zip_pkg:
        zip_pkg.writepy(str(package_root))
        zip_pkg.write(package_manifest_path, package_manifest_path.relative_to('src'))

package_manifest_path.unlink()
print('Package manifest removed.')
