using System;
using System.Collections.Immutable;
using xtofs.dtt;

namespace console
{
    class Program
    {
        static void Main(string[] args)
        {
            var repl = new REPL();

            repl.Declare("N", Expression.Universe(0));
            repl.Declare("z", Expression.Var("N"));
            repl.Declare("s", Expression.Pi(Variable.Var("p"), Expression.Var("N"), Expression.Var("N")));
            repl.ShowContext();

            var N = Expression.Var("N");
            var N2N = Expression.Pi(N, N);

            repl.Define(Variable.Var("triple"),
                Expression.Lambda(Variable.Var("f"), N2N,
                    Expression.Lambda(Variable.Var("x"), N,
                        Expression.App(Expression.Var("f"),
                            Expression.App(Expression.Var("f"),
                                Expression.App(Expression.Var("f"),
                                    Expression.Var("x")))))));
            repl.ShowContext();

            var e = Expression.App(Expression.Var("triple"), Expression.App(Expression.Var("triple"), Expression.Var("s")));
            repl.Check(e);

            repl.Eval(Expression.App(e, Expression.Var("z")));
        }
    }

}
