using System.IO;
using System;

namespace capers;

public class Token {
    public TokenType type;
    public String lexeme;
    public object? literal; //TODO: change 'Object'
    public int line;

    public Token(TokenType type, String lexeme, object? literal, int line) {
        this.type = type;
        this.lexeme = lexeme;
        this.literal = literal;
        this.line = line;
    }

    public String toString() {
        return $"{type} {lexeme} {literal}";
    }
}
