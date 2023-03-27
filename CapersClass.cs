namespace capers;

public class CapersClass: CapersCallable {
    public string name;
    private Dictionary<string, CapersFunction> methods;
    public int Arity {get;} = 0;

    public CapersClass(string name, Dictionary<string, CapersFunction>
            methods) {
        this.name = name;
        this.methods = methods;
    }

    public CapersFunction? findMethod(string name) {
        if (methods.ContainsKey(name)) {
            return methods[name];
        }

        return null;
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

        CapersFunction method = klass.findMethod(name.lexeme);
        if (method != null) return method;

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
