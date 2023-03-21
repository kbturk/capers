namespace capers;

public class CapersClass: CapersCallable {
    public string name;
    public int Arity {get;} = 0;

    public CapersClass(string name) {
        this.name = name;
    }

    public override string ToString() {
        return name;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments) {
        CapersInstance instance = new CapersInstance(this);
        return instance;
    }

}

public class CapersInstance {
    private CapersClass klass;

    public CapersInstance(CapersClass klass) {
        this.klass = klass;
    }

    public override string ToString() {
        return $"{klass.name} instance";
    }
}
