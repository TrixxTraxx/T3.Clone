using T3.Clone.Client.Components.MessageParts;

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
        public Guid Id { get; set; } = Guid.NewGuid();
        public SegmentType Type { get; set; } = SegmentType.Markdown;
        public string FullContent { get; set; } = "";
        public string Content { get; set; } = "";
        public string? Language { get; set; }
        public MessagePartComponent? Component { get; set; }
    }

    public List<Segment> Segments { get; private set; } = new List<Segment>();

    public void InitializeSegments(string content)
    {
        Segments.Clear();
        if (string.IsNullOrEmpty(content))
        {
            Segments.Add(new Segment());
            return;
        }

        Segments = SplitIntoSegments(content);
    }

    public List<Segment> AddTokenToSegments(string token)
    {
        var initialSegmentCount = Segments.Count;

        if (Segments.Count == 0)
        {
            Segments.Add(new Segment());
        }

        var lastSegment = Segments[^1];
        lastSegment.FullContent += token;

        // Re-parse just the last segment to handle boundaries
        var newSegments = SplitIntoSegments(lastSegment.FullContent);

        // Replace the last segment with the new segments
        Segments.RemoveAt(Segments.Count - 1);
        Segments.AddRange(newSegments);

        return Segments.Skip(Math.Max(0, initialSegmentCount - 1)).ToList();
    }

    private List<Segment> SplitIntoSegments(string message)
    {
        var segments = new List<Segment>();
        if (string.IsNullOrEmpty(message))
        {
            segments.Add(new Segment());
            return segments;
        }

        var currentSegment = new Segment();
        bool inCodeBlock = false;
        string? currentLanguage = null;

        int i = 0;
        while (i < message.Length)
        {
            // Check for ``` at current position
            if (i <= message.Length - 3 && message.Substring(i, 3) == "```")
            {
                if (!inCodeBlock)
                {
                    // Starting a code block
                    // Finish current markdown segment if it has content
                    if (currentSegment.FullContent.Length > 0)
                    {
                        currentSegment.Content = currentSegment.FullContent;
                        segments.Add(currentSegment);
                    }

                    // Start new code block segment
                    currentSegment = new Segment
                    {
                        Type = SegmentType.CodeBlock,
                        FullContent = "```"
                    };

                    i += 3; // Skip the ```
                    inCodeBlock = true;

                    // Parse language on the same line
                    int languageStart = i;
                    while (i < message.Length && message[i] != '\n')
                    {
                        currentSegment.FullContent += message[i];
                        i++;
                    }

                    if (i > languageStart)
                    {
                        string lang = message.Substring(languageStart, i - languageStart).Trim();
                        currentLanguage = string.IsNullOrEmpty(lang) ? null : lang;
                    }

                    currentSegment.Language = currentLanguage;

                    // Add the newline if present
                    if (i < message.Length && message[i] == '\n')
                    {
                        currentSegment.FullContent += message[i];
                        i++;
                    }
                }
                else
                {
                    // Ending a code block
                    currentSegment.FullContent += "```";
                    currentSegment.Language = currentLanguage;

                    // Extract content (everything between opening ``` and closing ```)
                    string fullContent = currentSegment.FullContent;
                    int firstNewline = fullContent.IndexOf('\n');
                    if (firstNewline >= 0 && fullContent.EndsWith("```"))
                    {
                        int contentStart = firstNewline + 1;
                        int contentEnd = fullContent.Length - 3;
                        currentSegment.Content = contentStart < contentEnd
                            ? fullContent.Substring(contentStart, contentEnd - contentStart)
                            : "";
                    }

                    segments.Add(currentSegment);

                    // Start new markdown segment
                    currentSegment = new Segment();
                    inCodeBlock = false;
                    currentLanguage = null;

                    i += 3; // Skip the ```
                }
            }
            else
            {
                // Regular character - add to current segment
                if (i < message.Length)
                {
                    currentSegment.FullContent += message[i];
                    i++;
                }
            }
        }

        // Add the final segment
        if (currentSegment.FullContent.Length > 0 || segments.Count == 0)
        {
            if (currentSegment.Type == SegmentType.Markdown)
            {
                currentSegment.Content = currentSegment.FullContent;
            }
            else if (currentSegment.Type == SegmentType.CodeBlock)
            {
                // Incomplete code block
                string fullContent = currentSegment.FullContent;
                int firstNewline = fullContent.IndexOf('\n');
                if (firstNewline >= 0)
                {
                    currentSegment.Content = fullContent.Substring(firstNewline + 1);
                }
                else
                {
                    currentSegment.Content = "";
                }

                currentSegment.Language = currentLanguage;
            }

            segments.Add(currentSegment);
        }

        return segments;
    }
}