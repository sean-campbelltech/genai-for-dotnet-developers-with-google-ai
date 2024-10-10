﻿// Step 1: Ctrl + Shift + P -> Select .NET: Generate Assets for Build & Debug
// Step 2: Add env variables
// Step 3: Set "console": "integratedTerminal" in launch.json
// Step 4: Convert to classic Main method to call recursively

using System.Globalization;
using System.Text;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        string API_KEY = Environment.GetEnvironmentVariable("API_KEY");

        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        using (HttpClient client = new())
        {
            using StringContent jsonRequestBody = new(
                JsonSerializer.Serialize(new
                {
                    contents = BuildContent(),
                    generationConfig = new
                    {
                        temperature = Convert.ToDouble(GetEnv("TEMPERATURE")),
                        maxOutputTokens = Convert.ToInt32(GetEnv("MAX_OUTPUT_TOKENS")),
                        topP = Convert.ToDouble(GetEnv("TOP-P")),
                        topK = Convert.ToInt32(GetEnv("TOP-K"))
                    }
                }),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = client.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={API_KEY}",
                jsonRequestBody
            ).Result;

            string result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
        }

        // Call recursively to keep on asking for user input prompts
        Main(args);
    }

    private static dynamic BuildContent()
    {
        string jsonContent = File.ReadAllText("contents.json")
            .Replace("{CONTEXT}", GetEnv("CONTEXT"))
            .Replace("{USER_PROMPT}", GetPrompt());

        return JsonSerializer.Deserialize<dynamic[]>(jsonContent);
    }

    private static string GetPrompt()
    {
        Console.WriteLine("Type a prompt:");
        return Console.ReadLine();
    }

    private static string GetEnv(string env)
    {
        return Environment.GetEnvironmentVariable(env);
    }
}