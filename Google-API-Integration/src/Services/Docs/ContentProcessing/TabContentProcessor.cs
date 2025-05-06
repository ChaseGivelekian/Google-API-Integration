using System.Text;
using Google_API_Integration.Interfaces;
using Google.Apis.Docs.v1.Data;

namespace Google_API_Integration.Services.Docs.ContentProcessing;

public class TabContentProcessor : IDocumentContentProcessor
{
    public async Task<string> ProcessContent(Document document)
    {
        if (document.Tabs == null) return string.Empty;

        var result = new StringBuilder();
        var allTabs = new List<Tab>();

        foreach (var tab in document.Tabs)
        {
            await AddCurrentAndChildTabs(tab, allTabs);
        }

        foreach (var tab in allTabs)
        {
            if (tab.DocumentTab?.Body?.Content != null)
            {
                result.AppendLine(await ReadStructuralElements(tab.DocumentTab.Body.Content));
            }
        }

        return result.ToString();
    }

    private static async Task AddCurrentAndChildTabs(Tab tab, List<Tab> allTabs)
    {
        allTabs.Add(tab);
        foreach (var childTab in tab.ChildTabs)
        {
            await AddCurrentAndChildTabs(childTab, allTabs);
        }
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