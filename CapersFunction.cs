namespace capers;

public class CapersFunction: CapersCallable {
    private Function declaration;
    private VarEnvironment closure;

    public CapersFunction(Function declaration, VarEnvironment closure) {
        this.declaration = declaration;
        this.closure = closure;
    }
    public int Arity => declaration.paramList.Count;


    public object? Call(Interpreter interpreter,List<object?> arguments) {
        VarEnvironment environment= new VarEnvironment(closure);
        for (int i = 0; i < declaration.paramList.Count; i++) {
            environment.define(declaration.paramList[i].lexeme, arguments[i]);
        }

        try {
            interpreter.executeBlock(declaration.body, environment);
        } catch (Return returnValue) {
            return returnValue.val;
        }
        return null;
    }

    public override string ToString() {
        return $"<fn {declaration.name.lexeme}>";
    }

}

