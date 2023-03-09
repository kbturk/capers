using System.IO;
using System;

namespace capers;

public interface VisitorStmt<T> {
    public T VisitBlock(Block e);
    public T VisitExpression(Expression e);
    public T VisitIf(If e);
    public T VisitPrint(Print e);
    public T VisitVar(Var e);
  }

public abstract class Stmt {
    public abstract T Accept<T>(VisitorStmt<T> visitor);
}

public class Block: Stmt {
    public List<Stmt> statements;


    public Block(List<Stmt> statements) {
      this.statements = statements;
    }

    public override T Accept<T>(VisitorStmt<T> visitor) {
      return visitor.VisitBlock(this);
    }
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

public class If: Stmt {
    public Expr condition;
    public Stmt thenBranch;
    public Stmt elseBranch;


    public If(Expr condition, Stmt thenBranch, Stmt elseBranch) {
      this.condition = condition;
      this.thenBranch = thenBranch;
      this.elseBranch = elseBranch;
    }

    public override T Accept<T>(VisitorStmt<T> visitor) {
      return visitor.VisitIf(this);
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

public class Var: Stmt {
    public Token name;
    public Expr initializer;


    public Var(Token name, Expr initializer) {
      this.name = name;
      this.initializer = initializer;
    }

    public override T Accept<T>(VisitorStmt<T> visitor) {
      return visitor.VisitVar(this);
    }
}

