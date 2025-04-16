using Google.Apis.Docs.v1.Data;
using Range = Google.Apis.Docs.v1.Data.Range;

namespace Google_API_Integration.Services.Docs.Formatting;

public static class GoogleDocsFormatter
{
    public static IList<Request> ConvertAiResponseToRequestsFormat(string aiResponse)
    {
        var requests = new List<Request>();
        var formattingRequests = new List<Request>();
        var lines = aiResponse.Trim().Split('\n');

        var documentLength = 1;

        var fontName = lines.FirstOrDefault(line => line.StartsWith("FONT: ") && line.Contains(" :FONT_END"));
        fontName = fontName?.Replace("FONT: ", "").Replace(" :FONT_END", "");

        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("## HEADING: ") && line.Contains(" :HEADING_END"))
                {
                    var textWithoutFormatting = line.Replace("## HEADING: ", "").Replace(" :HEADING_END", "");
                    var (processedText, alignment) = ExtractAlignmentInfo(textWithoutFormatting);

                    if (fontName != null) requests.Add(CreateFontRequest(1, processedText.Length, fontName));
                    requests.Add(CreateHeadingRequest(1, processedText.Length, "HEADING_2", alignment: alignment));
                    requests.Add(CreateParagraphRequest(1, processedText));
                    documentLength += processedText.Length;
                }
                else if (line.StartsWith("### SUBHEADING: ") && line.Contains(" :SUBHEADING_END"))
                {
                    var subheadingText = line.Replace("### SUBHEADING: ", "").Replace(" :SUBHEADING_END", "");
                    var (processedText, alignment) = ExtractAlignmentInfo(subheadingText);

                    if (fontName != null) requests.Add(CreateFontRequest(1, processedText.Length + 1, fontName));
                    requests.Add(CreateHeadingRequest(1, processedText.Length, "HEADING_3", alignment: alignment));
                    requests.Add(CreateParagraphRequest(1, processedText));
                    documentLength += processedText.Length;
                }
                else if (line.StartsWith("PARAGRAPH: ") && line.Contains(" :PARAGRAPH_END"))
                {
                    var paragraphText = line.Replace("PARAGRAPH: ", "").Replace(" :PARAGRAPH_END", "");
                    var (processedText, alignment) = ExtractAlignmentInfo(paragraphText);
                    var indentFirstLine = false;

                    if (processedText.StartsWith("INDENT_FIRST_LINE: "))
                    {
                        processedText = processedText.Replace("INDENT_FIRST_LINE: ", "");
                        indentFirstLine = true;
                    }

                    var textParts = ExtractInlineBoldText(processedText);
                    var currentPosition = 1;

                    processedText = processedText.Replace("[**", "").Replace("**]", "");

                    foreach (var (text, isBold) in textParts)
                    {
                        if (isBold)
                        {
                            requests.Add(new Request
                            {
                                UpdateTextStyle = new UpdateTextStyleRequest
                                {
                                    TextStyle = new TextStyle { Bold = true },
                                    Range = new Range
                                        { StartIndex = currentPosition, EndIndex = currentPosition + text.Length },
                                    Fields = "bold"
                                }
                            });
                        }

                        currentPosition += text.Length;
                    }

                    requests.Add(CreateParagraphStylingRequest(1, processedText.Length, indentFirstLine, alignment));
                    if (fontName != null) requests.Add(CreateFontRequest(1, processedText.Length, fontName));
                    requests.Add(CreateNamedStyleRequest(1, processedText.Length, "NORMAL_TEXT"));
                    requests.Add(CreateParagraphRequest(1, processedText));

                    documentLength += processedText.Length;
                }
                else if (line.Contains("BOLD: ") && line.Contains(" :BOLD_END"))
                {
                    var boldText = line.Replace("BOLD: ", "").Replace(" :BOLD_END", "");
                    if (fontName != null) requests.Add(CreateFontRequest(1, boldText.Length, fontName));
                    requests.AddRange(CreateBoldTextRequest(1, boldText));
                    documentLength += boldText.Length;
                }
                else if (line.StartsWith("LIST_ITEM_BULLET: ") && line.Contains(" :LIST_ITEM_END"))
                {
                    // TODO: Fix the bullet point formatting
                    var listItemText = line.Replace("LIST_ITEM_BULLET: ", "").Replace(" :LIST_ITEM_END", "");
                    var (processedText, alignment) = ExtractAlignmentInfo(listItemText);
                    var indentFirstLine = false;

                    if (processedText.StartsWith("INDENT_FIRST_LINE: "))
                    {
                        processedText = processedText.Replace("INDENT_FIRST_LINE: ", "");
                        indentFirstLine = true;
                    }

                    var textParts = ExtractInlineBoldText(processedText);
                    var currentPosition = 1;

                    processedText = processedText.Replace("[**", "").Replace("**]", "");

                    foreach (var (text, isBold) in textParts)
                    {
                        if (isBold)
                        {
                            requests.Add(new Request
                            {
                                UpdateTextStyle = new UpdateTextStyleRequest
                                {
                                    TextStyle = new TextStyle { Bold = true },
                                    Range = new Range
                                        { StartIndex = currentPosition, EndIndex = currentPosition + text.Length },
                                    Fields = "bold"
                                }
                            });
                        }

                        currentPosition += text.Length;
                    }

                    requests.Add(DeleteListItemBulletPointRequest(1, 2));

                    requests.Add(CreateParagraphRequest(1, "\n"));

                    requests.Add(CreateListItemBulletPointRequest(1, processedText.Length));
                    requests.Add(CreateParagraphStylingRequest(1, processedText.Length, indentFirstLine, alignment));
                    if (fontName != null) requests.Add(CreateFontRequest(1, processedText.Length, fontName));
                    requests.Add(CreateNamedStyleRequest(1, processedText.Length, "NORMAL_TEXT"));
                    requests.Add(CreateParagraphRequest(1, processedText));
                    documentLength += processedText.Length;
                }
                // Whole document formatting
                else if (line.StartsWith("SPACING: ") && line.Contains(" :SPACING_END"))
                {
                    var spacingValue = line.Replace("SPACING: ", "").Replace(" :SPACING_END", "");
                    formattingRequests.Add(CreateSpacingRequest(1, documentLength, spacingValue));
                }
                else if (line.StartsWith("SIZE: ") && line.Contains(" :SIZE_END"))
                {
                    var sizeValue = line.Replace("SIZE: ", "").Replace(" :SIZE_END", "");
                    formattingRequests.Add(CreateSizeRequest(1, documentLength, sizeValue));
                }
            }
            else
            {
                requests.Add(CreateParagraphRequest(1, "\n"));
                documentLength += 1;
            }
        }

        requests.Reverse();
        requests.AddRange(formattingRequests);

        return requests;
    }

    private static Request CreateSizeRequest(int startIndex, int endIndex, string sizeValue)
    {
        return new Request
        {
            UpdateTextStyle = new UpdateTextStyleRequest
            {
                TextStyle = new TextStyle
                {
                    FontSize = new Dimension { Magnitude = float.Parse(sizeValue), Unit = "PT" },
                    ForegroundColor = new OptionalColor
                    {
                        Color = new Color
                        {
                            RgbColor = new RgbColor
                            {
                                Red = 0,
                                Green = 0,
                                Blue = 0
                            }
                        }
                    }
                },
                Range = new Range { StartIndex = startIndex, EndIndex = endIndex },
                Fields = "fontSize,foregroundColor"
            }
        };
    }

    private static Request CreateSpacingRequest(int startIndex, int endIndex, string spacingValue)
    {
        return new Request
        {
            UpdateParagraphStyle = new UpdateParagraphStyleRequest
            {
                ParagraphStyle = new ParagraphStyle
                {
                    LineSpacing = 100 * float.Parse(spacingValue)
                },
                Range = new Range { StartIndex = startIndex, EndIndex = endIndex },
                Fields = "lineSpacing"
            }
        };
    }

    private static Request CreateFontRequest(int startIndex, int endIndex, string fontName = "Arial")
    {
        return new Request
        {
            UpdateTextStyle = new UpdateTextStyleRequest
            {
                TextStyle = new TextStyle { WeightedFontFamily = new WeightedFontFamily { FontFamily = fontName } },
                Range = new Range { StartIndex = startIndex, EndIndex = endIndex },
                Fields = "weightedFontFamily"
            }
        };
    }

    private static Request CreateHeadingRequest(int startIndex, int length, string headingLevel,
        bool indentFirstLine = false, string alignment = "START")
    {
        return new Request
        {
            UpdateParagraphStyle = new UpdateParagraphStyleRequest
            {
                ParagraphStyle = new ParagraphStyle
                {
                    NamedStyleType = headingLevel,
                    IndentFirstLine = indentFirstLine
                        ? new Dimension { Magnitude = 36, Unit = "PT" }
                        : new Dimension { Magnitude = 0, Unit = "PT" },
                    Alignment = alignment
                },
                Range = new Range { StartIndex = startIndex, EndIndex = startIndex + length },
                Fields = "namedStyleType,indentFirstLine,alignment"
            }
        };
    }

    private static Request CreateParagraphStylingRequest(int startIndex, int length, bool indentFirstLine = false,
        string alignment = "START")
    {
        return new Request
        {
            UpdateParagraphStyle = new UpdateParagraphStyleRequest
            {
                ParagraphStyle = new ParagraphStyle
                {
                    IndentFirstLine = indentFirstLine
                        ? new Dimension { Magnitude = 36, Unit = "PT" }
                        : new Dimension { Magnitude = 0, Unit = "PT" },
                    Alignment = alignment
                },
                Range = new Range { StartIndex = startIndex, EndIndex = startIndex + length },
                Fields = "indentFirstLine,alignment"
            }
        };
    }

    private static Request CreateNamedStyleRequest(int startIndex, int length, string styleName)
    {
        return new Request
        {
            UpdateParagraphStyle = new UpdateParagraphStyleRequest
            {
                ParagraphStyle = new ParagraphStyle
                {
                    NamedStyleType = styleName
                },
                Range = new Range { StartIndex = startIndex, EndIndex = length },
                Fields = "namedStyleType"
            }
        };
    }

    private static Request CreateParagraphRequest(int startIndex, string text)
    {
        return new Request
        {
            InsertText = new InsertTextRequest
            {
                Text = text,
                Location = new Location { Index = startIndex }
            }
        };
    }

    private static List<Request> CreateBoldTextRequest(int startIndex, string text, bool indentFirstLine = false,
        string alignment = "START")
    {
        var requests = new List<Request>
        {
            new()
            {
                InsertText = new InsertTextRequest
                {
                    Text = text,
                    Location = new Location { Index = startIndex }
                }
            },
            CreateHeadingRequest(startIndex, text.Length, "NORMAL_TEXT", indentFirstLine, alignment),
            new()
            {
                UpdateTextStyle = new UpdateTextStyleRequest
                {
                    TextStyle = new TextStyle { Bold = true },
                    Range = new Range { StartIndex = startIndex, EndIndex = startIndex + text.Length },
                    Fields = "bold"
                }
            }
        };

        requests.Reverse();

        return requests;
    }

    private static Request CreateListItemBulletPointRequest(int startIndex, int length)
    {
        return new Request
        {
            CreateParagraphBullets = new CreateParagraphBulletsRequest
            {
                Range = new Range { StartIndex = startIndex, EndIndex = length },
                BulletPreset = "BULLET_DISC_CIRCLE_SQUARE"
            }
        };
    }

    private static Request DeleteListItemBulletPointRequest(int startIndex, int length)
    {
        return new Request
        {
            DeleteParagraphBullets = new DeleteParagraphBulletsRequest
            {
                Range = new Range { StartIndex = startIndex, EndIndex = length }
            }
        };
    }

    private static (string text, string alignment) ExtractAlignmentInfo(string text)
    {
        var alignment = "START";

        if (text.StartsWith("ALIGNMENT_START: ") && text.Contains(" :ALIGNMENT_END"))
        {
            text = text.Replace("ALIGNMENT_START: ", "").Replace(" :ALIGNMENT_END", "");
            alignment = "START";
        }
        else if (text.StartsWith("ALIGNMENT_CENTER: ") && text.Contains(" :ALIGNMENT_END"))
        {
            text = text.Replace("ALIGNMENT_CENTER: ", "").Replace(" :ALIGNMENT_END", "");
            alignment = "CENTER";
        }
        else if (text.StartsWith("ALIGNMENT_END: ") && text.Contains(" :ALIGNMENT_END"))
        {
            text = text.Replace("ALIGNMENT_END: ", "").Replace(" :ALIGNMENT_END", "");
            alignment = "END";
        }

        return (text, alignment);
    }

    private static List<(string text, bool isBold)> ExtractInlineBoldText(string text)
    {
        var result = new List<(string text, bool isBold)>();
        var currentIndex = 0;

        while (currentIndex < text.Length)
        {
            // Check for both [** and **[ patterns
            var boldStartIndex1 = text.IndexOf("[**", currentIndex, StringComparison.Ordinal);
            var boldStartIndex2 = text.IndexOf("**[", currentIndex, StringComparison.Ordinal);

            // Find the closest bold marker (or -1 if none found)
            var boldStartIndex = boldStartIndex1 == -1 ? boldStartIndex2 :
                boldStartIndex2 == -1 ? boldStartIndex1 :
                Math.Min(boldStartIndex1, boldStartIndex2);

            // Determine which format was found
            var isReversedFormat = boldStartIndex == boldStartIndex2;

            // If no more bold markers found, add remaining text as non-bold and exit
            if (boldStartIndex == -1)
            {
                if (currentIndex < text.Length)
                {
                    result.Add((text[currentIndex..], false));
                }

                break;
            }

            // Add text before the bold marker (if any)
            if (boldStartIndex > currentIndex)
            {
                result.Add((text.Substring(currentIndex, boldStartIndex - currentIndex), false));
            }

            // Find the end of the bold text - look for a corresponding closing pattern
            var closingPattern = isReversedFormat ? "]**" : "**]";
            var boldEndIndex = text.IndexOf(closingPattern, boldStartIndex, StringComparison.Ordinal);

            if (boldEndIndex == -1)
            {
                // No closing bold marker, treat the rest as non-bold
                result.Add((text[boldStartIndex..], false));
                break;
            }

            // Extract the bold text (excluding the markers)
            var boldText = text.Substring(boldStartIndex + 3, boldEndIndex - boldStartIndex - 3);

            result.Add((boldText, true));

            // Move the current index to after the closing bold marker
            currentIndex = boldEndIndex + 3;
        }

        return result;
    }
}