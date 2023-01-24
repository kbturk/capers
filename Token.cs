using System.IO;
using System;

namespace capers;

class Token {
    private TokenType type;
    private string lexeme;
    private object literal; //TODO: change 'Object'
    private int line;

    Token(TokenType type, string lexeme, object literal, int line) {
        this.type = type;
        this.lexeme = lexeme;
        this.literal = literal;
        this.line = line;
    }

    public string toString() {
        return $"{type} {lexeme} {literal}";
    }
}
