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
                    var headingText = line.Replace("## HEADING: ", "").Replace(" :HEADING_END", "");
                    requests.Add(CreateHeadingRequest(1, headingText.Length, "HEADING_2"));
                    requests.Add(CreateParagraphRequest(1, headingText));
                    documentLength += headingText.Length;
                }
                else if (line.StartsWith("### SUBHEADING: ") && line.Contains(" :SUBHEADING_END"))
                {
                    var subheadingText = line.Replace("### SUBHEADING: ", "").Replace(" :SUBHEADING_END", "");
                    requests.Add(CreateHeadingRequest(1, subheadingText.Length, "HEADING_3"));
                    requests.Add(CreateParagraphRequest(1, subheadingText));
                    documentLength += subheadingText.Length;
                }
                else if (line.StartsWith("PARAGRAPH: ") && line.Contains(" :PARAGRAPH_END"))
                {
                    var paragraphText = line.Replace("PARAGRAPH: ", "").Replace(" :PARAGRAPH_END", "");
                    var indentFirstLine = false;
                    if (paragraphText.StartsWith("INDENT_FIRST_LINE: "))
                    {
                        paragraphText = paragraphText.Replace("INDENT_FIRST_LINE: ", "");
                        indentFirstLine = true;
                    }

                    requests.Add(CreateHeadingRequest(1, paragraphText.Length, "NORMAL_TEXT", indentFirstLine));
                    requests.Add(CreateParagraphRequest(1, paragraphText));
                    documentLength += paragraphText.Length;
                }
                else if (line.Contains("BOLD: ") && line.Contains(" :BOLD_END"))
                {
                    var boldText = line.Replace("BOLD: ", "").Replace(" :BOLD_END", "");
                    requests.Add(CreateBoldTextRequest(1, boldText.Length));
                    requests.Add(CreateParagraphRequest(1, boldText));
                    documentLength += boldText.Length;
                }
                else if (line.StartsWith("LIST_ITEM: ") && line.Contains(" :LIST_ITEM_END"))
                {
                    var listItemText = line.Replace("LIST_ITEM: ", "").Replace(" :LIST_ITEM_END", "");
                    requests.Add(CreateListItemRequest(1, listItemText.Length));
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
        bool indentFirstLine = false)
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
                        : new Dimension { Magnitude = 0, Unit = "PT" }
                },
                Range = new Range { StartIndex = startIndex, EndIndex = startIndex + length },
                Fields = "namedStyleType,indentFirstLine"
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

    private static Request CreateListItemRequest(int startIndex, int length)
    {
        return new Request
        {
            CreateParagraphBullets = new CreateParagraphBulletsRequest
            {
                Range = new Range { StartIndex = startIndex, EndIndex = startIndex + length },
                BulletPreset = "BULLET_DISC_CIRCLE_SQUARE"
            }
        };
    }
}