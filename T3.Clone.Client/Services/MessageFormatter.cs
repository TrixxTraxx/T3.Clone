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
    }

    public List<Segment> SplitIntoSegments(string message)
    {
        var segments = new List<Segment>();

        if (string.IsNullOrWhiteSpace(message))
            return segments; // return empty list if input is empty

        int currentIndex = 0;
        while (currentIndex < message.Length)
        {
            int codeBlockStart = message.IndexOf("```", currentIndex);

            if (codeBlockStart == -1)
            {
                // No more code blocks, remainder is normal markdown
                string markdownText = message.Substring(currentIndex);
                if (!string.IsNullOrEmpty(markdownText))
                {
                    segments.Add(new Segment
                    {
                        Type = SegmentType.Markdown,
                        Content = markdownText
                    });
                }

                break;
            }
            else
            {
                // Markdown segment before code block
                if (codeBlockStart > currentIndex)
                {
                    string markdownText = message.Substring(currentIndex, codeBlockStart - currentIndex);
                    if (!string.IsNullOrEmpty(markdownText))
                    {
                        segments.Add(new Segment
                        {
                            Type = SegmentType.Markdown,
                            Content = markdownText
                        });
                    }
                }

                // Find end of code block
                int codeBlockEnd = message.IndexOf("```", codeBlockStart + 3);
                if (codeBlockEnd == -1)
                {
                    // No closing backticks, treat rest as code block
                    string codeBlockContent = message.Substring(codeBlockStart + 3);
                    segments.Add(new Segment
                    {
                        Type = SegmentType.CodeBlock,
                        Content = codeBlockContent
                    });
                    break;
                }
                else
                {
                    string codeBlockContent = message.Substring(
                        codeBlockStart + 3,
                        codeBlockEnd - (codeBlockStart + 3)
                    );
                    segments.Add(new Segment
                    {
                        Type = SegmentType.CodeBlock,
                        Content = codeBlockContent
                    });
                    currentIndex = codeBlockEnd + 3;
                }
            }
        }

        return segments;
    }
}
