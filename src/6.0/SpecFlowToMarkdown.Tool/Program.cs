using System;
using System.IO;
using System.Reflection;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad;
using SpecFlowToMarkdown.Infrastructure.Io;
using SpecFlowToMarkdown.Infrastructure.Mermaid;
using SpecFlowToMarkdown.Infrastructure.Parsing;

Console
    .WriteLine("Starting SpecFlowToMarkdown console...");

if (args.Length < 2)
    throw new Exception("Expected 2 arguments");

var assemblyPath = args[0];
var executionResultsPath = args[1];
var markdownAnchor = (string)null;

if (string.IsNullOrEmpty(assemblyPath))
    throw new Exception("Assembly path argument invalid");

if (string.IsNullOrEmpty(executionResultsPath))
    throw new Exception("Results path argument invalid");

try
{
}
catch (FileNotFoundException)
{
    throw new Exception($"Could not load assembly from {assemblyPath}");
}

Console
    .WriteLine("ModelSnapshot type found in assembly... scanning...");

var specFlowAssembly =
    AssemblyScanner
        .Perform(assemblyPath);

var testExecution =
    TestExecutionParser
        .Perform(executionResultsPath);

var result =
    MermaidRenderer
        .Perform(null);

FileWriter
    .Perform(
        executionResultsPath,
        result
    );

Console
    .WriteLine("SpecFlowToMarkdown operation completed");