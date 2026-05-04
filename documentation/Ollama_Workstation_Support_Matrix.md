# InsTK Ollama Workstation Support Matrix

## Purpose

This document defines the initial workstation policy for local Ollama usage in `InsTK.MauiClient`.

As of May 1, 2026, the pinned managed Ollama runtime version in the client is `0.22.1`.

The pinned official digest for `ollama-windows-amd64.zip` is:

* `93c38a2ae97e4ab55c6d324e9cf62bc79408de85861045c34f4294c774d00c34`

The goal is to standardize:

* which Ollama runtime model we support
* which grading models we support
* what minimum workstation expectations should be
* how the MAUI client should behave when Ollama is missing or mismatched

## V1 Runtime Strategy

InsTK v1 should use a local Ollama instance on the instructor workstation.

The MAUI client now:

* detect whether Ollama is installed
* detect whether the required InsTK-supported version is present
* offer managed runtime install or update if needed
* detect whether the required grading model is present
* offer managed model download if needed
* offer managed runtime startup against the configured local models path
* report runtime activity and download progress in the workstation UI
* detect and surface conflicting unmanaged Ollama instances

For v1, InsTK should treat Ollama as a managed local dependency.

## V1 Model Policy

### Primary Model

`qwen3-coder:30b`

Reason:

* strongest coding-oriented local default currently identified for InsTK grading
* large context window
* suitable for multi-file repository evaluation

Reference:

* `qwen3-coder:30b` is listed by Ollama at approximately `19 GB` with a `256K` context window
* Source: https://ollama.com/library/qwen3-coder

### Fallback Model

`deepseek-coder:6.7b`

Reason:

* significantly smaller local footprint
* more realistic for lower-resource instructor machines
* good fallback when the primary model is too slow or too heavy

Reference:

* `deepseek-coder:6.7b` is listed by Ollama at approximately `3.8 GB`
* Source: https://ollama.com/library/deepseek-coder

## V1 Supported Profiles

### Standard Grading Profile

* Ollama installed locally
* Primary grading model: `qwen3-coder:30b`
* Intended for instructors with stronger workstation hardware

### Low-Resource Grading Profile

* Ollama installed locally
* Fallback grading model: `deepseek-coder:6.7b`
* Intended for instructors with weaker hardware or reduced patience for large model startup times

## Workstation Expectations

### Storage

Storage is not expected to be the limiting factor for most instructor machines if disk space is managed intentionally.

Suggested planning assumptions:

* Ollama runtime and support files: several GB
* `qwen3-coder:30b`: about `19 GB`
* `deepseek-coder:6.7b`: about `3.8 GB`
* local repos, run artifacts, and reports: additional variable usage

Recommended storage policy:

* reserve a dedicated local models path
* reserve a dedicated grading workspace path
* keep the model path separate from cloned repositories and grading runs

### Minimum Practical Workstation

This profile should be considered the lower bound for acceptable local grading:

* Windows workstation
* sufficient disk space for Ollama plus at least the fallback model
* stable local filesystem access for cloned repositories and generated reports
* ability to run local Ollama API on `http://127.0.0.1:11434`

This minimum profile should default to:

* fallback model: `deepseek-coder:6.7b`

### Recommended Workstation

This profile should be the preferred instructor experience:

* Windows workstation
* ample local storage
* stronger CPU and/or GPU resources
* sufficient memory for larger local coding models

This recommended profile should default to:

* primary model: `qwen3-coder:30b`

## Managed Ollama Policy

For InsTK v1, the MAUI client should prefer an app-managed Ollama runtime instead of relying on whatever happens to be installed globally.

Implemented behavior:

* install Ollama into an InsTK-managed dependency folder
* keep models in a configurable shared models folder
* pin an InsTK-supported Ollama version
* pin the expected runtime archive SHA-256 digest
* verify the runtime version during client startup
* verify the runtime archive checksum during managed install
* verify model availability separately from runtime installation

## Install and Update Policy

### Runtime

The MAUI client supports:

1. Detect runtime presence
2. Detect runtime version
3. Compare to InsTK-supported version
4. Offer install or update when missing or incompatible

### Models

The MAUI client supports:

1. Detect installed local model tags
2. Check whether the selected InsTK grading model is available
3. Offer download for the missing model
4. Report model readiness explicitly in the workstation UI

## Suggested MAUI Client UI States

The workstation bootstrap screen should show:

* Ollama runtime status
* Ollama runtime version
* required InsTK runtime version
* current grading profile
* required primary model status
* fallback model status
* install/update/download actions

Example states:

* `Ollama missing`
* `Ollama installed but unsupported version`
* `Ollama ready, model missing`
* `Ollama ready, primary model installed`
* `Ollama ready, fallback model installed`
* `Ollama conflict detected`

## V1 Recommendation

Use the following v1 default policy:

* Runtime host: instructor workstation only
* Platform target: Windows only
* Primary model: `qwen3-coder:30b`
* Fallback model: `deepseek-coder:6.7b`
* Runtime management: app-guided managed local install
* Model management: app-guided download and verification

## Future Expansion

Potential future enhancements:

* workstation capability scoring to auto-select primary or fallback profile
* admin-configurable model policy from the InsTK web app
* model benchmark capture for grading latency and consistency
* hosted Ollama node support for centralized grading
