using System;
using System.IO;
using System.Reflection;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad;
using SpecFlowToMarkdown.Infrastructure.Io;
using SpecFlowToMarkdown.Infrastructure.Mermaid;

Console
    .WriteLine("Starting SpecFlowToMarkdown console...");

if (args.Length < 2)
    throw new Exception("Expected 2 arguments");

var assemblyPath = args[0];
var outputPath = args[1];
var markdownAnchor = (string)null;

if (args.Length > 2)
{
    markdownAnchor = args[2];
}

if (string.IsNullOrEmpty(assemblyPath))
    throw new Exception("Assembly path argument invalid");

if (string.IsNullOrEmpty(outputPath))
    throw new Exception("Results path argument invalid");

Assembly assembly;
var isASnapshotAssembly = false;

try
{
    
}
catch (FileNotFoundException)
{
    throw new Exception($"Could not load assembly from {assemblyPath}");
}

TestExecution testExecution;

Console
    .WriteLine("ModelSnapshot type found in assembly... scanning...");

testExecution =
    FeatureAssemblyScanner
        .Perform(assemblyPath);

var result =
    MermaidRenderer
        .Perform(testExecution);

FileWriter
    .Perform(
        outputPath,
        result
    );

Console
    .WriteLine("SpecFlowToMarkdown operation completed");