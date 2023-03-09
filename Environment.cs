using System;

namespace capers;

public class VarEnvironment {
    protected Dictionary<string, object> values = new Dictionary<string, object>();
    public VarEnvironment enclosing;

    public VarEnvironment() {
        enclosing = null;
    }

    public VarEnvironment(VarEnvironment enclosing) {
        this.enclosing = enclosing;
    }

    //helper functions
    public object get(Token name) {
        if (values.ContainsKey(name.lexeme)) {
            return values[name.lexeme];
        }

        if (enclosing != null) return enclosing.get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    public void assign(Token name, object val) {
        if (values.ContainsKey(name.lexeme)) {
            values[name.lexeme] = val;
            return;
        }

        if (enclosing != null) {
            enclosing.assign(name, val);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable ' {name.lexeme}'.");
    }

    //Add things to the environment
    //TODO: add error handling if name isn't a string?
    public void define(string name, object val) {
            values.Add(name, val);
    }
}
