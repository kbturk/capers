namespace capers;

public class CapersFunction: CapersCallable {
    private Function declaration;
    private VarEnvironment closure;
    private bool isInitializer;
    public int Arity {get;set;}

    public CapersFunction(Function declaration, VarEnvironment closure,
            bool isInitializer) {
        this.declaration = declaration;
        this.closure = closure;
        this.isInitializer = isInitializer;
        this.Arity = declaration.paramList.Count;
    }

    public CapersFunction bind(CapersInstance instance) {
        VarEnvironment environment = new VarEnvironment(closure);
        environment.define("this", instance);
        return new CapersFunction(declaration, environment, isInitializer);
    }

    public object? Call(Interpreter interpreter,List<object?> arguments) {
        VarEnvironment environment= new VarEnvironment(closure);
        for (int i = 0; i < declaration.paramList.Count; i++) {
            environment.define(declaration.paramList[i].lexeme, arguments[i]);
        }

        try {
            interpreter.executeBlock(declaration.body, environment);
        } catch (Return returnValue) {
            if (isInitializer) return closure.getAt(0, "this");
            return returnValue.val;
        }

        if (isInitializer) return closure.getAt(0, "this");
        return null;
    }

    public override string ToString() {
        return $"<fn {declaration.name.lexeme}>";
    }

}

