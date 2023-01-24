using System.IO;
using System;

namespace capers;

public class Scanner {
    private string source;
    private List<Token> tokens = new List<Token>();

    public Scanner(string source) {
        this.source = source;
    }

    List<Token> scanTokens(){
        while (!isAtEnd()) {

            //next lexeme
            start = current;
            scanToken();
        }

        tokens.add(new Token(EOF, "", null, line));
        return tokens;
    }
}
