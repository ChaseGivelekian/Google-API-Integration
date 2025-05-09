using System.Text;
using System.Text.Json;
using Google_API_Integration.Interfaces;

namespace Google_API_Integration.Services.Gemini;

public class GeminiService(string apiKey) : IGeminiService
{
    private readonly HttpClient _httpClient = new();
    private readonly string _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

    private const string ApiBaseUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public async Task<string> GenerateContentAsync(string prompt,
        string systemPrompt = "")
    {
        var requestContent = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new
                    {
                        text = """
                               Format your response using the following structure:
                               1. Use 'FONT: ' to indicate the style of the response and end with ' :FONT_END' Put this at the end of the response.
                               2. Use 'SPACING: ' to indicate the spacing of the response and end with ' :SPACING_END' Put this at the end of the response.
                               3. Use 'SIZE: ' to indicate the size of the response and end with ' :SIZE_END' Put this at the end of the response.
                               4. Use '## HEADING: ' for section headings and end with ' :HEADING_END'
                               5. Use '### SUBHEADING: ' for subsection headings and end with ' :SUBHEADING_END'
                               6. Use 'PARAGRAPH: ' to start paragraphs and end with ' :PARAGRAPH_END'
                               7. Use 'BOLD: ' before bold text and end with ' :BOLD_END'
                               8. Use [**around text for inline bold text and end with**] Always keep the brackets and apostrophes (*) in this format [**example text**].
                               8. Use 'LIST_ITEM_BULLET: ' before each bullet point list item and end with ' :LIST_ITEM_END'
                               9. Use 'INDENT_FIRST_LINE: ' to indicate indentation
                               10. Use 'ALIGNMENT_START: ' to left align text and end with ' :ALIGNMENT_END'
                               11. Use 'ALIGNMENT_CENTER: ' to center align text and end with ' :ALIGNMENT_END'
                               12. Use 'ALIGNMENT_END: ' to right align text and end with ' :ALIGNMENT_END'

                               Examples:
                               ## HEADING: Introduction :HEADING_END
                               
                               ## HEADING: ALIGNMENT_CENTER: This is a centered heading :ALIGNMENT_END :HEADING_END
                               
                               ### SUBHEADING: Background :SUBHEADING_END
                               
                               PARAGRAPH: This is a normal paragraph text. :PARAGRAPH_END
                               
                               PARAGRAPH: INDENT_FIRST_LINE: This is an indented paragraph. :PARAGRAPH_END
                               
                               PARAGRAPH: [**This is a bold paragraph.**] :PARAGRAPH_END
                               
                               BOLD: This is an important section. :BOLD_END
                               
                               LIST_ITEM_BULLET: First point :LIST_ITEM_END
                               
                               LIST_ITEM_BULLET: Second point :LIST_ITEM_END
                               
                               LIST_ITEM_BULLET: [**This is a bold list item.**] :LIST_ITEM_END
                               
                               FONT: Times New Roman :FONT_END
                               SPACING: 2 :SPACING_END
                               SIZE: 12 :SIZE_END
                               
                               
                               1. Always remember to put the FONT, SPACING, AND SIZE at the end of the response.
                               2. Use BOLD: to indicate a bold section of text and end with :BOLD_END for inline bold text use [**bold text**].
                               3. Use LIST_ITEM_BULLET: to indicate a bullet point and end with :LIST_ITEM_END.
                               4. Always follow the format above.
                               5. If it isn't specified use the default values of FONT: Times New Roman, SPACING: 1.5, SIZE: 12.
                               6. Write in first person. Don't specify who you are. Don't do this: "I, name, etc."
                               7. Always use the correct grammar and spelling.
                               8. Always indent paragraph elements unless otherwise told not to.
                               9. Center the title of the document unless otherwise told not to. Put the alignment information inside of the parent element ie. paragraphs, headers, subheadings, and list items. For example HEADING: ALIGNMENT_CENTER: This is a centered heading :ALIGNMENT_END :HEADING_END
                               10. Don't include any introductory text or explanations in your response. Just provide the content as requested.
                               
                               
                               
                               Here is a perfect example of a response with the correct format, however use your discretion based on the assignment you are tasked with completing:
                               PARAGRAPH: April 28, 2025 :PARAGRAPH_END
                               
                               
                               
                               PARAGRAPH: Hello incoming senior, :PARAGRAPH_END
                               
                               PARAGRAPH: To be a senior at Medina County Career Center (MCCC) is to be on the precipice of adulthood, as you are a short nine months away from being able to achieve the future that you desire. The decision to go to MCCC was made because you had an idea of what you wanted to be in the future. However, these next few months will be a scary time. For those going to college, there is a lot of anxiety over getting into the school you want, and those going into the workforce will have a large change in their day-to-day routine. In the next chapter of your life, there is also the fear of being at the bottom of the totem pole in seniority. Right now, you’re a senior in high school; however, wherever you go next, be it college or a full-time job, you’ll be back to the bottom of the list. Nevertheless, you should still enjoy this final year of high school. Some of my favorite moments at MCCC are participating in the competitions that each lab has, like BPA and HOSA. These competitions, while nerve-wracking, were also some of the most fun, especially if it was an overnight trip. :PARAGRAPH_END
                               
                               PARAGRAPH: There is undoubtedly a different vibe in classes when all the students are seniors. Teachers seem more carefree and open in these classes, which I attribute to the students' age and increased maturity. However, there will still be teachers that you won’t like. This could be caused by many things, such as the way they teach, their interactions with students, or it could just be the material that they teach. Nevertheless, you still have to take the class, so my recommendation is to do good in the class out of spite. This is what got me through quite a few bad classes that I had to take. The feeling of superiority that you get when you pass their class is exhilarating. However, most teachers will be very cordial and nice if you just do your work and behave in class. In the end, I would say that during senior year, students are given more liberty and respect by the teachers as long as the students do not betray their trust in them. :PARAGRAPH_END
                               
                               PARAGRAPH: To be successful in your senior year, you must work on your communication, time management, and critical thinking. These are the most important skills to hone in your senior year, as they will help the most in the years after graduation. The sheer number of things that will be thrown at you during the senior year will test your time management, and during all of that, you have to make the correct decisions using critical thinking and communicating with others. Technology is one of the best things you can use to help with this. By utilizing technology such as reminders, calendars, and templates for certain types of assignments, it becomes a lot easier to manage everything that needs to be done. During my senior year, I needed to work on my communication skills as I had to work in a large group during my Capstone project in my lab. My biggest challenge was working with team members who didn’t want to do any of the work. My advice to you is to be a leader and encourage those around you to do their best as well, and hold them accountable for doing their part. :PARAGRAPH_END
                               
                               PARAGRAPH: As I said before, one of my biggest challenges was working in a group where others didn’t even try to participate. However, my solution to the problem wasn’t the best, as I worked harder to compensate for their lack of participation. I highly recommend you don’t go that route, as those who didn’t work hard (or at all) still got the same recognition as those who did all the work. While I didn’t struggle regarding school work, attendance, and grades, I know some seniors find it hard to keep up with all of that. My advice (if this applies to you) is to be a person that you can look back on and be proud of the hard work and commitment you put into even the most mundane of tasks. :PARAGRAPH_END
                               
                               PARAGRAPH: Ultimately, if I could go back and do it all over again, I would choose a remarkably similar path. Due to the hard work I put into all my assignments, I do not have many moments that I look back on and think, “I should have done more.” This mentality is something that my senior year has solidified and has become one of my greatest strengths. To me, this last year of high school is more about setting in stone who you are as a person so that the next step, whether that is college or the workforce, isn’t so daunting. In the end, I think that to flourish in this last year of high school, you should always stick to who you are, always do your best (even for the boring tasks), and not grow complacent in the months leading up to graduation. :PARAGRAPH_END
                               
                               PARAGRAPH: Sincerely, :PARAGRAPH_END
                               
                               
                               
                               PARAGRAPH: Chase Givelekian :PARAGRAPH_END
                               
                               """ + systemPrompt
                    }
                }
            },
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        return await SendRequestAsync(requestContent);
    }

    public async Task<string> AnalyzeDocumentContentAsync(string documentContent)
    {
        var prompt = $"Analyze the following document content: {documentContent}";
        return await GenerateContentAsync(prompt);
    }

    public async Task<string> CompleteAssignment(Dictionary<string, string> assignmentInformation, string systemPrompt = "")
    {
        var prompt =
            $"Complete this assignment from the students point of view based on the following information: Don't add anything else to your response outside of the assignment that you are being asked to complete. Don't include bold text or bullet points inside a formal letter. Here is the assignments information: {string.Join(",\n", assignmentInformation.Select(kv => $"{kv.Key}: {kv.Value}"))}";
        return await GenerateContentAsync(prompt, systemPrompt);
    }

    public async Task<string> SummarizeSubmissionAsync(string submissionContent, string assignmentDescription)
    {
        var prompt =
            $"Assignment description: {assignmentDescription}\n\nSubmission content: {submissionContent}\n\nPlease analyze this submission in relation to the assignment requirements and provide feedback.";
        return await GenerateContentAsync(prompt);
    }

    /// <summary>
    /// Sends a request to the Gemini API and returns the response.
    /// </summary>
    /// <param name="requestContent">Object that is in the Gemini REST format</param>
    /// <returns>Returns a string of the AI's response</returns>
    private async Task<string> SendRequestAsync(object requestContent)
    {
        var url = $"{ApiBaseUrl}?key={_apiKey}";

        var requestJson = JsonSerializer.Serialize(requestContent);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, httpContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        // Extract the text from the response
        if (responseObj.TryGetProperty("candidates", out var candidates) &&
            candidates.GetArrayLength() > 0 &&
            candidates[0].TryGetProperty("content", out var content) &&
            content.TryGetProperty("parts", out var parts) &&
            parts.GetArrayLength() > 0 &&
            parts[0].TryGetProperty("text", out var text))
        {
            return text.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}