using System.Text;

namespace SpecFlowToMarkdown.Infrastructure.Io
{
    public interface IFileWriter
    {
        public void Perform(StringBuilder result, string filePath);
    }
}