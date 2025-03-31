using Google.Apis.Classroom.v1.Data;
using Aspose.Words;

namespace Google_Drive_Organizer.Services.Classroom;

public static class AttachmentTextExtractor
{
    public static async Task<List<string>> ExtractTextFromAttachmentsAsync(CourseWork courseWork)
    {
         List<string> attachmentText = [];

        foreach (var material in courseWork.Materials)
        {
            if (material.DriveFile != null)
            {
                var extractedText = await ExtractTextFromDriveFileAsync(material.DriveFile);

                attachmentText.Add($"This is the text from the {material.DriveFile.DriveFile.Title} attachment: {(string.IsNullOrWhiteSpace(extractedText) ? "This file has no content" : extractedText)}");
            }

            if (material.YoutubeVideo != null)
            {
                // TODO - Implement Youtube video caption extraction
                attachmentText.Add(await ExtractTextFromYoutubeVideoAsync(material.YoutubeVideo));
            }

            if (material.Link != null)
            {
                // TODO - Implement link extraction
                attachmentText.Add(await ExtractTextFromLinkAsync(material.Link));
            }

            if (material.Form != null)
            {
                // TODO - Implement form extraction
                attachmentText.Add(await ExtractTextFromFormAsync(material.Form));
            }
        }

        return attachmentText;
    }

    private static async Task<string> ExtractTextFromFormAsync(Form form)
    {
        throw new NotImplementedException();
    }

    private static async Task<string> ExtractTextFromLinkAsync(Link link)
    {
        throw new NotImplementedException();
    }

    private static async Task<string> ExtractTextFromYoutubeVideoAsync(YouTubeVideo youtubeVideo)
    {
        throw new NotImplementedException();
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
        return doc.GetText();
    }
}