using System.IO;
using System;

namespace capers;

public interface VisitorExpr<T> {
    public T VisitAssign(Assign e);
    public T VisitBinary(Binary e);
    public T VisitGrouping(Grouping e);
    public T VisitLiteral(Literal e);
    public T VisitUnary(Unary e);
    public T VisitVariable(Variable e);
  }

public abstract class Expr {
    public abstract T Accept<T>(VisitorExpr<T> visitor);
}

public class Assign: Expr {
    public Token name;
    public Expr value;


    public Assign(Token name, Expr value) {
      this.name = name;
      this.value = value;
    }

    public override T Accept<T>(VisitorExpr<T> visitor) {
      return visitor.VisitAssign(this);
    }
}

public class Binary: Expr {
    public Expr left;
    public Token oper;
    public Expr right;


    public Binary(Expr left, Token oper, Expr right) {
      this.left = left;
      this.oper = oper;
      this.right = right;
    }

    public override T Accept<T>(VisitorExpr<T> visitor) {
      return visitor.VisitBinary(this);
    }
}

public class Grouping: Expr {
    public Expr expression;


    public Grouping(Expr expression) {
      this.expression = expression;
    }

    public override T Accept<T>(VisitorExpr<T> visitor) {
      return visitor.VisitGrouping(this);
    }
}

public class Literal: Expr {
    public object value;


    public Literal(object value) {
      this.value = value;
    }

    public override T Accept<T>(VisitorExpr<T> visitor) {
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

    public override T Accept<T>(VisitorExpr<T> visitor) {
      return visitor.VisitUnary(this);
    }
}

public class Variable: Expr {
    public Token name;


    public Variable(Token name) {
      this.name = name;
    }

    public override T Accept<T>(VisitorExpr<T> visitor) {
      return visitor.VisitVariable(this);
    }
}

