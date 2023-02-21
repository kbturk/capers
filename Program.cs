// See https://aka.ms/new-console-template for more information
using System.IO;
using System;

namespace capers;

public class Capers {
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
        //TEMP CODE Showing Expr and pretty printer's capabilities
       //Expr expression = new Binary(
               //new Unary(
                   //new Token(TokenType.MINUS, "-", null, 1),
                   //new Literal(123)),
               //new Token(TokenType.STAR, "*", null, 1),
               //new Grouping(
                   //new Literal(45.67)));

    }


    private static void RunFile(string file) {
        string input_str;
        try {
            input_str = File.ReadAllText(file);
        }
        catch (Exception e) {
            throw new Exception(String.Format($"Could not open {e}"));
        }
        foreach (string line in input_str.Split('\n')) {
            Run(line);
        }

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

        Parser parser = new Parser(tokens);
        Expr expression = parser.parse();

        //Stop if there was a syntax error
        if (hadError) return;

        //foreach (Token token in tokens) {
        //Console.WriteLine($"{token.lexeme} : {token.type}, {token.literal} @ {token.line}");
        //}
        Console.WriteLine(new PrettyPrinter().print(expression));
    }

    //TODO: build an actual ErrorReporter
    public static void error(int line, String message) {
        report(line, "", message);
    }

    public static void error(Token token, string message) {
        if (token.type == TokenType.EOF) {
            report(token.line, "at end", message);
        } else {
            report(token.line, $" at '{token.lexeme}'", message);
        }
    }

    private static void report(int line, String where, String message) {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        hadError = true;
    }

}
