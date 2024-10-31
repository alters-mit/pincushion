from shutil import copy
from subprocess import run
from os import getcwd, chdir
from pathlib import Path

"""
Create and copy files into the UnityExample project.
"""

if __name__ == "__main__":
    rust_root = 'pincushion'
    src = f'{rust_root}/target/release/{rust_root}.dll'
    unity_root = 'UnityExample/Assets/Pincushion/'
    dst = f'{unity_root}{rust_root}.dll'

    cwd = getcwd()
    chdir(rust_root)
    run(['cargo', 'build', '--release'])
    chdir(cwd)
    # Copy the library.
    try:
        copy(src, dst)
    except PermissionError:
        print('Failed to copy native library, probably because Unity is using it.')

    # Copy the C# and shader files.
    cs_root = Path('PincushionCs/PincushionCs').resolve()
    shader_root = cs_root.joinpath('Shaders')
    resources_root = cs_root.joinpath('Resources')
    for src_root, unity_folder in zip([cs_root, shader_root, resources_root], ['', 'Shaders', 'Resources']):
        for src in src_root.iterdir():
            if not src.is_file() or src.suffix == '.config':
                continue
            dst = f'{unity_root}{unity_folder}/{src.name}'
            copy(src.as_posix(), dst)
