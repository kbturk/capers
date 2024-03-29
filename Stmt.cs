using System.IO;
using System;

namespace capers;

public interface VisitorStmt<T> {
    public T VisitBlock(Block e);
    public T VisitClass(Class e);
    public T VisitExpression(Expression e);
    public T VisitFunction(Function e);
    public T VisitIf(If e);
    public T VisitPrint(Print e);
    public T VisitReturnStmt(ReturnStmt e);
    public T VisitVar(Var e);
    public T VisitWhile(While e);
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

public class Class: Stmt {
    public Token name;
    public Variable superclass;
    public List<Function> methods;


    public Class(Token name, Variable superclass, List<Function> methods) {
      this.name = name;
      this.superclass = superclass;
      this.methods = methods;
    }

    public override T Accept<T>(VisitorStmt<T> visitor) {
      return visitor.VisitClass(this);
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

public class Function: Stmt {
    public Token name;
    public List<Token> paramList;
    public List<Stmt> body;


    public Function(Token name, List<Token> paramList, List<Stmt> body) {
      this.name = name;
      this.paramList = paramList;
      this.body = body;
    }

    public override T Accept<T>(VisitorStmt<T> visitor) {
      return visitor.VisitFunction(this);
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

public class ReturnStmt: Stmt {
    public Token keyword;
    public Expr value;


    public ReturnStmt(Token keyword, Expr value) {
      this.keyword = keyword;
      this.value = value;
    }

    public override T Accept<T>(VisitorStmt<T> visitor) {
      return visitor.VisitReturnStmt(this);
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

public class While: Stmt {
    public Expr condition;
    public Stmt body;


    public While(Expr condition, Stmt body) {
      this.condition = condition;
      this.body = body;
    }

    public override T Accept<T>(VisitorStmt<T> visitor) {
      return visitor.VisitWhile(this);
    }
}

