using System.IO;
using System;

namespace capers;

public interface VisitorStmt<T> {
    public T VisitExpression(Expression e);
    public T VisitPrint(Print e);
}

public abstract class Stmt {
    public abstract T Accept<T>(VisitorStmt<T> visitor);
}

public class Expression: Stmt {
    public Expr expression;

    public Expression(Expr expression) {
        this.expression = expression;
    }

    public override T Accept<T>(VisitorStmt<T> visitor) {
        return visitor.VisitExpression(this);
    }
}

public class Print: Stmt {
    public Expr expression;

    public Print(Expr expression) {
        this.expression = expression;
    }

    public override T Accept<T>(VisitorStmt<T> visitor) {
        return visitor.VisitPrint(this);
    }
}

