# SpecFlowToMarkdown

[![nuget](https://github.com/gman-au/specflow-to-markdown/actions/workflows/nuget.yml/badge.svg)](https://github.com/gman-au/specflow-to-markdown/actions/workflows/nuget.yml)

![GitHub Release](https://img.shields.io/github/v/release/gman-au/specflow-to-markdown)

## Summary
It is a simple command line tool that can be installed from NuGet.
It is a modernised version of the [SpecFlow LivingDoc](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/) engine which takes a test assembly and a set of SpecFlow test results, and renders them in a summarised and presentable format.

### [See here for a repository of example implementations](https://github.com/gman-au/specflow-to-markdown-sample)

## Usage
### Installation
You can install the `specflow-to-markdown` tool via the following .NET command
```
dotnet tool install -g Gman.SpecFlowToMarkdown
```
### Running the tool
The tool takes five arguments:
```
specflow-to-markdown <PATH_TO_TEST_ASSEMBLY> <TEST_ASSEMBLY_FILE> <PATH_TO_TEST_RESULTS_FILE> <TEST_RESULTS_FILE> <PATH_TO_OUTPUT_FILE>
```
- `PATH_TO_TEST_ASSEMBLY` - this will be the location of the built .NET DLL containing the SpecFlow tests.
- `TEST_ASSEMBLY_FILE` - this will be the name of the assembly file. Supports wildcards.
- `PATH_TO_TEST_RESULTS_FILE` - this will be the location of the (JSON) test execution results file.
- `TEST_RESULTS_FILE` - this will be the name of the results file. Supports wildcards.
- `PATH_TO_OUTPUT_FILE` - this will be the path to the generated output file where the markdown should be generated; includes the full file name. The file _does not have to be a markdown_ (`.md`) file.

The above arguments are specified separately; in cases where a path to a given set of results or assemblies may *not be exactly known* at build time (i.e. CI/CD pipelines), the **first** file found matching the name in the given folder will be provided to the tool.

### Running in a GitHub Actions Workflow
You can install the dotnet tool globally in the build agent, run the (SpecFlow) tests, and then run this tool afterwards to generate the (markdown) file on the build agent, which can be used by further action steps.
[See here for example implmentations](https://github.com/gman-au/specflow-to-markdown-sample), as well as the output markdown test results (taking the form of GitHub [action checks](https://github.com/marketplace/actions/github-checks)).