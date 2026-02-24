# Documentation Deployment Guide

This guide explains how to deploy Lean WebUI documentation to various platforms.

## Overview

Lean WebUI documentation is available in the following formats:
- **Markdown files** in `/WebUI/docs/` directory
- **GitHub Pages** (recommended)
- **Static website** (custom hosting)
- **GitBook** or **Docusaurus** (optional)

---

## Option 1: GitHub Pages (Recommended)

### Setup

1. **Enable GitHub Pages** for the repository:
   - Go to repository Settings → Pages
   - Source: "Deploy from a branch"
   - Branch: `gh-pages` or `main`, folder: `/docs` or `/WebUI/docs`

2. **Create documentation index** (if not exists):

```bash
cd WebUI/docs
cat > index.md << 'EOF'
# Lean WebUI Documentation

Welcome to the Lean WebUI documentation!

## Quick Links

### User Guides
- [中文用户指南](./WebUI用户指南.md)
- [English User Guide](./WebUI-User-Guide.md)

### Developer Guides
- [中文开发者指南](./WebUI开发者完整指南.md)
- [English Developer Guide](./WebUI-Developer-Guide.md)

### API Reference
- [API Reference Documentation](./WebUI-API-Reference.md)

### Setup & Configuration
- [IBKR Integration Guide](../IBKR_INTEGRATION_README.md)
- [Docker Deployment](../DOCKER_DEPLOYMENT.md)
- [HTTPS Setup](../HTTPS_SETUP.md)
- [Security Configuration](../SECURITY_CONFIGURATION.md)

### Operations
- [Operations Manual](../OPERATIONS_MANUAL.md)
- [Disaster Recovery](../DISASTER_RECOVERY_DRILL.md)
- [Production Checklist](../PRODUCTION_DEPLOYMENT_CHECKLIST.md)

### Other
- [Architecture Diagrams](./Architecture-Diagrams.md)
- [Changelog](../CHANGELOG.md)
- [Release Notes](../RELEASE_NOTES.md)

## Version

Current version: **v1.0.0**

Released: **February 19, 2026**
EOF
```

3. **Configure Jekyll** (optional, for better formatting):

Create `_config.yml` in `/WebUI/docs/`:

```yaml
title: Lean WebUI Documentation
description: Complete documentation for Lean WebUI
theme: jekyll-theme-cayman
markdown: kramdown
```

4. **Push changes**:

```bash
git add WebUI/docs/
git commit -m "Add documentation index for GitHub Pages"
git push origin main
```

5. **Access documentation**:
   - URL: `https://quantconnect.github.io/Lean/WebUI/docs/`
   - Or custom domain if configured

---

## Option 2: Static Website Hosting

### Build Static Site with MkDocs

1. **Install MkDocs**:

```bash
pip install mkdocs mkdocs-material
```

2. **Create `mkdocs.yml`** in `/WebUI/`:

```yaml
site_name: Lean WebUI Documentation
site_description: Complete documentation for Lean WebUI algorithmic trading platform
site_url: https://docs.yourdomain.com/lean-webui/

theme:
  name: material
  palette:
    primary: indigo
    accent: indigo
  language: en
  features:
    - navigation.tabs
    - navigation.sections
    - navigation.expand
    - search.suggest

nav:
  - Home: index.md
  - User Guides:
      - Installation (中文): docs/WebUI用户指南.md
      - Installation (English): docs/WebUI-User-Guide.md
  - Developer Guides:
      - Development (中文): docs/WebUI开发者完整指南.md
      - Development (English): docs/WebUI-Developer-Guide.md
  - API Reference: docs/WebUI-API-Reference.md
  - Setup:
      - IBKR Integration: IBKR_INTEGRATION_README.md
      - Docker Deployment: DOCKER_DEPLOYMENT.md
      - HTTPS Setup: HTTPS_SETUP.md
  - Operations:
      - Operations Manual: OPERATIONS_MANUAL.md
      - Disaster Recovery: DISASTER_RECOVERY_DRILL.md
  - About:
      - Changelog: CHANGELOG.md
      - Release Notes: RELEASE_NOTES.md

plugins:
  - search

markdown_extensions:
  - admonition
  - codehilite
  - toc:
      permalink: true
```

3. **Build documentation**:

```bash
cd WebUI
mkdocs build
```

Output will be in `/WebUI/site/`

4. **Deploy to hosting**:

```bash
# Deploy to custom server
scp -r site/* user@server:/var/www/docs/

# Or deploy to GitHub Pages
mkdocs gh-deploy

# Or deploy to Netlify/Vercel
# Just point to the 'site' directory
```

---

## Option 3: Docusaurus (Modern Documentation Platform)

1. **Install Docusaurus**:

```bash
cd WebUI
npx create-docusaurus@latest docs-site classic
```

2. **Copy documentation** to `/WebUI/docs-site/docs/`

3. **Configure** `docusaurus.config.js`

4. **Build and deploy**:

```bash
cd docs-site
npm run build
npm run serve  # Local preview
```

---

## Option 4: ReadTheDocs

1. **Create `.readthedocs.yml`** in repository root:

```yaml
version: 2

build:
  os: ubuntu-22.04
  tools:
    python: "3.11"

sphinx:
  configuration: WebUI/docs/conf.py

formats:
  - pdf
  - epub

python:
  install:
    - requirements: WebUI/docs/requirements.txt
```

2. **Create Sphinx configuration** in `/WebUI/docs/conf.py`

3. **Link repository** to ReadTheDocs.org

---

## Deployment Checklist

Before deploying documentation:

- [ ] All documentation files reviewed and finalized
- [ ] Screenshots and diagrams up to date
- [ ] Links tested (no broken links)
- [ ] Version numbers updated (v1.0.0)
- [ ] Release date updated
- [ ] Code examples tested
- [ ] Translations reviewed (Chinese and English)
- [ ] CHANGELOG.md included
- [ ] RELEASE_NOTES.md included
- [ ] API reference complete
- [ ] Table of contents accurate

---

## Update Documentation for Future Releases

### For v1.1.0 and beyond:

1. **Create new documentation version** (for versioned docs):

```bash
# MkDocs with mike
pip install mike
mike deploy v1.0.0 stable
mike deploy v1.1.0 latest
```

2. **Update version references**:
   - Update version in all docs
   - Add changelog entry
   - Update API reference

3. **Archive old versions**:
   - Keep v1.0.0 documentation accessible
   - Add version selector (if using Docusaurus/MkDocs Material)

---

## Monitoring

After deployment, monitor:
- Documentation site uptime
- User feedback on documentation quality
- Search queries (identify missing topics)
- Page view analytics

---

## Maintenance

Regular documentation maintenance tasks:
- **Monthly**: Review and update screenshots
- **Per Release**: Update version numbers, changelog
- **Quarterly**: Review all content for accuracy
- **Annually**: Major documentation refresh

---

## Internal Documentation Repository

For internal development documentation (not public):

Create a separate private repository or use:
- Confluence
- Notion
- GitHub Wiki (private repository)
- GitBook (private workspace)

---

## Contact

For documentation issues:
- **GitHub Issues**: Report documentation bugs
- **Pull Requests**: Contribute documentation improvements
- **Discord**: Ask documentation questions (coming soon)

---

**Last Updated**: February 19, 2026  
**Version**: 1.0.0
