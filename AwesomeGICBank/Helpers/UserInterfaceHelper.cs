public static class UserInterfaceHelper
{
    public static void DisplayMainMenu()
    {
        Console.WriteLine("\nWelcome to AwesomeGIC Bank! What would you like to do?");
        Console.WriteLine("[I]nput transactions");
        Console.WriteLine("[D]efine interest rules");
        Console.WriteLine("[P]rint statement");
        Console.WriteLine("[Q]uit");
        Console.Write(">");
    }

    public static void DisplayErrorMessage(string message)
    {
        Console.WriteLine($"An error occurred: {message}");
    }
}
