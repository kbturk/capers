using System.IO;
using System;

namespace capers;

public class Interpreter: VisitorExpr<object>, VisitorStmt<Nullable<bool>>{

    //a dictionary to hold variables, starting with global values.
    public VarEnvironment globals = new VarEnvironment();
    private VarEnvironment environment = new VarEnvironment();
    private Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

    public Interpreter() {
        foreach (Builtin global in Builtin.BuiltinFunctions) {
            environment.define(global.Name, global);
        }
        environment.define("print_string",new Builtin("print_string",1,
                    (args) => {
                    Console.WriteLine(ValueString(args[0]));
                    return null;
                    }
                    )
                );
        globals = environment;
    }

    //Main API to Interpreter interface (ch 7.4)
    public void interpret(List<Stmt> statements) {
        try {
            foreach (Stmt statement in statements) {
                execute(statement);
            }
        } catch (RuntimeError error) {
            Capers.runtimeError(error);
        }
    }

    //Visitor Patterns for Expressions
    public object VisitAssign(Assign expr) {
        object val = evaluate(expr.value);

        if (locals.ContainsKey(expr)) {
            int distance = locals[expr];
            environment.assignAt(distance, expr.name, val);
        } else {
            globals.assign(expr.name, val);
        }
        return val;
    }

    public object VisitBinary(Binary expr) {
        object left = evaluate(expr.left);
        object right = evaluate(expr.right);

