using System.Text;
using Google.Apis.Forms.v1;

namespace Google_API_Integration.Services.AttachmentTextExtraction;

public static class GoogleFormTextExtraction
{
    private static readonly FormsService FormsService =
        GoogleCredentialsManager.CreateFormsServiceAsync().GetAwaiter().GetResult() ??
        throw new NullReferenceException();

    public static async Task<string> ExtractTextFromGoogleFormIdAsync(string formUrl)
    {
        var formId = formUrl.Split(["/forms/d/e/", "/forms/d/", "/forms/"], StringSplitOptions.None)[1].Split('/')[0];
        var request = FormsService.Forms.Get(formId);
        var form = await request.ExecuteAsync();

        if (form.Items == null || !form.Items.Any())
        {
            return "No items available in this form.";
        }

        var textBuilder = new StringBuilder();
        foreach (var item in form.Items)
        {
            textBuilder.AppendLine(item.Title);
            if (item.QuestionItem != null)
            {
                textBuilder.AppendLine(item.Description);
            }
        }

        return textBuilder.ToString();
    }
}