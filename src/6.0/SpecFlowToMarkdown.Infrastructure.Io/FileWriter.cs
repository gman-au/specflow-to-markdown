using System;
using System.IO;
using System.Text;

namespace SpecFlowToMarkdown.Infrastructure.Io
{
    public static class FileWriter
    {
        public static void Perform(
            string filePath,
            StringBuilder result
        )
        {
            var exists =
                File
                    .Exists(filePath);

            if (!exists)
            {
                Console
                    .WriteLine($"Existing file \"{filePath}\" not found, creating...");

                OverwriteFileContents(
                    filePath,
                    result.ToString()
                );
                return;
            }

            var fileString =
                File
                    .ReadAllText(filePath);

            OverwriteFileContents(
                filePath,
                result.ToString()
            );
        }

        private static void OverwriteFileContents(string filePath, string result) =>
            File
                .WriteAllText(
                    filePath,
                    result
                );
    }
}