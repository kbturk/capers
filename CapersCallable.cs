namespace capers;

public interface CapersCallable {
    public int Arity {get;}
    public object? Call(Interpreter interpreter, List<object?> arguments);
}

//New globals go here
//TODO: Make a print builtin,
//TODO: make a builtin that prints all variables in the environments.
internal record class Builtin(string Name, int Arity, Func<List<object?>, object?> Function): CapersCallable {
    public object? Call(Interpreter interpreter, List<object?> arguments) => Function(arguments);
    public override string ToString() => $"<native fn {Name}>";

    public static List<Builtin> BuiltinFunctions = new List<Builtin> {
        new Builtin("clock",0,
                (_) => (double)Environment.TickCount/ 1000.0),
            new Builtin("read_line", 0,
                    (_) => System.Console.ReadLine()),
            new Builtin("square",1, (arg) => {
                    if (arg[0] is double) {
                    return (double)arg[0] * (double)arg[0];
                    } else {
                    return null;
                    }
                    })
    };
}
