from pathlib import Path

if __name__ == '__main__':
    readme = Path('readme_template.md').read_text()
    readme = readme.replace('@ RUST_EXAMPLE @', 
                            Path('../pincushion/examples/readme.rs').resolve().read_text())
    readme_rs = Path('readme_rs_template.md').read_text()
    readme_rs = readme_rs.replace('@ BENCHMARKS @', Path('benchmark.txt').read_text())
    Path('readme_rs.md').write_text(readme_rs)
    readme = readme.replace('@ RUST_DOC @', readme_rs)
  
    Path('../README.md').write_text(readme)
