using System;

namespace capers;

public class VarEnvironment {
    protected Dictionary<string, object> values = new Dictionary<string, object>();

    //helper functions
    public object get(Token name) {
        if (values.ContainsKey(name.lexeme)) {
            return values[name.lexeme];
        }

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}.");
    }
    //Add things to the environment
    //TODO: add error handling if name isn't a string?
    public void define(string name, object val) {
            values.Add(name, val);
    }
}
