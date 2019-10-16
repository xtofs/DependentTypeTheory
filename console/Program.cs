using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace console
{
    class Program
    {
        static void Main(string[] args)
        {

            Declare("N", Expression.Universe(0));
            Declare("z", Expression.Var("N"));
            Declare("s", Expression.Pi(Variable.Var("p"), Expression.Var("N"), Expression.Var("N")));
            ShowContext();

            var N = Expression.Var("N");
            var N2N = Expression.Pi(Variable.Dummy, N, N);

            Define(Variable.Var("triple"),
                Expression.Lambda(Variable.Var("f"), N2N,
                    Expression.Lambda(Variable.Var("x"), N,
                        Expression.App(Expression.Var("f"),
                            Expression.App(Expression.Var("f"),
                                Expression.App(Expression.Var("f"),
                                    Expression.Var("x")))))));
            ShowContext();

            var e = Expression.App(Expression.Var("triple"), Expression.App(Expression.Var("triple"), Expression.Var("s")));
            Check(e);

            Eval(Expression.App(e, Expression.Var("z")));
        }


        private static IImmutableDictionary<IVariable, (IExpression type, IExpression value)> ctx = Context.Empty;


        private static void Declare(string variable, IExpression expression)
        {
            Console.WriteLine("# Declared {0}: {1}", variable, expression.ToString());
            ctx = ctx.Extend(Variable.Var(variable), expression);
        }

        private static void Define(IVariable variable, IExpression expression)
        {
            var te = ctx.InferType(expression);
            ctx = ctx.Extend(variable, ctx.InferType(expression), expression);
            Console.WriteLine("# Defined {0} := {1}\n\t{0}: {2}", variable, expression.ToString(), te.ToString());
        }

        private static void ShowContext()
        {
            Console.WriteLine("# Context: \n\t{0}", ctx.Format("\n\t"));
        }

        private static void Eval(IExpression expression)
        {
            Console.WriteLine("# Eval {0}\n\t{1}", expression, ctx.Eval(expression));
        }

        private static void Check(IExpression expression)
        {
            Console.WriteLine("# {0}: {1}", expression, ctx.InferType(expression));
        }
    }
}
