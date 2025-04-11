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

        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("## HEADING: ") && line.Contains(" :HEADING_END"))
                {
                    var textWithoutFormatting = line.Replace("## HEADING: ", "").Replace(" :HEADING_END", "");
                    var (processedText, alignment) = ExtractAlignmentInfo(textWithoutFormatting);

                    requests.Add(CreateHeadingRequest(1, processedText.Length, "HEADING_2", alignment: alignment));
                    requests.Add(CreateParagraphRequest(1, processedText));
                    documentLength += processedText.Length;
                }
                else if (line.StartsWith("### SUBHEADING: ") && line.Contains(" :SUBHEADING_END"))
                {
                    var subheadingText = line.Replace("### SUBHEADING: ", "").Replace(" :SUBHEADING_END", "");
                    var (processedText, alignment) = ExtractAlignmentInfo(subheadingText);

                    requests.Add(CreateHeadingRequest(1, processedText.Length, "HEADING_3", alignment: alignment));
                    requests.Add(CreateParagraphRequest(1, processedText));
                    documentLength += processedText.Length;
                }
                else if (line.StartsWith("PARAGRAPH: ") && line.Contains(" :PARAGRAPH_END"))
                {
                    // TODO: Add inline bold text support
                    var paragraphText = line.Replace("PARAGRAPH: ", "").Replace(" :PARAGRAPH_END", "");
                    var (processedText, alignment) = ExtractAlignmentInfo(paragraphText);
                    var indentFirstLine = false;

                    if (processedText.StartsWith("INDENT_FIRST_LINE: "))
                    {
                        processedText = processedText.Replace("INDENT_FIRST_LINE: ", "");
                        indentFirstLine = true;
                    }

                    requests.Add(CreateHeadingRequest(1, processedText.Length, "NORMAL_TEXT", indentFirstLine,
                        alignment: alignment));
                    requests.Add(CreateParagraphRequest(1, processedText));
                    documentLength += processedText.Length;
                }
                else if (line.Contains("BOLD: ") && line.Contains(" :BOLD_END"))
                {
                    var boldText = line.Replace("BOLD: ", "").Replace(" :BOLD_END", "");
                    requests.Add(CreateBoldTextRequest(1, boldText.Length));
                    requests.Add(CreateParagraphRequest(1, boldText));
                    documentLength += boldText.Length;
                }
                else if (line.StartsWith("LIST_ITEM_BULLET: ") && line.Contains(" :LIST_ITEM_END"))
                {
                    // TODO: Fix the bullet point formatting
                    var listItemText = line.Replace("LIST_ITEM_BULLET: ", "").Replace(" :LIST_ITEM_END", "");
                    requests.Add(CreateListItemBulletPointRequest(1, listItemText.Length));
                    requests.Add(CreateParagraphRequest(1, listItemText));
                    documentLength += listItemText.Length;
                }
                // Whole document formatting
                else if (line.StartsWith("FONT: ") && line.Contains(" :FONT_END"))
                {
                    var fontName = line.Replace("FONT: ", "").Replace(" :FONT_END", "");
                    formattingRequests.Add(CreateFontRequest(1, documentLength, fontName));
                }
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

    private static Request CreateFontRequest(int startIndex, int endIndex, string fontName)
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

    private static Request CreateBoldTextRequest(int startIndex, int length)
    {
        return new Request
        {
            UpdateTextStyle = new UpdateTextStyleRequest
            {
                TextStyle = new TextStyle { Bold = true },
                Range = new Range { StartIndex = startIndex, EndIndex = startIndex + length },
                Fields = "bold"
            }
        };
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
}