// See https://aka.ms/new-console-template for more information
using System.IO;
using System;

namespace capers;

public class Lox {
    public static bool hadError = false;

    public static void Main(string[] args) {
        switch (args.Length) {
        case > 1:
            throw new ArgumentException(String.Format(
                        "Please provide one argument instead of {0}.",
                        args.Length));
        case 1:
            RunFile(args[0]);
            break;

        case 0:
            RunPrompt();
            break;

        default:
           throw new ArgumentException(String.Format("Something went really wrong. {0}", args.Length));
        }
    }

    private static void RunFile(string file) {
        string input_str;
        try {
            input_str = File.ReadAllText(file);
        }
        catch (Exception e) {
            throw new Exception(String.Format($"Could not open {e}"));
        }
        Run(input_str);

        if (hadError) Environment.Exit(1); //TODO: give that int some info.
    }

    private static void RunPrompt() {
        while (true)
        {
            Console.Write("> ");
            Console.Out.Flush();
            string? line = Console.ReadLine();
            if (line == "") {
                break;
            }
            Run(line);
            hadError = false; //BOOOOOOOOOO
        }
    }

    private static void Run(string str){
        Scanner scanner = new Scanner (str);
        List<Token> tokens = scanner.scanTokens();

        foreach (Token token in tokens) {
            Console.WriteLine($"{token.lexeme} : {token.type}");
        }
    }

    //TODO: build an actual ErrorReporter
    public static void error(int line, String message) {
        report(line, "", message);
    }

    private static void report(int line, String where, String message) {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        hadError = true;
    }
}
