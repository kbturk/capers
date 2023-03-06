using System.IO;
using System;

namespace capers;

public class Interpreter: Visitor<object> {

    //Main API to Interpreter interface (ch 7.4)
    public void interpret(Expr expression) {
        try {
            object value = evaluate(expression);
            Console.WriteLine(stringify(value));
        } catch (RuntimeError error) {
            Capers.runtimeError(error);
        }
    }

    public object VisitLiteral(Literal expr) {
        return expr.value;
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
                throw new RuntimeError(expr.oper,
                        "Operands must be two numbers or two strings.");
            case TokenType.SLASH:
                checkNumberOperand(expr.oper, left, right);
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

    public object VisitGrouping(Grouping expr) {
        return evaluate(expr.expression);
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

    private string stringify(object obj) {
        if (obj == null) return "nil";

        if (obj is double) {
            string text = obj.ToString();
            //TODO: This is Java's syntax for inter-valued doubles.
            //Look up C#'s equivalent
            if (text.EndsWith(".0")) {
                text = text.Substring(0, text.Length - 2);
            }
            return text;
        }

        return obj.ToString();
    }

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