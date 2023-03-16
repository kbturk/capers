using System.Collections;
using System;

namespace capers;

public class Resolver: VisitorExpr<Nullable<bool>>, VisitorStmt<Nullable<bool>>{
    private Interpreter interpreter;
    private Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();

    public Resolver(Interpreter interpreter) {
        this.interpreter = interpreter;
    }

    //Stmt Visitor Patterns
    public bool? VisitBlock(Block stmt) {
        beginScope();
        resolve(stmt.statements);
        endScope();
        return null;
    }

    public bool? VisitExpression(Expression stmt) {
        resolve(stmt.expression);
        return null;
    }

    public bool? VisitFunction(Function stmt) {
        declare(stmt.name);
        define(stmt.name);

        resolveFunction(stmt);
        return null;
    }

    public bool? VisitIf(If stmt) {
        return null;
    }

    public bool? VisitPrint(Print stmt) {
        return null;
    }

    public bool? VisitReturnStmt(ReturnStmt stmt) {
        return null;
    }

    public bool? VisitVar(Var stmt) {
        declare(stmt.name);
        if (stmt.initializer != null) {
            resolve(stmt.initializer);
        }
        define(stmt.name);
        return null;
    }

    public bool? VisitWhile(While stmt) {
        return null;
    }

    //Expr Visitor Patterns
    public bool? VisitAssign(Assign expr) {
        resolve(expr.value);
        resolveLocal(expr, expr.name);
        return null;
    }

    public bool? VisitBinary(Binary expr) {
        return null;
    }

    public bool? VisitCall(Call expr) {
        return null;
    }

    public bool? VisitGrouping(Grouping expr) {
        return null;
    }

    public bool? VisitLiteral(Literal expr) {
        return null;
    }

    public bool? VisitLogical(Logical expr) {
        return null;
    }

    public bool? VisitUnary(Unary expr) {
        return null;
    }

    public bool? VisitVariable(Variable expr) {
        if (!(scopes.Count == 0) &&
                scopes.Peek()[expr.name.lexeme] == false) {
            Capers.error(expr.name, "Can't read local variable in its own initializer.");
        }

        resolveLocal(expr, expr.name);
        return null;
    }

    //Private helper functions

    //Resolve overload functions
    private void resolve(List<Stmt> statements) {
        foreach (Stmt statement in statements) {
            resolve(statement);
        }
    }

    private void resolve(Stmt stmt) {
        stmt.Accept(this);
    }

    private void resolve(Expr expr) {
        expr.Accept(this);
    }

    private void resolveFunction(Function function) {
        beginScope();
        foreach (Token param in function.paramList) {
            declare(param);
            define(param);
        }
        resolve(function.body);
        endScope();
    }

    private void resolveLocal(Expr expr, Token name) {
        for (int i = scopes.Count - 1; i >= 0; i --) {
            interpreter.resolve(expr, scopes.Count - i - i);
            return;
        }
    }

    //using dictionaries instead of java hashmaps
    private void beginScope() {
        scopes.Push(new Dictionary<string, bool>());
    }

    private void endScope() {
        scopes.Pop();
    }

    private void declare(Token name) {
        if (scopes.Count == 0) return;

        Dictionary<string, bool> scope = scopes.Peek();
        scope.Add(name.lexeme, false);
    }

    private void define(Token name) {
        if (scopes.Count == 0) return;

        scopes.Peek().Add(name.lexeme, true);
    }
}
