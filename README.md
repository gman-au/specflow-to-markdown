# SpecFlowToMarkdown

[![nuget](https://github.com/gman-au/specflow-to-markdown/actions/workflows/nuget.yml/badge.svg)](https://github.com/gman-au/specflow-to-markdown/actions/workflows/nuget.yml)

![GitHub Release](https://img.shields.io/github/v/release/gman-au/specflow-to-markdown)

## Summary
It is a simple command line tool that can be installed from NuGet.
It is a modernised version of the [SpecFlow LivingDoc](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/) engine which takes a test assembly and a set of SpecFlow test results, and renders them in a summarised and presentable format.

## Usage
### Installation
You can install the `specflow-to-markdown` tool via the following .NET command
```
dotnet tool install -g Gman.SpecFlowToMarkdown
```
### Running the tool
The tool takes three arguments:
```
specflow-to-markdown <PATH_TO_TEST_ASSEMBLY> <PATH_TO_TEST_RESULTS_FILE> <PATH_TO_OUTPUT_FILE>
```
- `PATH_TO_TEST_ASSEMBLY` - this will be the location of the built .NET DLL containing the SpecFlow tests.
- `PATH_TO_TEST_RESULTS_FILE` - this will be the location of the (JSON) test execution results file.
- `PATH_TO_OUTPUT_FILE` - this will be the path to the generated output file where the markdown should be generated; includes the full file name. The file _does not have to be a markdown_ (`.md`) file.