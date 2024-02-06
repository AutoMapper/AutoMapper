# Configuration file for the Sphinx documentation builder.

# -- Project information

project = 'AutoMapper'
copyright = '2024, Jimmy Bogard'
author = 'Jimmy Bogard'

# -- General configuration

extensions = [
    'sphinx.ext.duration',
    'sphinx.ext.doctest',
    'sphinx.ext.autodoc',
    'sphinx.ext.autosummary',
    'sphinx.ext.intersphinx',
    'myst_parser'
]

intersphinx_mapping = {
    'python': ('https://docs.python.org/3/', None),
    'sphinx': ('https://www.sphinx-doc.org/en/master/', None),
}
intersphinx_disabled_domains = ['std']

templates_path = ['_templates']

# -- Options for HTML output

html_theme = 'sphinx_rtd_theme'
html_theme_options = {
    'logo_only': True,
    'display_version': False
}
html_logo = 'img/logo.png'


# -- Options for EPUB output
epub_show_urls = 'footnote'