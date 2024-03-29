using System.IO;
using System.Text;
using System.Linq;
using System;

namespace capers;

public class PrettyPrinter: VisitorExpr<string> {
    public string print(Expr expr) {
        return expr.Accept(this);
    }

    public string VisitAssign(Assign expr) {
        return expr.name.lexeme;
    }

    public string VisitBinary(Binary expr) {
        return parenthesize(expr.oper.lexeme,
                expr.left, expr.right);
    }

    public string VisitCall(Call expr) {
        StringBuilder builder = new StringBuilder();
        builder.Append(expr.callee);
        builder.Append($"({expr.arguments})");
        return builder.ToString();
    }

    public string VisitGet(Get expr){
        return $"{expr.Accept(this)}[{expr.name.lexeme}]";
    }

    public string VisitGrouping(Grouping expr){
        return parenthesize("group", expr.expression);
    }

    public string VisitLiteral(Literal expr){
        if (expr.value == null) return "nil";
        return expr.value.ToString();
    }

    public string VisitLogical(Logical expr) {
        return parenthesize(expr.oper.lexeme, expr.left, expr.right);
    }

    public string VisitSet(Set expr) {
        return $"{expr.obj.ToString()} = {expr.Accept(this)}";
    }

    public string VisitSuper(Super expr) {
        return $"this is a weird thing to ask to print but whatever: {expr}";
    }

    public string VisitThis(This expr) {
        return $"this is a weird thing to ask to print but whatever: {expr}";
    }

    public string VisitUnary(Unary expr) {
        return parenthesize(expr.oper.lexeme, expr.right);
    }

    public string VisitVariable(Variable expr) {
        return expr.name.lexeme; 
    }

    public string parenthesize(string name, params Expr[] exprs) {
        StringBuilder builder = new StringBuilder();

        builder.Append("(");
        builder.Append(name);
        foreach (Expr expr in exprs) {
            builder.Append(" ");
            builder.Append(expr.Accept(this));
        }
        builder.Append(")");

        return builder.ToString();

    }

}
