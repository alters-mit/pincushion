import re
from pathlib import Path

if __name__ == '__main__':
    readme = Path('readme_template.md').read_text()
    # Add the rust example code to the template.
    readme = readme.replace('@ RUST_EXAMPLE @', 
                            Path('../pincushion/examples/readme.rs').resolve().read_text())
    # Add the overview to the template.
    readme = readme.replace('@ OVERVIEW @', 
                            Path('overview.md').read_text())
    # Create the Rust doc.
    readme_rs = Path('readme_rs_template.md').read_text()
    readme_rs = readme_rs.replace('@ BENCHMARKS @', Path('benchmark.txt').read_text())
    Path('readme_rs.md').write_text(readme_rs)
    # Add the Rust doc to the main README.
    readme = readme.replace('@ RUST_DOC @', readme_rs)
  
    Path('../README.md').write_text(readme)

    # Remove images for the package README.
    readme_package = re.sub(r'(\n!\[.*?\]\(.*?\)\n)', '', readme)
    Path('../com.mit.pincushion/README.md').resolve().write_text(readme_package)
