using SpecFlowToMarkdown.Domain;

namespace SpecFlowToMarkdown.Infrastructure.Markdown.Extensions
{
    internal static class StringEx
    {
        private const string StatusOk = "OK";
        private const string StatusError = "TestError";
        
        public static TestStatusEnum ToStatusEnum(this string value)
        {
            switch (value)
            {
                case StatusOk: return TestStatusEnum.Success;
                case StatusError: return TestStatusEnum.Failure;
            }

            return TestStatusEnum.Other;
        }

        public static string ToAnchor(this string value, string icon = null)
        {
            var id =
                value
                    .ToLower()
                    .Trim()
                    .Replace(
                        " ",
                        "-"
                    )
                    .Replace(
                        ":",
                        ""
                    );

            var iconString = string.Empty;

            if (icon != null)
            {
                iconString = $"{icon}-";
            }
            
            return $"{iconString}{id}";
        }
        

        public static string ToStatusIcon(this TestStatusEnum result)
        {
            return result switch
            {
                TestStatusEnum.Success => IconReference.IconSuitePassed,
                TestStatusEnum.Failure => IconReference.IconSuiteFailed,
                _ => IconReference.IconSuiteSkipped
            };
        }
    }
}