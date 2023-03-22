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
    private Dictionary<string, object> fields = new Dictionary<string, object>();

    public CapersInstance(CapersClass klass) {
        this.klass = klass;
    }

    public object get(Token name) {
        if (fields.ContainsKey(name.lexeme)) {
            return fields[name.lexeme];
        }

        throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
    }

    public void set(Token name, object val) {
        if (fields.ContainsKey(name.lexeme)) {
            fields[name.lexeme] = val;
        } else {
            fields.Add(name.lexeme, val);
        }
    }

    public override string ToString() {
        return $"{klass.name} instance";
    }
}
