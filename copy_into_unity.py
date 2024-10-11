from shutil import copy
from subprocess import run
from os import getcwd, chdir
from pathlib import Path
from hashlib import sha256

"""
Create and copy files into the UnityExample project.
"""

def hash_file(path: Path) -> str: 
    sha = sha256()
    with path.open('rb') as f:
        while True:
            data = f.read(4096)
            if not data:
                break
            sha.update(data)
    return sha.hexdigest()

if __name__ == "__main__":
    rust_root = 'pincushion'
    src = f'{rust_root}/target/release/{rust_root}.dll'
    unity_root = 'UnityExample/Assets/Pincushion/'
    dst = f'{unity_root}{rust_root}.dll'

    # Hash the source and destination Rust libraries before building.
    src_hash = hash_file(Path(src).resolve())
    dst_hash = hash_file(Path(dst).resolve())
    
    # Create the native Rust library.
    if src_hash != dst_hash:
        cwd = getcwd()
        chdir(rust_root)
        run(['cargo', 'build', '--release'])
        chdir(cwd)
        # Copy the library.
        try:
            copy(src, dst)
        except PermissionError:
            print("Failed to copy native library, probably because Unity is using it.")

    # Copy the C# and shader files.
    cs_root = Path('PincushionCs/PincushionCs').resolve()
    suffixes = ['.cs', '.shader']
    for src in cs_root.iterdir():
        if not src.is_file() or src.suffix not in suffixes:
            continue
        dst = f'{unity_root}{src.name}'
        copy(src.as_posix(), dst)
