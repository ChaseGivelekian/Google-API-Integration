using System.Text;
using Google_API_Integration.Interfaces;
using Google.Apis.Docs.v1.Data;

namespace Google_API_Integration.Services.Docs.DocsContentProcessing;

public class BodyContentProcessor : IDocumentContentProcessor
{
    public async Task<string> ProcessContent(Document document)
    {
        if (document.Body?.Content == null) return string.Empty;
        return await ReadStructuralElements(document.Body.Content);
    }

    private static async Task<string> ReadStructuralElements(IList<StructuralElement> elements)
    {
        var text = new StringBuilder();

        foreach (var element in elements)
        {
            if (element.Paragraph != null)
            {
                foreach (var paragraphElement in element.Paragraph.Elements)
                {
                    text.Append(await ReadParagraphElement(paragraphElement));
                }
            }
            else if (element.Table != null)
            {
                foreach (var row in element.Table.TableRows)
                {
                    foreach (var cell in row.TableCells)
                    {
                        text.Append(await ReadStructuralElements(cell.Content));
                    }
                }
            }
            else if (element.TableOfContents != null)
            {
                text.Append(await ReadStructuralElements(element.TableOfContents.Content));
            }
        }

        return text.ToString();
    }

    private static async Task<string> ReadParagraphElement(ParagraphElement paragraphElement)
    {
        return await Task.FromResult(paragraphElement.TextRun?.Content ?? string.Empty);
    }

}