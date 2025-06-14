using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using T3.Clone.Client.Components.MessageParts;

namespace T3.Clone.Client.Models;

public class MessageFormatter
{
    public enum SegmentType
    {
        Markdown,
        CodeBlock
    }

    public class Segment
    {
        public Guid Id { get; } = Guid.NewGuid();
        public SegmentType Type { get; set; } = SegmentType.Markdown;
        public int StartIndex { get; set; } = 0;
        public int EndIndex { get; set; } = 0;
        public string FullContent { get; set; } = "";
        public string Content { get; set; } = "";
        public string? Language { get; set; }
        public MessagePartComponent? Component { get; set; }
    }

    public string fullContent { get; private set; } = "";
    public List<Segment> Segments { get; private set; } = new();

    public void InitializeSegments(string content)
    {
        fullContent = content;
        Segments.Clear();
        Segments.Add(new Segment { });
        SplitIntoSegments();
        Console.WriteLine("Updated Segments: " + JsonSerializer.Serialize(Segments.Select(x => x.Type).ToList()));
    }

    public List<Segment> AddTokenToSegments(string token)
    {
        fullContent += token;
        var initialSegmentCount = Segments.Count;
        if (Segments.Count == 0)
        {
            Segments.Add(new Segment { });
        }
        var lastSegment = Segments[^1];
        lastSegment.Content += token;

        SplitIntoSegments(token.Length);
        
        return Segments.Skip(initialSegmentCount -1).ToList();
    }

    private void SplitIntoSegments(int tokenLength = 0)
    {
        var initialLastSegment = Segments.Last();
        if (tokenLength == 0)
        {
            tokenLength = fullContent.Length;
        }
        var segmentStart = Math.Max(initialLastSegment.StartIndex, fullContent.Length - tokenLength - 4);

        for (int i = segmentStart; i < fullContent.Length; i++)
        {
            //Console.WriteLine("Scanning index: " + i + " with character: " + fullContent[i]);
            var lastSegment = Segments.Last();
            if (lastSegment.StartIndex == i && lastSegment.Type != SegmentType.Markdown)
            {
                continue;
            }
            var seperatorType = HasNewSegmentType(fullContent, i);
            if (seperatorType.HasValue)
            {
                //Console.WriteLine("Found new segment Seperator: " + seperatorType.Value);
                if (lastSegment.Type == SegmentType.CodeBlock)
                {
                    i += 3;
                }
                // If the last segment is of the same type, just append to it
                lastSegment.EndIndex = Math.Max(lastSegment.StartIndex, i);
                //Console.WriteLine($"Extracting message from index {lastSegment.StartIndex} to {lastSegment.EndIndex}");
                lastSegment.FullContent = fullContent.Substring(lastSegment.StartIndex, lastSegment.EndIndex - lastSegment.StartIndex);
                //Console.WriteLine($"Extracting message: {lastSegment.FullContent}");
                UpdateContentAndLanguage(lastSegment);
                
                SegmentType type = seperatorType.Value;
                if(lastSegment.Type == type && lastSegment.Type != SegmentType.Markdown)
                {
                    // If the last segment is not of the same type, we need to create a new segment
                    type = SegmentType.Markdown;
                }
                

                var content = fullContent
                    .Substring(i);
                if (type == SegmentType.CodeBlock)
                {
                    content = content.TrimStart('`');
                }
                
                // Create a new segment
                //Console.WriteLine("Adding new segment: " + type + " with content: " + content);
                var segment = new Segment
                {
                    Type = type,
                    FullContent = fullContent.Substring(i),
                    Content = content,
                    Language = null,
                    StartIndex = i 
                };
                
                // Add the segment to the list
                Segments.Add(segment);
            }
            /*else if (lastSegment.Type != SegmentType.CodeBlock && lastSegment.Language == null &&
                     lastSegment.FullContent.Contains("\n"))
            {
                var firstCodeBlockCharacterIndex =
                    lastSegment.FullContent.IndexOf("\n", StringComparison.Ordinal);
                lastSegment.Language = lastSegment.FullContent
                    .Substring(
                        lastSegment.FullContent.LastIndexOf("`", StringComparison.Ordinal) + 1,
                        firstCodeBlockCharacterIndex - 1
                        );
                lastSegment.Content = lastSegment.Content.Substring(
                    lastSegment.Content.LastIndexOf("`", StringComparison.Ordinal) + 1
                );
            }*/
        }
    }

    private void UpdateContentAndLanguage(Segment lastSegment)
    {
        lastSegment.Content = lastSegment.FullContent
            .TrimEnd('\r', '\n')
            .Trim();
        if (lastSegment.Type == SegmentType.CodeBlock)
        {
            lastSegment.Content = lastSegment.Content.Trim('`');
        }
        /*else if (lastSegment.Type == SegmentType.Latex)
        {
            lastSegment.Content = lastSegment.Content.Trim('$');
        }*/
        Console.WriteLine("Finished Block with full content: " + lastSegment.FullContent);
    }

    private SegmentType? HasNewSegmentType(string message, int i)
    {
        if (message[i] == '`')
        {
            // Check for code block
            if (i + 2 < message.Length && message[i + 1] == '`' && message[i + 2] == '`')
            {
                // It's a code block
                return SegmentType.CodeBlock;
            }
        }
        /*else if (message[i] == '$')
        {
            // Check for LaTeX
            if (i + 1 < message.Length && message[i + 1] == '$')
            { 
                // It's a LaTeX segment
                return SegmentType.Latex;
            }
        }*/
        return null; // Default to Markdown
    }
}