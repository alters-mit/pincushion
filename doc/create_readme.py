import re
from pathlib import Path
from shutil import copy

if __name__ == '__main__':
    root_dir = Path(__file__).parent
    rs_dir = root_dir.parent.joinpath('pincushion')
    readme = root_dir.joinpath('readme_template.md').read_text()
    # Add the rust example code to the template.
    readme = readme.replace('@ RUST_EXAMPLE @', 
                            rs_dir.joinpath('examples/readme.rs').resolve().read_text())
    overview_path = root_dir.joinpath('overview.md')
    copy(overview_path.as_posix(), rs_dir.joinpath("doc/overview.md").resolve().as_posix())
    overview = overview_path.read_text()
    # Add the overview to the template.
    readme = readme.replace('@ OVERVIEW @', overview)
    # Create the Rust doc.
    readme_rs = root_dir.joinpath('readme_rs_template.md').read_text()
    rs_dir.joinpath('doc/README.md').resolve().write_text(readme_rs)
    # Add the Rust doc to the main README.
    readme = readme.replace('@ RUST_DOC @', readme_rs)
  
    root_dir.parent.joinpath('README.md').write_text(readme)

    readme_crates_io = rs_dir.joinpath('doc/header.md').resolve().read_text() + '\n\n'
    readme_crates_io += overview + '\n\n'
    readme_crates_io += rs_dir.joinpath('doc/unity.md').resolve().read_text()
    readme_crates_io += '\n\n```rust\n' + rs_dir.joinpath('examples/readme.rs').read_text() + '```\n\n'
    readme_crates_io += readme_rs
    
    
    rs_dir.joinpath('README.md').write_text(readme_crates_io)

    # Remove images for the package README.
    readme_package = re.sub(r'(\n!\[.*?\]\(.*?\)\n)', '', readme)
    root_dir.parent.joinpath('com.mit.pincushion/README.md').resolve().write_text(readme_package)
