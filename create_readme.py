from pathlib import Path

if __name__ == '__main__':
    readme = Path('readme_template.md').read_text()
    readme = readme.replace('@ RUST_EXAMPLE @', 
                            Path('pincushion/examples/readme.rs').resolve().read_text())
    readme = readme.replace('@ RUST_DOC @', Path('readme_rs.md').read_text())
    Path('README.md').write_text(readme)