        //arithmetic expressions
        switch (expr.oper.type) {
            //comparisons
            case TokenType.GREATER:
                checkNumberOperand(expr.oper, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                checkNumberOperand(expr.oper, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                checkNumberOperand(expr.oper, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                checkNumberOperand(expr.oper, left, right);
                return (double)left <= (double)right;
            case TokenType.EQUAL_EQUAL:
                checkNumberOperand(expr.oper, left, right);
                return !isEqual(left, right);
            case TokenType.BANG_EQUAL:
                checkNumberOperand(expr.oper, left, right);
                return isEqual(left, right);

                //arithmetic
            case TokenType.MINUS:
                checkNumberOperand(expr.oper, left, right);
                return (double)left - (double)right;
            case TokenType.PLUS:
                if (left is double && right is double) {
                    return (double)left + (double)right;
                }
                //overloading to handle strings
                if (left is string && right is string) {
                    return (string)left + (string) right;
                }
                if (left is string && right is double) {
                    return (string)left + (string)right.ToString();
                }
                if (left is double && right is string) {
                    return (string)left.ToString() + (string)right;
                }
                throw new RuntimeError(expr.oper,
                        "Operands must be numbers or strings.");
            case TokenType.SLASH:
                checkNumberOperand(expr.oper, left, right);
                if (right is double && Convert.ToDouble(right) == 0) {
                    throw new RuntimeError(expr.oper, "Cannot divide by 0");
                }
                return (double)left / (double)right;
            case TokenType.STAR:
                if (left is string | right is string) {
                    return stringConcat(left, right);
                }
                checkNumberOperand(expr.oper, left, right);
                return (double)left * (double)right;
            default:
                throw new RuntimeError(expr.oper, "Could not match TokenType in Interpreter.VisitBinary");
        }

        //unreachable
        throw new RuntimeError(expr.oper, "Reached an unreachable condition in Interpreter.VisitBinary");
    }

    public object VisitCall(Call expr) {
        object callee = evaluate(expr.callee);

        List<object> arguments = new List<object>();
        foreach (Expr argument in expr.arguments) {
            arguments.Add(evaluate(argument));
        }

        if (!(callee is CapersCallable)) {
            throw new RuntimeError(expr.paren,
                    "Can only call functions and classes.");
        }

        CapersCallable function = (CapersCallable)callee;

        if (arguments.Count != function.Arity) {
            throw new RuntimeError(expr.paren,
                    $"Expected {function.Arity} arguments but got {arguments.Count}.");
        }

        return function.Call(this,arguments);
    }

    public object VisitGet(Get expr) {
        object obj = evaluate(expr.obj);
        if (obj is CapersInstance) {
            return ((CapersInstance) obj).get(expr.name);
        }

        throw new RuntimeError(expr.name, "Only instances have properties.");
    }

    public object VisitGrouping(Grouping expr) {
        return evaluate(expr.expression);
    }

    public object VisitLiteral(Literal expr) {
        return expr.value;
    }

    public object VisitLogical(Logical expr) {
        object left = evaluate(expr.left);

        if (expr.oper.type == TokenType.OR) {
            if (isTruthy(left)) return left;
        } else {
            //this is the "AND" eval: basically,
            //if the left side is false, bail out
            //early.
            if (!isTruthy(left)) return left;
        }

        return evaluate(expr.right);
    }

    public object VisitSet(Set expr) {
        object obj = evaluate(expr.obj);

        if (!(obj is CapersInstance)) {
            throw new RuntimeError(expr.name, "Only instances have fields.");
        }

        object val = evaluate(expr.value);
        ((CapersInstance)obj).set(expr.name, val);
        return val;
    }

    public object VisitThis(This expr) {
        return lookUpVariable(expr.keyword, expr);
    }

    public object VisitUnary(Unary expr) {
        object right = evaluate(expr.right);

        switch (expr.oper.type) {
            case TokenType.MINUS:
                checkNumberOperand(expr.oper, right);
                return -(double)right;
            case TokenType.BANG:
                return !isTruthy(right);
            default:
                throw new RuntimeError(expr.oper, $"{right} {expr.oper} cannot be evaluated in Interpreter.VisitUnary.");
        }

        //Should be unreachable...
        throw new RuntimeError(expr.oper, "Reached an unreachable condition in Interpreter.VisitUnary");
        return null;
    }

    public object VisitVariable(Variable expr) {
        return lookUpVariable(expr.name, expr);
    }

    public object lookUpVariable(Token name, Expr expr) {
        if (locals.ContainsKey(expr)) {
            return environment.getAt(locals[expr], name.lexeme);
        } else {
            return globals.get(name);
        }
    }

    //Visitor Patterns for Statements
    public bool? VisitBlock(Block stmt) {
        executeBlock(stmt.statements, new VarEnvironment(environment));
        return null;
    }

    public bool? VisitClass(Class stmt) {
        object superclass = null;
        if (stmt.superclass != null) {
            superclass = evaluate(stmt.superclass);
            if(!(superclass is CapersClass)) {
                throw new RuntimeError(stmt.superclass.name,
                        "Superclass must be a class.");
            }
        }

        environment.define(stmt.name.lexeme, null);

        Dictionary<string, CapersFunction> methods = new Dictionary<string, CapersFunction>();
        foreach (Function method in stmt.methods) {
            CapersFunction function = new CapersFunction(method, environment, method.name.lexeme == "init");
            methods.Add(method.name.lexeme, function);
        }
        CapersClass klass = new CapersClass(stmt.name.lexeme, methods);
        environment.assign(stmt.name, klass);
        return null;
    }

    public bool? VisitExpression(Expression stmt) {
        evaluate(stmt.expression);
        return null;
    }

    public bool? VisitFunction(Function stmt) {
        CapersFunction function = new CapersFunction(stmt, environment, false);
        environment.define(stmt.name.lexeme, function);
        return null;
    }

    public bool? VisitIf(If stmt) {
        if (isTruthy(evaluate(stmt.condition))) {
            execute(stmt.thenBranch);
        } else if (stmt.elseBranch != null) {
            execute(stmt.elseBranch);
        }
        return null;
    }

    public bool? VisitPrint(Print stmt) {
        object val = evaluate(stmt.expression);
        Console.WriteLine(stringify(val));
        return null;
    }

    public bool? VisitReturnStmt(ReturnStmt stmt) {
        object val = null;
        if (stmt.value != null) val = evaluate(stmt.value);

        throw new Return(val);
    }

    public bool? VisitVar(Var stmt) {
        object val = null;
        if (stmt.initializer != null) {
            val = evaluate(stmt.initializer);
        }

        environment.define(stmt.name.lexeme, val);
        return null;
    }

    public bool? VisitWhile(While stmt) {
        while (isTruthy(evaluate(stmt.condition))) {
            execute(stmt.body);
        }
        return null;
    }

    //helper functions
    private void checkNumberOperand(Token oper, object operand) {
        if (operand is double) return;
        throw new RuntimeError(oper, "Operand must be a number.");
    }

    private void checkNumberOperand(Token oper, object left, object right) {
        if (left is double && right is double) return;
        throw new RuntimeError(oper, "Operands must be numbers.");
    }

    private object evaluate(Expr expr) {
        return expr.Accept(this);
    }

    private void execute(Stmt stmt) {
        stmt.Accept(this);
    }

    public void resolve(Expr expr, int depth) {
        locals.Add(expr, depth);
    }

    public void executeBlock(List<Stmt> statements, VarEnvironment environment) {
        VarEnvironment previous = this.environment;
        try {
            this.environment = environment;

            foreach (Stmt statement in statements) {
                execute(statement);
            }
        } finally {
            this.environment = previous;
        }
    }

    //Some goofy string concatinations
    private string? stringify(object obj) {
        if (obj == null) return "nil";

        return obj.ToString();
    }

    //a little more useful string function
    private static string ValueString(object? val) => val switch
    {
        null => "nil",
             true => "he speaks the true - true",
             false => "falsey",
             double d => d.ToString(),
             string s => $"\"{s}\"",
             _ => throw new RuntimeError(val, "invalid runtime value")
    };


    //custom capers string helper functions
    //example: 3 * "dog" or "dog" * 3 = "dogdogdog" 
    private string stringConcat(object a, object b) {
        var sb = new System.Text.StringBuilder();
        if (a is string) {
            int c;
            switch (b) {
                case int:
                case float:
                case long:
                case decimal:
                case double:
                    //cast to int
                    c = Convert.ToInt32(b);
                    break;
                default:
                    throw new RuntimeError(b, "Cannot multiply string by Operand. Operand must be a number");
            }

            for (int i = 0; i < c; i++) {
                sb.Append((string)a);
            }

            return sb.ToString();
        }
        //if a wasn't a string but b is
        if (b is string) { return stringConcat(b,a); }

        throw new RuntimeError(a, "Blowup in stringConcat. Neither operand is a string");
    }

    //null and false are false
    //everything else is true
    private bool isTruthy(object obj) {
        if (obj == null) return false;
        if (obj is bool) return (bool) obj;
        return true;
    }

    //do I care of a & b are both null?
    private bool isEqual(object a, object b) {
        if (a == null && b == null) return true;
        if (a == null) return false;
        //There's some subtlety to this
        return a.Equals(b);
    }
}

public class RuntimeError: Exception {
    public Token token;
    public string message;

    public RuntimeError(Token token, String message) {
        this.token = token;
        this.message = message;
    }

    public RuntimeError(object obj, String message) {
        this.token = new Token(TokenType.NIL, obj.ToString(), obj, -9999);
        this.message = message;
    }

}
