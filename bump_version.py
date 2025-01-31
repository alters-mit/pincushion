import re
import tomllib
import json
from argparse import ArgumentParser
from pathlib import Path
from packaging.version import Version

# Get the canonical version from Cargo.toml
cargo_toml_path = Path("pincushion/Cargo.toml").resolve()
cargo_toml = cargo_toml_path.read_text()
cargo_toml_data = tomllib.loads(cargo_toml)
version = Version(cargo_toml_data["package"]["version"])

parser = ArgumentParser()
parser.add_argument("bump", choices=["major", "minor", "micro"])

bump = parser.parse_args().bump
if bump == "major":
    version = Version(f"{version.major + 1}.0.0")
elif bump == "minor":
    version = Version(f"{version.major}.{version.minor + 1}.0")
else:
    version = Version(f"{version.major}.{version.minor}.{version.micro + 1}")

# Set Cargo.toml and Cargo.lock
re_cargo = re.compile(r'(name\s+=\s+"pincushion"\nversion\s+=\s+)"(.*?)"')
cargo_lock_path = Path("pincushion/Cargo.lock").resolve()
cargo_lock = cargo_lock_path.read_text()
for text, path in zip([cargo_toml, cargo_lock], [cargo_toml_path, cargo_lock_path]):
    text = re.sub(re_cargo, r'\1"' + str(version) + '"', text)
    path.write_text(text)

# Set the C# package.json
package_path = Path("com.mit.pincushion/package.json").resolve()
package = json.loads(package_path.read_text())
package["version"] = str(version)
package_path.write_text(json.dumps(package, indent=2))
