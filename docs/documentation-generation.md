# Documentation generation

There are various layers of documentation for this project. This document describes the different layers and how they are generated.

## Documentation layers

The following layers of documentation are available for this project:

- Markdown files - These are the source files for FoundationaLLM documentation. They are located in the `docs` folder in the `main` branch of the repository.
- API documentation - This is the documentation for the FoundationaLLM API. Language-specific API documentation is generated using tools (more on tools below).
- [GitHub Pages](https://solliancenet.github.io/foundationallm/) - This is the primary documentation for the project. It is generated from the markdown files in the `docs` folder. It also includes the API documentation.

## Documentation tools

We use a number of tools to generate the documentation for this project. The following table lists the tools and their purpose:

| Tool | Purpose |
| --- | --- |
| [DocFX](https://dotnet.github.io/docfx/) | Generates the .NET API documentation for the FoundationaLLM API. It also generates the docs website, which includes custom documentation (markdown files) and the combined .NET and Python API documentation. |
| [Sphinx](https://www.sphinx-doc.org/en/master/) | Generates the Python API documentation for the FoundationaLLM API. |
| [Sphinx DocFX YAML](https://sphinx-docfx-yaml.readthedocs.io/) | Generates the DocFX YAML files for the Python API documentation. This allows the DocFx build process to incorporate the Python documentation as well. |
| [GitHub Pages](https://pages.github.com/) | Hosts the documentation website. It is configured to use the `gh-pages` branch of the repository as the source for the website. |

### DocFX

Execute the following command from the repository root to generate the .NET API documentation:

```bash
docfx docs/docfx.json
```

#### Previewing Documentation Locally

To preview the documentation locally in your browser, use the `--serve` flag:

```bash
docfx docs/docfx.json --serve
```

This command will:
1. Build the documentation (same as running `docfx docs/docfx.json`)
2. Start a local web server (typically at `http://localhost:8080`)
3. Open the documentation site in your default browser

**Note:** The `--serve` command builds the documentation once and serves the static files. If you make changes to markdown files:
- Run `docfx docs/docfx.json` in another terminal to rebuild, or restart the serve command
- Refresh your browser to see the updated content