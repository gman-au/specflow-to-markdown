using System;
using System.IO;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad;
using SpecFlowToMarkdown.Infrastructure.Io;
using SpecFlowToMarkdown.Infrastructure.Markdown;
using SpecFlowToMarkdown.Infrastructure.Parsing;

Console
    .WriteLine("Starting SpecFlowToMarkdown console...");

if (args.Length < 3)
    throw new Exception("Expected 3 arguments");

var assemblyPath = args[0];
var executionResultsPath = args[1];
var outputPath = args[2];

if (string.IsNullOrEmpty(assemblyPath))
    throw new Exception("Assembly path argument invalid");

if (string.IsNullOrEmpty(executionResultsPath))
    throw new Exception("Results path argument invalid");

if (string.IsNullOrEmpty(outputPath))
    throw new Exception("Output path argument invalid");

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
    MarkdownRenderer
        .Perform(specFlowAssembly, testExecution);

FileWriter
    .Perform(
        outputPath,
        result
    );

Console
    .WriteLine("SpecFlowToMarkdown operation completed");