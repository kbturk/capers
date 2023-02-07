using System.IO;
using System;

namespace capers;

public interface Visitor<T> {
    public T VisitLiteral(Literal e);
    public T VisitBinary(Binary e);
    public T VisitGrouping(Grouping e);
    public T VisitUnary(Unary e);
}

public abstract class Expr {
    public abstract T Accept<T>(Visitor<T> visitor);
}

public class Binary:Expr {
    public Expr left;
    public Token oper;
    public Expr right;

    public Binary(Expr left, Token oper, Expr right){
        this.left = left;
        this.oper = oper;
        this.right = right;
    }

    public override T Accept<T>(Visitor<T> visitor) {
        return visitor.VisitBinary(this);
    }

}

public class Grouping: Expr {
    public Expr expression;

    public Grouping(Expr expression){
        this.expression = expression;
    }

    public override T Accept<T>(Visitor<T> visitor) {
        return visitor.VisitGrouping(this);
    }
}

public class Literal: Expr {
    public object value;

    public Literal(object value){
        this.value = value;
    }

    public override T Accept<T>(Visitor<T> visitor) {
        return visitor.VisitLiteral(this);
    }
}

public class Unary: Expr {
    public Token oper;
    public Expr right;

    public Unary(Token oper, Expr right) {
        this.oper = oper;
        this.right = right;
    }

    public override T Accept<T>(Visitor<T> visitor) {
        return visitor.VisitUnary(this);
    }    
}
