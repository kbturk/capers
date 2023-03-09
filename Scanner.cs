using System.IO;
using System;

namespace capers;

public class Scanner {
    private String source;
    public List<Token> tokens = new List<Token>();
    private int start = 0;
    private int current = 0;
    private int line = 1;

    public Scanner(String source) {
        this.source = source;
    }

    public List<Token> scanTokens(){
        while (!isAtEnd()) {

            //next lexeme
            start = current;
            scanToken();
        }

        tokens.Add(new Token(TokenType.EOF, "", null, line));
        return tokens;
    }

    private void scanToken() {
        char c = advance();
        switch (c) {
            case '(': addToken(TokenType.LEFT_PAREN); break;
            case ')': addToken(TokenType.RIGHT_PAREN); break;
            case '{': addToken(TokenType.LEFT_BRACE); break;
            case '}': addToken(TokenType.RIGHT_BRACE); break;
            case ',': addToken(TokenType.COMMA); break;
            case '.': addToken(TokenType.DOT); break;
            case '-': addToken(TokenType.MINUS); break;
            case '+': addToken(TokenType.PLUS); break;
            case ';': addToken(TokenType.SEMICOLON); break;
            case '*': addToken(TokenType.STAR); break;
            case '!':
                      addToken(match('=') ?
                              TokenType.BANG_EQUAL : TokenType.BANG);
                      break;
            case '=':
                      addToken(match('=') ?
                              TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                      break;
            case '<': addToken(match('=') ?
                              TokenType.LESS_EQUAL: TokenType.LESS);
                      break;
            case '>': addToken(match('=') ?
                              TokenType.GREATER_EQUAL : TokenType.GREATER);
                      break;
            case '/':
                      if (match('/')) {
                          // you have a comment line!
                          // just ignore it until \n happens
                          while (peek() != '\n' && !isAtEnd()) advance();
                      } else {
                          addToken(TokenType.SLASH);
                      }
                      break;
            
            //and now the skip chars
            case ' ' :
            case '\r':
            case '\t':
                      break;
            //advance line char
            case '\n':
                      this.line++;
                      break;

            //string literals with yet another helper function
            case '"': isString(); break;

            default:
                      if (isDigit(c)) {
                          number();
                      } else if (isAlpha(c)) {
                          identifier();
                      } else {
                          Capers.error(line, $"Unexpected character. {c}");
                      }
                      break;
        }
    }

    private void isString() {
        while (peek() != '"' && !isAtEnd()) {
            if (peek() == '\n') this.line++;
            advance();
        }

        //handle the case of running off the end w/o a closing '"'
        if (isAtEnd()) {
            Capers.error(line, "Unterminated string.");
            return;
        }

        //otherwise... we did have a " so...
        advance();

        //trim the surrounding quotes
        String value = source.Substring(start + 1, current - 2 - start);
        addToken(TokenType.STRING, value);
    }

    private bool isDigit(char c) {
        return c >= '0' && c <= '9';
    }

    private void number() {
        while (isDigit(peek())) advance();

        //look for fractional part.
        //TODO: how hard would it be to add INT and DOUBLE types?
        if (peek() == '.' && isDigit(peekNext())) {
            //consume the '.'
            advance();

            while (isDigit(peek())) advance();
        }

        addToken(TokenType.NUMBER,
                Convert.ToDouble(
                    source.Substring(start, current - start)));
    }

    private void identifier() {
        //if the first char is alpha or numeric continue until end
        while (isAlphaNumeric(peek())) advance();
        
        String text = source.Substring(start, current - start);
        TokenType type = CheckForIdentTokenType(text);
        addToken(type);
    }

    private bool isAlpha(char c) {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private bool isAlphaNumeric(char c) {
        return isAlpha(c) || isDigit(c);
    }

    private static TokenType CheckForIdentTokenType(string lexeme)
    {
        switch (lexeme) {
            case "and": return TokenType.AND;
            case "class": return TokenType.CLASS;
            case "else": return TokenType.ELSE;
            case "false": return TokenType.FALSE;
            case "for": return TokenType.FOR;
            case "funct": return TokenType.FUNCT;
            case "if": return TokenType.IF;
            case "nil": return TokenType.NIL;
            case "or": return TokenType.OR;
            case "print": return TokenType.PRINT;
            case "return": return TokenType.RETURN;
            case "super": return TokenType.SUPER;
            case "this": return TokenType.THIS;
            case "true": return TokenType.TRUE;
            case "var": return TokenType.VAR;
            case "while": return TokenType.WHILE;
            default: return TokenType.IDENTIFIER;
        }
    }

    private bool match(char expected) {
        if (isAtEnd()) return false;
        if (source[current] != expected) return false;
        current++;
        return true;
    }

    private char peek() {
        if (isAtEnd()) return '\0';
        return source[current];
    }

    private char peekNext() {
        if (current + 1 > source.Length) return '\0';
        return source[current + 1];
    }

    private bool isAtEnd() {
        return current >= source.Length;
    }

    private char advance() {
        return source[current++];
    }

    private void addToken(TokenType type) {
        addToken(type, null);
    }

    private void addToken(TokenType type, object literal) {
        String text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }
}
