namespace capers;

public class Parser {
    private List<Token> tokens;
    private int current = 0;
    private class ParseError:Exception {}

    public Parser(List<Token> tokens) {
        this.tokens = tokens;
    }

    public List<Stmt> parse() {
        List<Stmt> statements = new List<Stmt>();
        while (!isAtEnd()) {
            statements.Add(declaration());
        }

        return statements;
    }

    private Expr expression() {

        return assignment();
    }

    private Stmt declaration() {
        try {
            if (b_match(TokenType.VAR)) return varDeclaration();

            return statement();
        } catch (ParseError error) {
            synchronize();
            return null;
        }
    }

    private Stmt varDeclaration() {
        Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expr initializer = null;
        if (b_match(TokenType.EQUAL)) {
            initializer = expression();
        }

        consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new Var(name, initializer);
    }

    //Statement code
    private Stmt statement() {
        if (b_match(TokenType.PRINT)) return printStatement();
        if (b_match(TokenType.LEFT_BRACE)) return new Block(block());

        return expressionStatement();
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

    private Stmt printStatement() {
        Expr val = expression();
        consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new Print(val);
    }

    private Stmt expressionStatement() {
        Expr expr = expression();
        consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new Expression(expr);
    }

    private List<Stmt> block() {
        List<Stmt> statements = new List<Stmt>();

        while (!check_next(TokenType.RIGHT_BRACE) && !isAtEnd()) {
            statements.Add(declaration());
        }
        consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Expr assignment() {
        Expr expr = equality();

        if (b_match(TokenType.EQUAL)) {
            Token equals = previous();
            Expr val = assignment();

            if (expr is Variable) {
                Token name = ((Variable)expr).name;
                return new Assign(name, val);
            }

            error(equals, "Invalid assignment target.");
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
            case TokenType.IDENTIFIER:
                advance();
                return new Variable(previous());
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
                case TokenType.FUNCT:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            advance();
        }
    }
}
