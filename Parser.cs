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
            if (b_match(TokenType.CLASS)) return classDeclaration();
            if (b_match(TokenType.FUNCT)) return function("function");
            if (b_match(TokenType.VAR)) return varDeclaration();

            return statement();
        } catch (ParseError error) {
            synchronize();
            return null;
        }
    }

    private Stmt classDeclaration() {
        Token name = consume(TokenType.IDENTIFIER, "Expect class name.");

        Variable superclass = null;
        if (b_match(TokenType.LESS)) {
            consume(TokenType.IDENTIFIER, "Expect superclass name.");
            superclass = new Variable(previous());
        }

        consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

        List<Function> methods = new List<Function>();
        while (!check_next(TokenType.RIGHT_BRACE) && !isAtEnd()) {
            methods.Add(function("method"));
        }

        consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

        return new Class(name, superclass, methods);
    }

    private Function function(string kind) {
        Token name = consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
        consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
        List<Token> parameters = new List<Token>();
        if (!check_next(TokenType.RIGHT_PAREN)) {
            do {
                if (parameters.Count >= 255) {
                    error(peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(
                        consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (b_match(TokenType.COMMA));
        }
        consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
        List<Stmt> body = block();
        return new Function(name, parameters, body);

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
        if (b_match(TokenType.FOR)) return forStatement();
        if (b_match(TokenType.IF)) return ifStatement();
        if (b_match(TokenType.PRINT)) return printStatement();
        if (b_match(TokenType.RETURN)) return returnStatement();
        if (b_match(TokenType.WHILE)) return whileStatement();
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

    private Stmt forStatement() {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

        Stmt initializer;
        if (b_match(TokenType.SEMICOLON)) {
            initializer = null;
        } else if (b_match(TokenType.VAR)) {
            initializer = varDeclaration();
        } else {
            initializer = expressionStatement();
        }

        Expr condition = null;
        if (!check_next(TokenType.SEMICOLON)) {
            condition = expression();
        }
        consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        Expr increment = null;
        if (!check_next(TokenType.RIGHT_PAREN)) {
            increment = expression();
        }
        consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");
        Stmt body = statement();

        if (increment !=null) {
            body = new Block(
                    new List<Stmt>{body, new Expression(increment)});

        }

        if (condition == null) condition = new Literal(true);
        body = new While(condition, body);

        if (initializer != null) {
            body = new Block(new List<Stmt>{initializer, body});
        }
        return body;
    }


    private Stmt ifStatement() {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        Stmt thenBranch = statement();
        Stmt elseBranch = null;
        if (b_match(TokenType.ELSE)) {
            elseBranch = statement();
        }

        return new If(condition, thenBranch, elseBranch);
    }

    private Stmt printStatement() {
        Expr val = expression();
        consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new Print(val);
    }

    private Stmt returnStatement() {
        Token keyword = previous();
        Expr val = null;
        if (!check_next(TokenType.SEMICOLON)) {
            val = expression();
        }

        consume(TokenType.SEMICOLON, "Expect ';' after return value.");
        return new ReturnStmt(keyword, val);
    }
                

    private Stmt whileStatement() {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expr condition = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        Stmt body = statement();

        return new While(condition, body);
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
        Expr expr = or();

        if (b_match(TokenType.EQUAL)) {
            Token equals = previous();
            Expr val = assignment();

            if (expr is Variable) {
                Token name = ((Variable)expr).name;
                return new Assign(name, val);
            } else if (expr is Get) {
                Get get = (Get)expr;
                return new Set(get.obj, get.name, val);
            }

            error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr or() {
        Expr expr = and();

        while (b_match(TokenType.OR)) {
            Token oper = previous();
            Expr right = and();
            expr = new Logical(expr, oper, right);
        }

        return expr;
    }

    private Expr and() {
        Expr expr = equality();

        while (b_match(TokenType.AND)) {
            Token oper = previous();
            Expr right = equality();
            expr = new Logical(expr, oper, right);
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

        return call();
    }

    private Expr finishCall(Expr callee) {
        List<Expr> arguments = new List<Expr>();
        if (!check_next(TokenType.RIGHT_PAREN)) {
            do {
                if (arguments.Count >= 255) {
                    error(peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(expression());
            } while (b_match(TokenType.COMMA));
        }

        Token paren = consume(TokenType.RIGHT_PAREN, "Expect ')' after agruments.");

        return new Call(callee, paren, arguments);
    }

    private Expr call() {
        Expr expr = primary();

        while (true) {
            if (b_match(TokenType.LEFT_PAREN)) {
                expr = finishCall(expr);
            } else if (b_match(TokenType.DOT)){
                Token name = consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                expr = new Get(expr, name);
            } else {
                break;
            }
        }

        return expr;
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
            case TokenType.THIS:
                advance();
                return new This(previous());
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
