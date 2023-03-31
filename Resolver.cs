using System.Collections;
using System;

namespace capers;

public class Resolver: VisitorExpr<Nullable<bool>>, VisitorStmt<Nullable<bool>>{
    private Interpreter interpreter;
    private Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
    private FunctionType currentFunction = FunctionType.NO;
    private enum ClassType {
        NONE,
        CLASS,
        SUBCLASS
    }

    private ClassType currentClass = ClassType.NONE;

    public Resolver(Interpreter interpreter) {
        this.interpreter = interpreter;
    }

    //Private enums?
    private enum FunctionType {
        NO,
        YES,
        INITIALIZER,
        METHOD
    }

    //Stmt Visitor Patterns
    public bool? VisitBlock(Block stmt) {
        beginScope();
        resolve(stmt.statements);
        endScope();
        return null;
    }

    public bool? VisitClass(Class stmt) {
        ClassType enclosingClass = currentClass;
        currentClass = ClassType.CLASS;

        declare(stmt.name);
        define(stmt.name);

        if (stmt.superclass != null && 
                stmt.name.lexeme == stmt.superclass.name.lexeme) {
            Capers.error(stmt.superclass.name,
                    "A class can't inherit from itself.");
        }

        if (stmt.superclass != null) {
            currentClass = ClassType.SUBCLASS;
            resolve(stmt.superclass);
        }

        if (stmt.superclass != null) {
            beginScope();
            scopes.Peek().Add("super", true);
        }

        beginScope();
        if (scopes.Peek().ContainsKey("this")) {
            scopes.Peek()["this"] = true;
        } else {
            scopes.Peek().Add("this", true);
        }

        foreach (Function method in stmt.methods) {
            FunctionType declaration = FunctionType.METHOD;
            if (method.name.lexeme == "init") {
                declaration = FunctionType.INITIALIZER;
            }
            resolveFunction(method, declaration);
        }

        endScope();

        if (stmt.superclass != null) endScope();

        currentClass = enclosingClass;

        return null;
    }

    public bool? VisitExpression(Expression stmt) {
        resolve(stmt.expression);
        return null;
    }

    public bool? VisitFunction(Function stmt) {
        declare(stmt.name);
        define(stmt.name);

        resolveFunction(stmt, FunctionType.YES);
        return null;
    }

    public bool? VisitIf(If stmt) {
        resolve(stmt.condition);
        resolve(stmt.thenBranch);
        if (stmt.elseBranch != null) resolve(stmt.elseBranch);
        return null;
    }

    public bool? VisitPrint(Print stmt) {
        resolve(stmt.expression);
        return null;
    }

    public bool? VisitReturnStmt(ReturnStmt stmt) {
        if (currentFunction == FunctionType.NO) {
            Capers.error(stmt.keyword, "Can't return from top-level code.");
        }
        if (stmt.value != null) {
            if (currentFunction == FunctionType.INITIALIZER) {
                Capers.error(stmt.keyword,
                        "Can't return a value from an initializer");
            }
            resolve(stmt.value);
        }
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
        resolve(stmt.condition);
        resolve(stmt.body);
        return null;
    }

    //Expr Visitor Patterns
    public bool? VisitAssign(Assign expr) {
        resolve(expr.value);
        resolveLocal(expr, expr.name);
        return null;
    }

    public bool? VisitBinary(Binary expr) {
        resolve(expr.left);
        resolve(expr.right);
        return null;
    }

    public bool? VisitCall(Call expr) {
        resolve(expr.callee);

        foreach (Expr argument in expr.arguments) {
            resolve(argument);
        }

        return null;
    }

    public bool? VisitGet(Get expr) {
        resolve(expr.obj);
        return null;
    }

    public bool? VisitGrouping(Grouping expr) {
        resolve(expr.expression);
        return null;
    }

    public bool? VisitLiteral(Literal expr) {
        return null;
    }

    public bool? VisitLogical(Logical expr) {
        resolve(expr.right);
        resolve(expr.left);
        return null;
    }

    public bool? VisitSet(Set expr) {
        resolve(expr.value);
        resolve(expr.obj);
        return null;
    }

    public bool? VisitSuper(Super expr) {
        if (currentClass == ClassType.NONE) {
            Capers.error(expr.keyword, "Can't use 'super' outside of a class.");
        } else if (currentClass != ClassType.SUBCLASS) {
            Capers.error(expr.keyword,
                    "Can't use 'super' in a class with no superclass.");
        }

        resolveLocal(expr, expr.keyword);
        return null;
    }

    public bool? VisitThis(This expr) {
        if (currentClass == ClassType.NONE) {
            Capers.error(expr.keyword,
                    "Can't use 'this' outside of a class.");
            return null;
        }
        resolveLocal(expr, expr.keyword);
        return null;
    }

    public bool? VisitUnary(Unary expr) {
        resolve(expr.right);
        return null;
    }

    public bool? VisitVariable(Variable expr) {

        if (!(scopes.Count == 0)) {
            if (scopes.Peek().ContainsKey(expr.name.lexeme) &&
                    scopes.Peek()[expr.name.lexeme] == false) {
                Capers.error(expr.name, "Can't read local variable in its own initializer.");
            }
        }

        resolveLocal(expr, expr.name);
        return null;
    }

    //Private helper functions

    //Resolve overload functions
    public void resolve(List<Stmt> statements) {
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

    private void resolveFunction(Function function, FunctionType type) {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = type;
        beginScope();
        foreach (Token param in function.paramList) {
            declare(param);
            define(param);
        }
        resolve(function.body);
        endScope();
        currentFunction = enclosingFunction;
    }

    private void resolveLocal(Expr expr, Token name) {
        var i = 0;
        foreach (Dictionary<string, bool> stack in scopes) {
            if (stack.ContainsKey(name.lexeme)){
                interpreter.resolve(expr,i);
                return;
            }
            i++;
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
        if (scope.ContainsKey(name.lexeme)) {
            Capers.error(name,
                    "There is already a variable with this name in this scope.");
        }
        scope.Add(name.lexeme, false);
    }

    private void define(Token name) {
        if (scopes.Count == 0) return;
        scopes.Peek()[name.lexeme] = true;
    }
}
