using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using Gherkin.Ast;
using LivingDoc.SpecFlowPlugin;
using Microsoft.Extensions.DependencyInjection;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad
{
    public static class FeatureAssemblyScanner
    {
        private const string CustomFeatureAttributeValue = "TechTalk.SpecFlow";
        public static TestExecution Perform(string assemblyPath)
        {
            string path = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var allPaths = Directory.GetFiles(
                path,
                "*.dll"
            ).ToList();
            allPaths.Add(assemblyPath);
            
            var results = new List<SpecFlowFeature>();
            var resolver = new PathAssemblyResolver(allPaths);
            // var resolver = new PathAssemblyResolver(new string[] {assemblyPath, typeof(object).Assembly.Location});
    
            using var mlc = new MetadataLoadContext(resolver, typeof(object).Assembly.GetName().ToString());

            // Load assembly into MetadataLoadContext
            // var assembly = mlc.LoadFromAssemblyPath(assemblyPath);
            
            var assembly =
                Assembly
                    .LoadFrom(assemblyPath);
            
            var featureTypes =
                assembly
                    .GetTypes()
                    .Where(t => 
                        t
                            .CustomAttributes
                            .Any(x => 
                                x
                                    .ConstructorArguments
                                    .Any(
                                        i => 
                                            i.Value != null && i.Value.ToString() == CustomFeatureAttributeValue)))
                    .ToList();
            
            foreach(var featureType in featureTypes)
            {
                var services = new ServiceCollection();
                services.AddTransient(featureType);
                var builder = services.BuildServiceProvider();

                
                var activated = builder.GetService(featureType);
                
                
                var f = Activator.CreateInstanceFrom(
                    assemblyPath,
                    featureType.FullName
                );
                var snapshot =
                    Activator
                        .CreateInstance
                            (featureType);
                
                results.Add(
                    new SpecFlowFeature
                    {
                        FeatureName = featureType.Name,
                        Namespace = featureType.Namespace
                    }
                );
                
                var fred = featureType.GetMethods();

                foreach (var d in fred)
                {
                }
                
                var custom = featureType.CustomAttributes;
                foreach (var customAttr in custom)
                {
                    var ff = customAttr.GetType();
                }
            }
            
            var result =
                new TestExecution
                {
                };

            return result;
        }
    }
}