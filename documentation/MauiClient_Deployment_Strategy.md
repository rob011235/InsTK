# InsTK MAUI Client Deployment Strategy

## Scope

This document describes the initial deployment strategy for `InsTK.MauiClient`.

The v1 target is:

* Windows only
* instructor workstation only
* manual install and manual update

## Why Windows-Only First

* The current development environment is Windows-based.
* .NET MAUI desktop builds can be produced and tested locally for Windows.
* The grading workflow depends on local repository cloning, filesystem access, and local Ollama execution.
* Cross-platform packaging adds complexity before the grading workflow itself is complete.

## Recommended v1 Distribution Model

Distribute the MAUI client from the InsTK web app as a downloadable Windows package.

Initial recommendation:

* publish a Windows build of `InsTK.MauiClient`
* expose the current downloadable package in the web app
* show version number and release date in the web app
* require manual update by downloading a newer package

## Packaging Direction

Short-term acceptable options:

* self-contained published folder distributed as a zip
* MSIX package
* unpackaged Windows executable distribution

Preferred evaluation path:

1. Start with a simple downloadable Windows publish artifact.
2. Add installer packaging once the grading workflow is stable.
3. Consider auto-update only after desktop authentication, local settings, and grading execution are stable.

## Web App Responsibilities

The web app should eventually provide:

* current MAUI client version metadata
* download link for the current Windows package
* release notes or deployment notes for instructors

## MAUI Client Responsibilities

The desktop app should eventually provide:

* visible app version
* backend environment indicator
* local workspace path configuration
* Ollama endpoint or local runtime status

## Not in v1

* Mac packaging
* mobile targets
* automatic background update service
* Microsoft Store distribution
