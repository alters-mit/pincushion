from shutil import copy
from subprocess import run
from os import getcwd, chdir
from pathlib import Path

"""
Create and copy files into the UnityExample project.
"""

if __name__ == "__main__":
    rust_root = 'pincushion'

    # Create the library.
    cwd = getcwd()
    chdir(rust_root)
    run(['cargo', 'build', '--release'])
    chdir(cwd)

    # Copy the library.
    src = f'{rust_root}/target/release/{rust_root}.dll'
    dst = f'com.mit.pincushion/Runtime/{rust_root}.dll'
    copy(src, dst)
