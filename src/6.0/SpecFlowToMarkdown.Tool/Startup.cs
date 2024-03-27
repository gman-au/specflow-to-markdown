using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpecFlowToMarkdown.Application;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors;
using SpecFlowToMarkdown.Infrastructure.Io;
using SpecFlowToMarkdown.Infrastructure.Markdown;
using SpecFlowToMarkdown.Infrastructure.Parsing.Arguments;
using SpecFlowToMarkdown.Infrastructure.Parsing.Results;

namespace SpecFlowToMarkdown.Tool
{
    public static class Startup
    {
        public static IServiceCollection AddServices()
        {
            var services = new ServiceCollection();

            services
                .AddSingleton<ISpecFlowApplication, SpecFlowApplication>()
                .AddSingleton<IAssemblyScanner, AssemblyScanner>()
                .AddSingleton<IFeatureExtractor, FeatureExtractor>()
                .AddSingleton<IScenarioExtractor, ScenarioExtractor>()
                .AddSingleton<IFileWriter, FileWriter>()
                .AddSingleton<IResultSummariser, ResultSummariser>()
                .AddSingleton<IMarkdownRenderer, MarkdownRenderer>()
                .AddSingleton<IColourSorter, ColourSorter>()
                .AddSingleton<ITestExecutionParser, JsonTestExecutionParser>()
                .AddSingleton<IProgramArgumentsParser, ProgramArgumentsParser>();

            services
                .AddLogging(o => o.AddConsole());
            
            return services;
        } 
    }
}