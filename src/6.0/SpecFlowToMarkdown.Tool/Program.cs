using System;
using Microsoft.Extensions.DependencyInjection;
using SpecFlowToMarkdown.Application;
using SpecFlowToMarkdown.Tool;

var services = 
    Startup
        .AddServices();

var serviceProvider =
    services
        .BuildServiceProvider();

var application = 
    serviceProvider
        .GetRequiredService<ISpecFlowApplication>();

application
    .Perform(args);

Console
    .WriteLine("Starting SpecFlowToMarkdown console...");

return;

/*
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
        .Perform(
            specFlowAssembly,
            testExecution
        );

FileWriter
    .Perform(
        outputPath,
        result
    );

Console
    .WriteLine("SpecFlowToMarkdown operation completed");*/