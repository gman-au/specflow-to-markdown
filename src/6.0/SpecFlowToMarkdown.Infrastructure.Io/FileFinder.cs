using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SpecFlowToMarkdown.Infrastructure.Io
{
    public class FileFinder : IFileFinder
    {
        private readonly ILogger<FileFinder> _logger;

        public FileFinder(ILogger<FileFinder> logger)
        {
            _logger = logger;
        }

        public string GetFirstFound(string pathName, string fileName)
        {
            var foundFilePath =
                Directory
                    .EnumerateFiles(
                        pathName,
                        fileName,
                        SearchOption.AllDirectories
                    )
                    .FirstOrDefault();
            
            if (foundFilePath == null)
                throw new Exception($"Could not find file matching path {pathName} and file {fileName}");

            _logger
                .LogInformation($"Found file path at {foundFilePath}");

            return foundFilePath;
        }
    }
}