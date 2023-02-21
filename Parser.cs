namespace capers;

//big man on campus
public class Parser {
    private List<Token> tokens;
    private int current = 0;
    private class ParseError:Exception {}

    //surely there's a better way to do this.
    public Parser(List<Token> tokens) {
        this.tokens = tokens;
    }

    public Expr parse() {
        try {
            return expression();
        } catch (ParseError error) {
            return null;
        }
    }

    private Expr expression() {

        return equality();
    }

    private Expr equality() {

        Expr expr = comparison();

        while (b_match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
            Token oper = previous();
            Expr right = comparison();
            expr = new Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr comparison() {
        Expr expr = term();

        while (b_match(
                    TokenType.GREATER,
                    TokenType.GREATER_EQUAL,
                    TokenType.LESS,
                    TokenType.LESS_EQUAL)) {
            Token oper = previous();
            Expr right = term();
            expr = new Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr term() {
        Expr expr = factor();

        while (b_match(TokenType.MINUS, TokenType.PLUS)) {
            Token oper = previous();
            Expr right = factor();
            expr = new Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr factor() {
        Expr expr = unary();

        while (b_match(TokenType.SLASH, TokenType.STAR)) {
            Token oper = previous();
            Expr right = unary();
            expr = new Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr unary() {

        if (b_match(TokenType.BANG, TokenType.MINUS)) {
            Token oper = previous();
            Expr right = unary();
            return new Unary(oper, right);
        }

        return primary();
    }

    private Expr primary() {
        switch(tokens[current].type) {
            case TokenType.TRUE:
                advance();
                return new Literal(true);
            case TokenType.FALSE:
                advance();
                return new Literal(false);
            case TokenType.NIL:
                advance();
                return new Literal(null);
            case TokenType.NUMBER:
            case TokenType.STRING:
                advance();
                return new Literal(previous().literal);
            case TokenType.LEFT_PAREN:
                advance();
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping (expr);
        }
        throw error(peek(), "Expected expression.");

        return new Literal(null);//TODO: remove. This is a placeholder
    }

    //A collection of simple functions
    private bool b_match(params TokenType[] types) {
        foreach (TokenType type in types) {
            if (check_next(type)) {
                advance();
                return true;
            }
        }

        return false;
    }

    private bool check_next(TokenType type) {
        if (isAtEnd()) return false;
        return peek().type == type;
    }

    private bool isAtEnd() {
        return peek().type == TokenType.EOF;
    }

    private Token consume(TokenType type, string message) {
        if (check_next(type)) return advance();

        throw error(peek(), message);
    }

    private Token advance() {
        if (!isAtEnd()) current++;
        return previous();
    }

    private Token peek() {
        return tokens[current];
    }

    private Token previous() {
        return tokens[current - 1];
    }

    private ParseError error(Token token, string message) {
        Capers.error(token, message);
        return new ParseError();
    }

    private void synchronize() {
        advance();

        while(!isAtEnd()) {
            if (previous().type == TokenType.SEMICOLON) return;

            switch (peek().type) {
                case TokenType.CLASS:
                    break;
                case TokenType.FUNCT:
                    break;
                case TokenType.VAR:
                    break;
                case TokenType.FOR:
                    break;
                case TokenType.IF:
                    break;
                case TokenType.WHILE:
                    break;
                case TokenType.PRINT:
                    break;
                case TokenType.RETURN:
                    return;
            }

            advance();
        }
    }
}
