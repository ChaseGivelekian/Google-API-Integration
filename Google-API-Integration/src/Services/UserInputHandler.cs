namespace Google_Drive_Organizer.Services;

public class UserInputHandler
{
    /// <summary>
    /// Prompts the user for input and returns the input.
    /// </summary>
    /// <param name="prompt">Text to display to the user</param>
    /// <returns>User's input as string</returns>
    public static string GetInput(string prompt)
    {
        Console.WriteLine($"{prompt}: ");
        return Console.ReadLine() ?? string.Empty;
    }

    /// <summary>
    /// Gets confirmation from the user (y/n)
    /// </summary>
    /// <param name="prompt">Question to ask the user</param>
    /// <returns>True if the user confirms, false otherwise</returns>
    public static bool GetConfirmation(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (y/n): ");
            var response = Console.ReadLine()?.Trim().ToLower();

            switch (response)
            {
                case "y":
                    return true;
                case "n":
                    return false;
                default:
                    Console.WriteLine("Please enter y or n.");
                    break;
            }
        }
    }

    /// <summary>
    /// Gets an integer input from the user
    /// </summary>
    /// <param name="prompt">Prompt to display</param>
    /// <param name="minValue">Minimum allowed value</param>
    /// <param name="maxValue">Maximum allowed value</param>
    /// <returns>Valid integer within range</returns>
    public int GetIntegerInput(string prompt, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            if (int.TryParse(Console.ReadLine(), out var result) && result >= minValue && result <= maxValue)
            {
                return result;
            }

            Console.WriteLine($"Please enter a valid number between {minValue} and {maxValue}.");
        }
    }
}