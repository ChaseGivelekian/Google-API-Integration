using Google.Apis.Docs.v1.Data;
using Range = Google.Apis.Docs.v1.Data.Range;

namespace Google_API_Integration.Services.Docs.Formatting;

public static class GoogleDocsFormatter
{
    public static IList<Request> ConvertAiResponseToRequestsFormat(string aiResponse)
    {
        var requests = new List<Request>();
        var lines = aiResponse.Split('\n');
        var formattingRequests = new List<Request>();

        var currentIndex = 1;

        foreach (var line in lines)
        {
            // Document-wid formatting settings to apply later
            // Font formatting
            if (line.StartsWith("FONT: ") && line.Contains(" :FONT_END"))
            {
                var fontName = line.Replace("FONT: ", "")
                    .Replace(" :FONT_END", "");
                formattingRequests.Add(CreateFontRequest(1, currentIndex, fontName));
                continue;
            }

            // Spacing formatting
            if (line.StartsWith("SPACING: ") && line.Contains(" :SPACING_END"))
            {
                var spacingValue = line.Replace("SPACING: ", "")
                    .Replace(" :SPACING_END", "");
                formattingRequests.Add(CreateSpacingRequest(1, currentIndex, spacingValue));
                continue;
            }

            // Size formatting
            if (line.StartsWith("SIZE: ") && line.Contains(" :SIZE_END"))
            {
                var sizeValue = line.Replace("SIZE: ", "")
                    .Replace(" :SIZE_END", "");
                formattingRequests.Add(CreateSizeRequest(1, currentIndex, sizeValue));
                continue;
            }

            var startIndex = currentIndex;

            // Content formatting
            if (line.StartsWith("## HEADING: ") && line.Contains(" :HEADING_END"))
            {
                var headingText = line.Replace("## HEADING: ", "").Replace(" :HEADING_END", "");
                requests.Add(CreateParagraphRequest(currentIndex, headingText));
                var textLength = headingText.Length;
                currentIndex += textLength;
                requests.Add(CreateHeadingRequest(startIndex, textLength, "HEADING_2"));
            }
            else if (line.StartsWith("### SUBHEADING: ") && line.Contains(" :SUBHEADING_END"))
            {
                var subheadingText = line.Replace("### SUBHEADING: ", "").Replace(" :SUBHEADING_END", "");
                requests.Add(CreateParagraphRequest(currentIndex, subheadingText));
                var textLength = subheadingText.Length;
                currentIndex += textLength;
                requests.Add(CreateHeadingRequest(startIndex, textLength, "HEADING_3"));
            }
            else if (line.StartsWith("PARAGRAPH: ") && line.Contains(" :PARAGRAPH_END"))
            {
                var paragraphText = line.Replace("PARAGRAPH: ", "").Replace(" :PARAGRAPH_END", "");
                requests.Add(CreateParagraphRequest(currentIndex, paragraphText));
                currentIndex += paragraphText.Length;
            }
            else if (line.StartsWith("BOLD: ") && line.Contains(" :BOLD_END"))
            {
                var boldText = line.Replace("BOLD: ", "").Replace(" :BOLD_END", "");
                requests.Add(CreateParagraphRequest(currentIndex, boldText));
                var textLength = boldText.Length;
                currentIndex += textLength;
                requests.Add(CreateBoldTextRequest(startIndex, textLength));
            }
            else if (line.StartsWith("LIST_ITEM: ") && line.Contains(" :LIST_ITEM_END"))
            {
                var listItemText = line.Replace("LIST_ITEM: ", "").Replace(" :LIST_ITEM_END", "");
                requests.Add(CreateParagraphRequest(currentIndex, listItemText));
                var textLength = listItemText.Length;
                currentIndex += textLength;
                requests.Add(CreateListItemRequest(startIndex, textLength));
            }
            else if (line.StartsWith("CODE_BLOCK:") && line.Contains(":CODE_BLOCK_END"))
            {
                var codeText = line.Replace("CODE_BLOCK:", "").Replace(":CODE_BLOCK_END", "");
                requests.Add(CreateParagraphRequest(currentIndex, codeText));
                var textLength = codeText.Length;
                currentIndex += textLength;
                requests.Add(CreateCodeBlockRequest(startIndex, textLength));
            }
            else if (line.StartsWith("QUOTE: ") && line.Contains(" :QUOTE_END"))
            {
                var quoteText = line.Replace("QUOTE: ", "").Replace(" :QUOTE_END", "");
                requests.Add(CreateParagraphRequest(currentIndex, quoteText));
                var textLength = quoteText.Length;
                currentIndex += textLength;
                requests.Add(CreateQuoteRequest(startIndex, textLength));
            }
            else
            {
                // Plain text
                requests.Add(CreateParagraphRequest(currentIndex, line));
                currentIndex += line.Length;
            }

            // Add a newline after each line except for the last one
            if (line == lines.Last()) continue;
            requests.Add(CreateParagraphRequest(currentIndex, "\n"));
            currentIndex += 1;
        }

        // Apply the document-wide formatting requests
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
                    { FontSize = new Dimension { Magnitude = float.Parse(sizeValue), Unit = "PT" } },
                Range = new Range { StartIndex = startIndex, EndIndex = endIndex },
                Fields = "fontSize"
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
                    LineSpacing = float.TryParse(spacingValue, out var spacing) ? spacing : 100
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
        if (string.IsNullOrWhiteSpace(text))
        {
            text = "\n";
        }

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

    private static Request CreateCodeBlockRequest(int startIndex, int length)
    {
        return new Request
        {
            UpdateTextStyle = new UpdateTextStyleRequest
            {
                TextStyle = new TextStyle { WeightedFontFamily = new WeightedFontFamily { FontFamily = "Consolas" } },
                Range = new Range { StartIndex = startIndex, EndIndex = startIndex + length },
                Fields = "weightedFontFamily"
            }
        };
    }

    private static Request CreateQuoteRequest(int startIndex, int length)
    {
        return new Request
        {
            UpdateParagraphStyle = new UpdateParagraphStyleRequest
            {
                ParagraphStyle = new ParagraphStyle
                {
                    IndentStart = new Dimension { Magnitude = 36, Unit = "PT" }
                },
                Range = new Range { StartIndex = startIndex, EndIndex = startIndex + length },
                Fields = "indentStart"
            }
        };
    }
}