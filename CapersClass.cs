namespace capers;

public class CapersClass: CapersCallable {
    public CapersClass? superclass;
    public string name;
    private Dictionary<string, CapersFunction> methods;
    public int Arity {get;set;}

    public CapersClass(string name, CapersClass? superclass,
            Dictionary<string, CapersFunction> methods) {
        this.superclass = superclass;
        this.name = name;
        this.methods = methods;
    }

    public CapersFunction? findMethod(string name) {
        if (methods.ContainsKey(name)) {
            return methods[name];
        }

        if (superclass != null) {
            return superclass.findMethod(name);
        }

        return null;
    }

    public override string ToString() {
        return name;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments) {
        CapersInstance instance = new CapersInstance(this);
        CapersFunction initializer = findMethod("init");
        if (initializer != null) {
            initializer.bind(instance).Call(interpreter, arguments);
        }

        if (initializer == null) {
            Arity = 0;
        } else {
            Arity = arguments.Count;
        }

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
        if (method != null) return method.bind(this);

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
