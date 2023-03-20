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
    public void define(string name, object? val) {
        //Troubleshooting code
//      Console.WriteLine($"current dictionary:");
//      foreach ((string key, object value2) in values) {
//          Console.WriteLine($"{key},{value2}");
//      }
//      Console.WriteLine($"Trying to add {name}, {val}");
        if (values.ContainsKey(name)){
            values[name] = val;
        } else {
            values.Add(name, val);
        }
    }

    public object getAt(int distance, string name) {
        //Console.WriteLine($"sending {distance}, {name} to ancestor");
        return ancestor(distance).values[name];
    }

    public void assignAt(int distance, Token name, object val) {
        if (ancestor(distance).values.ContainsKey(name.lexeme)) {
            ancestor(distance).values[name.lexeme] = val;
        } else {
        ancestor(distance).values.Add(name.lexeme, val);
        }
    }

    public VarEnvironment ancestor(int distance) {
        VarEnvironment environment = this;
        for (int i = 0; i < distance; i ++) {
//          foreach ((string key, object val) in environment.values) {
//              Console.WriteLine($"i: {i}, {key},{val}");
//          }
            environment = environment.enclosing;
        }
        //Console.WriteLine($"done finding environment:");
        return environment;
    }
}
