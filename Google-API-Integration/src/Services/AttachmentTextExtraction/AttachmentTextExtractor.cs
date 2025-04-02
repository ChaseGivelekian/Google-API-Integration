using Aspose.Words;
using Google.Apis.Classroom.v1.Data;

namespace Google_API_Integration.Services.AttachmentTextExtraction;

public static class AttachmentTextExtractor
{
    public static async Task<List<string>> ExtractTextFromAttachmentsAsync(CourseWork courseWork)
    {
        List<string> attachmentText = [];

        if (courseWork.Materials is null)
        {
            return ["This course work has no attachments"];
        }

        foreach (var material in courseWork.Materials)
        {
            if (material.DriveFile != null)
            {
                attachmentText.Add(
                    $"This is the text from the {material.DriveFile.DriveFile.Title} attachment: {await ExtractTextFromDriveFileAsync(material.DriveFile)}");
            }

            if (material.YoutubeVideo != null)
            {
                attachmentText.Add(
                    $"This is the text from the {material.YoutubeVideo.Title} attachment: {await ExtractTextFromYoutubeVideoAsync(material.YoutubeVideo)}");
            }

            if (material.Link != null)
            {
                attachmentText.Add(
                    $"This is the text from the {material.Link.Title} attachment: {await ExtractTextFromLinkAsync(material.Link)}");
            }

            if (material.Form != null)
            {
                attachmentText.Add(
                    $"This is the text from the {material.Form.Title} attachment: {await ExtractTextFromFormAsync(material.Form)}");
            }
        }

        return attachmentText;
    }

    private static async Task<string> ExtractTextFromFormAsync(Form form)
    {
        return await GoogleFormTextExtraction.ExtractTextFromGoogleFormIdAsync(form.FormUrl);
    }

    private static async Task<string> ExtractTextFromLinkAsync(Link link)
    {
        var extractedText = await new HttpClient().GetStringAsync(link.Url);

        return string.IsNullOrWhiteSpace(extractedText) ? "This link has no content" : extractedText;
    }

    private static async Task<string> ExtractTextFromYoutubeVideoAsync(YouTubeVideo youtubeVideo)
    {
        return await YoutubeCaptionExtraction.ExtractTextFromYoutubeIdAsync(youtubeVideo.Id);
    }

    private static async Task<string> ExtractTextFromDriveFileAsync(SharedDriveFile sharedDriveFile)
    {
        var driveService = await GoogleCredentialsManager.CreateDriveServiceAsync();

        // Download the file content into memory
        var request = driveService.Files.Get(sharedDriveFile.DriveFile.Id);
        using var stream = new MemoryStream();
        await request.DownloadAsync(stream);
        stream.Position = 0;

        // Load the document and extract text
        var doc = new Document(stream);
        var extractedText = doc.GetText();
        return string.IsNullOrWhiteSpace(extractedText) ? "This file has no content" : extractedText;
    }
}