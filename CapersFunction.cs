namespace capers;

public class CapersFunction: CapersCallable {
    private Function declaration;
    public CapersFunction(Function declaration) {
        this.declaration = declaration;
    }

    public int Arity => declaration.paramList.Count;


    public object? Call(Interpreter interpreter,List<object?> arguments) {
        VarEnvironment environment= new VarEnvironment(interpreter.globals);
        for (int i = 0; i < declaration.paramList.Count; i++) {
            environment.define(declaration.paramList[i].lexeme, arguments[i]);
        }

        interpreter.executeBlock(declaration.body, environment);
        return null;
    }

    public override string ToString() {
        return $"<fn {declaration.name.lexeme}>";
    }

}

