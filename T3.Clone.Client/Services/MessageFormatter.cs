namespace T3.Clone.Client.Models;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MessageFormatter
{
    public enum SegmentType
    {
        Markdown,
        CodeBlock,
        Latex
    }

    public class Segment
    {
        public SegmentType Type { get; set; }

        public string Content { get; set; }

        // For code blocks, this holds the language if provided; otherwise null
        public string? Language { get; set; }
    }

    // Regex to find LaTeX expressions (both inline and block)
    private static readonly Regex LatexRegex = new Regex(
        @"(?:\$\$([^$]+?)\$\$|\$([^$]+?)\$|\\begin\{([^}]+)\}(.*?)\\end\{\3\})",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    public List<Segment> SplitIntoSegments(string message)
    {
        var segments = new List<Segment>();
        if (string.IsNullOrEmpty(message))
        {
            return segments;
        }

        // First, handle code blocks
        var codeBlockSegments = SplitCodeBlocks(message);
        
        // Then, for each non-code segment, check for LaTeX
        foreach (var segment in codeBlockSegments)
        {
            if (segment.Type == SegmentType.CodeBlock)
            {
                segments.Add(segment);
            }
            else
            {
                // Process this markdown segment for LaTeX
                var latexSegments = SplitLatexInMarkdown(segment.Content);
                segments.AddRange(latexSegments);
            }
        }

        return segments;
    }

    private List<Segment> SplitCodeBlocks(string message)
    {
        var segments = new List<Segment>();
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

    private List<Segment> SplitLatexInMarkdown(string markdownContent)
    {
        var segments = new List<Segment>();
        if (string.IsNullOrEmpty(markdownContent))
        {
            return segments;
        }

        var matches = LatexRegex.Matches(markdownContent);
        if (matches.Count == 0)
        {
            // No LaTeX found, return as single markdown segment
            segments.Add(new Segment
            {
                Type = SegmentType.Markdown,
                Content = markdownContent
            });
            return segments;
        }

        int lastIndex = 0;
        foreach (Match match in matches)
        {
            // Add markdown content before this LaTeX match
            if (match.Index > lastIndex)
            {
                var beforeContent = markdownContent.Substring(lastIndex, match.Index - lastIndex);
                if (!string.IsNullOrEmpty(beforeContent))
                {
                    segments.Add(new Segment
                    {
                        Type = SegmentType.Markdown,
                        Content = beforeContent
                    });
                }
            }

            // Add the LaTeX segment
            var latexContent = match.Groups[1].Success ? match.Groups[1].Value : // $$...$$
                              match.Groups[2].Success ? match.Groups[2].Value : // $...$
                              match.Groups[4].Value; // \begin{...}...\end{...}

            segments.Add(new Segment
            {
                Type = SegmentType.Latex,
                Content = latexContent
            });

            lastIndex = match.Index + match.Length;
        }

        // Add any remaining markdown content after the last LaTeX match
        if (lastIndex < markdownContent.Length)
        {
            var afterContent = markdownContent.Substring(lastIndex);
            if (!string.IsNullOrEmpty(afterContent))
            {
                segments.Add(new Segment
                {
                    Type = SegmentType.Markdown,
                    Content = afterContent
                });
            }
        }

        return segments;
    }
}