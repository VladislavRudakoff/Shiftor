namespace Shiftor;

public class ConsoleUI
{
    public static string SelectOption(string prompt, string[] options)
    {
        int selected = 0;

        ConsoleKey key;

        do
        {
            Console.Clear();

            Console.WriteLine(prompt);

            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"{(i == selected ? "> " : "  ")}{options[i]}");
            }

            key = Console.ReadKey(true).Key;

            selected = key switch
            {
                ConsoleKey.UpArrow => Math.Max(0, selected - 1),
                ConsoleKey.DownArrow => Math.Min(options.Length - 1, selected + 1),
                _ => selected
            };

        } while (key is not ConsoleKey.Enter);

        return options[selected];
    }

    public static string ReadString(string prompt)
    {
        Console.Write($"{prompt}: ");

        return Console.ReadLine() ?? string.Empty;
    }
}
