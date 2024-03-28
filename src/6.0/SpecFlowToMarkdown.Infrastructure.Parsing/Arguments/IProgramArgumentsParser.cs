using SpecFlowToMarkdown.Domain;

namespace SpecFlowToMarkdown.Infrastructure.Parsing.Arguments
{
    public interface IProgramArgumentsParser
    {
        ProgramArguments Parse(string[] args);
    }
}