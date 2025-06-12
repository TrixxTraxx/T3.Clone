namespace T3.Clone.Client.Models;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MessageFormatter
{
    public enum SegmentType
    {
        Markdown,
        CodeBlock
    }

    public class Segment
    {
        public SegmentType Type { get; set; }

        public string Content { get; set; }

        // For code blocks, this holds the language if provided; otherwise null
        public string? Language { get; set; }
    }

    // Regex to find a code block and capture its language and content.
    // We use named capture groups "lang" and "content" for clarity.
    // RegexOptions.Singleline ensures that "." matches newline characters.
    private static readonly Regex CodeBlockRegex = new Regex(
        @"^```(?<lang>[^\n\r]*)[\r\n]?(?<content>.*)[\r\n]?```$",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    public List<Segment> SplitIntoSegments(string message)
    {
        var segments = new List<Segment>();
        if (string.IsNullOrEmpty(message))
        {
            return segments;
        }

        const string delimiter = "```";
        var parts = message.Split(
            new[] { delimiter },
            StringSplitOptions.None
        );

        // The loop increments by 2 because each code block is surrounded
        // by Markdown parts. 'isCodeBlock' toggles state.
        bool isCodeBlock = message.StartsWith(delimiter);

        for (int i = 0; i < parts.Length; i++)
        {
            string currentPart = parts[i];
            if (string.IsNullOrEmpty(currentPart))
            {
                // If a part is empty, we might just be toggling state.
                // e.g., message starts or ends with ```
                isCodeBlock = !isCodeBlock;
                continue;
            }

            if (isCodeBlock)
            {
                // This part is a code block. The language is the first line.
                var firstNewLine = currentPart.IndexOf('\n');
                string language = string.Empty;
                string content = currentPart;

                if (firstNewLine != -1)
                {
                    language = currentPart.Substring(0, firstNewLine)
                        .Trim();
                    content = currentPart.Substring(firstNewLine + 1);
                }

                segments.Add(
                    new Segment
                    {
                        Type = SegmentType.CodeBlock,
                        Language = string.IsNullOrWhiteSpace(language)
                            ? null
                            : language,
                        Content = content
                    }
                );
            }
            else
            {
                // This part is standard Markdown text.
                segments.Add(
                    new Segment
                    {
                        Type = SegmentType.Markdown,
                        Content = currentPart
                    }
                );
            }

            // Toggle for the next part
            isCodeBlock = !isCodeBlock;
        }

        return segments;
    }
}