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

        // var formattingRequests = new List<Request>();

        // var currentIndex = 1;

        // foreach (var line in lines)
        // {
        //     // Document-wid formatting settings to apply later
        //     // Font formatting
        //     if (line.StartsWith("FONT: ") && line.Contains(" :FONT_END"))
        //     {
        //         var fontName = line.Replace("FONT: ", "")
        //             .Replace(" :FONT_END", "");
        //         formattingRequests.Add(CreateFontRequest(1, currentIndex, fontName));
        //         continue;
        //     }
        //
        //     // Spacing formatting
        //     if (line.StartsWith("SPACING: ") && line.Contains(" :SPACING_END"))
        //     {
        //         var spacingValue = line.Replace("SPACING: ", "")
        //             .Replace(" :SPACING_END", "");
        //         formattingRequests.Add(CreateSpacingRequest(1, currentIndex, spacingValue));
        //         continue;
        //     }
        //
        //     // Size formatting
        //     if (line.StartsWith("SIZE: ") && line.Contains(" :SIZE_END"))
        //     {
        //         var sizeValue = line.Replace("SIZE: ", "")
        //             .Replace(" :SIZE_END", "");
        //         formattingRequests.Add(CreateSizeRequest(1, currentIndex, sizeValue));
        //         continue;
        //     }
        //
        //     var startIndex = currentIndex;
        //
        //     // Content formatting
        //     if (line.StartsWith("## HEADING: ") && line.Contains(" :HEADING_END"))
        //     {
        //         var headingText = line.Replace("## HEADING: ", "").Replace(" :HEADING_END", "");
        //         // Insert a paragraph break before the heading if not at the beginning
        //         if (currentIndex > 1)
        //         {
        //             requests.Add(CreateParagraphRequest(currentIndex, "\n"));
        //             currentIndex += 1;
        //         }
        //
        //         startIndex = currentIndex;
        //         requests.Add(CreateParagraphRequest(currentIndex, headingText));
        //         var textLength = headingText.Length;
        //         currentIndex += textLength;
        //         // Apply the heading style to the whole paragraph
        //         requests.Add(CreateHeadingRequest(startIndex, textLength, "HEADING_2"));
        //         // Ensure there's a paragraph break after
        //         requests.Add(CreateParagraphRequest(currentIndex, "\n"));
        //         currentIndex += 1;
        //     }
        //     else if (line.StartsWith("### SUBHEADING: ") && line.Contains(" :SUBHEADING_END"))
        //     {
        //         var subheadingText = line.Replace("### SUBHEADING: ", "").Replace(" :SUBHEADING_END", "");
        //         // Insert a paragraph break before the subheading if not at the beginning
        //         if (currentIndex > 1)
        //         {
        //             requests.Add(CreateParagraphRequest(currentIndex, "\n"));
        //             currentIndex += 1;
        //         }
        //
        //         startIndex = currentIndex;
        //         requests.Add(CreateParagraphRequest(currentIndex, subheadingText));
        //         var textLength = subheadingText.Length;
        //         currentIndex += textLength;
        //         // Apply the subheading style to the whole paragraph
        //         requests.Add(CreateHeadingRequest(startIndex, textLength, "HEADING_3"));
        //         // Ensure there's a paragraph break after
        //         requests.Add(CreateParagraphRequest(currentIndex, "\n"));
        //         currentIndex += 1;
        //     }
        //     else if (line.StartsWith("PARAGRAPH: ") && line.Contains(" :PARAGRAPH_END"))
        //     {
        //         var paragraphText = line.Replace("PARAGRAPH: ", "").Replace(" :PARAGRAPH_END", "");
        //         // var wrappedText = WordWrap(paragraphText, 100); // Adjust maxLineLength as needed
        //         requests.Add(CreateParagraphRequest(currentIndex, paragraphText));
        //         currentIndex += paragraphText.Length;
        //     }
        //     else if (line.StartsWith("BOLD: ") && line.Contains(" :BOLD_END"))
        //     {
        //         var boldText = line.Replace("BOLD: ", "").Replace(" :BOLD_END", "");
        //         requests.Add(CreateParagraphRequest(currentIndex, boldText));
        //         var textLength = boldText.Length;
        //         currentIndex += textLength;
        //         requests.Add(CreateBoldTextRequest(startIndex, textLength));
        //     }
        //     else if (line.StartsWith("LIST_ITEM: ") && line.Contains(" :LIST_ITEM_END"))
        //     {
        //         var listItemText = line.Replace("LIST_ITEM: ", "").Replace(" :LIST_ITEM_END", "");
        //         requests.Add(CreateParagraphRequest(currentIndex, listItemText));
        //         var textLength = listItemText.Length;
        //         currentIndex += textLength;
        //         requests.Add(CreateListItemRequest(startIndex, textLength));
        //     }
        //     else if (line.StartsWith("CODE_BLOCK:") && line.Contains(":CODE_BLOCK_END"))
        //     {
        //         var codeText = line.Replace("CODE_BLOCK:", "").Replace(":CODE_BLOCK_END", "");
        //         requests.Add(CreateParagraphRequest(currentIndex, codeText));
        //         var textLength = codeText.Length;
        //         currentIndex += textLength;
        //         requests.Add(CreateCodeBlockRequest(startIndex, textLength));
        //     }
        //     else if (line.StartsWith("QUOTE: ") && line.Contains(" :QUOTE_END"))
        //     {
        //         var quoteText = line.Replace("QUOTE: ", "").Replace(" :QUOTE_END", "");
        //         requests.Add(CreateParagraphRequest(currentIndex, quoteText));
        //         var textLength = quoteText.Length;
        //         currentIndex += textLength;
        //         requests.Add(CreateQuoteRequest(startIndex, textLength));
        //     }
        //     else
        //     {
        //         // Plain text
        //         if (string.IsNullOrWhiteSpace(line)) continue;
        //         requests.Add(CreateParagraphRequest(currentIndex, line));
        //         currentIndex += line.Length;
        //     }
        //
        //     // Add a newline after each line except for the last one
        //     // if (line == lines.Last()) continue;
        //     // requests.Add(CreateParagraphRequest(currentIndex, "\n"));
        //     // currentIndex += 1;
        // }

        // Apply the document-wide formatting requests
        // requests.AddRange(formattingRequests);

        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("## HEADING: ") && line.Contains(" :HEADING_END"))
                {
                    var headingText = line.Replace("## HEADING: ", "").Replace(" :HEADING_END", "");
                    requests.Add(CreateParagraphRequest(1, headingText));
                    requests.Add(CreateHeadingRequest(1, headingText.Length, "HEADING_2"));
                    documentLength += headingText.Length;
                }
                else if (line.StartsWith("### SUBHEADING: ") && line.Contains(" :SUBHEADING_END"))
                {
                    var subheadingText = line.Replace("### SUBHEADING: ", "").Replace(" :SUBHEADING_END", "");
                    requests.Add(CreateParagraphRequest(1, subheadingText));
                    requests.Add(CreateHeadingRequest(1, subheadingText.Length, "HEADING_3"));
                    documentLength += subheadingText.Length;
                }
                else if (line.StartsWith("PARAGRAPH: ") && line.Contains(" :PARAGRAPH_END"))
                {
                    var paragraphText = line.Replace("PARAGRAPH: ", "").Replace(" :PARAGRAPH_END", "");
                    requests.Add(CreateParagraphRequest(1, paragraphText));
                    // requests.Add(CreateHeadingRequest(1, paragraphText.Length, "NORMAL_TEXT"));
                    documentLength += paragraphText.Length;
                }
                else if (line.StartsWith("BOLD: ") && line.Contains(" :BOLD_END"))
                {
                    var boldText = line.Replace("BOLD: ", "").Replace(" :BOLD_END", "");
                    requests.Add(CreateParagraphRequest(1, boldText));
                    requests.Add(CreateBoldTextRequest(1, boldText.Length));
                    documentLength += boldText.Length;
                }
                else if (line.StartsWith("LIST_ITEM: ") && line.Contains(" :LIST_ITEM_END"))
                {
                    var listItemText = line.Replace("LIST_ITEM: ", "").Replace(" :LIST_ITEM_END", "");
                    requests.Add(CreateParagraphRequest(1, listItemText));
                    requests.Add(CreateListItemRequest(1, listItemText.Length));
                    documentLength += listItemText.Length;
                }
                else if (line.StartsWith("CODE_BLOCK") && line.Contains(" :CODE_BLOCK_END"))
                {
                    var codeText = line.Replace("CODE_BLOCK:", "").Replace(":CODE_BLOCK_END", "");
                    requests.Add(CreateParagraphRequest(1, codeText));
                    documentLength += codeText.Length;
                }
                else if (line.StartsWith("QUOTE: ") && line.Contains(" :QUOTE_END"))
                {
                    var quoteText = line.Replace("QUOTE: ", "").Replace(" :QUOTE_END", "");
                    requests.Add(CreateParagraphRequest(1, quoteText));
                    documentLength += quoteText.Length;
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
                Fields = "*"
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

    private static Request CreateHeadingRequest(int startIndex, int length, string headingLevel)
    {
        return new Request
        {
            UpdateParagraphStyle = new UpdateParagraphStyleRequest
            {
                ParagraphStyle = new ParagraphStyle { NamedStyleType = headingLevel },
                Range = new Range { StartIndex = startIndex, EndIndex = startIndex + length },
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