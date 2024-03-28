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