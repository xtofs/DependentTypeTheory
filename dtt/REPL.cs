using System;
using System.Collections.Immutable;
using xtofs.dtt;

namespace xtofs.dtt
{
    public class REPL
    {

        private IImmutableDictionary<IVariable, (Expression type, Expression value)> ctx = Context.Empty;


        public void Declare(string variable, Expression expression)
        {
            Console.WriteLine("# Declare {0}: {1}", variable, expression.ToString());
            ctx = ctx.Extend(Variable.Var(variable), expression);
        }

        public void Define(IVariable variable, Expression expression)
        {
            var te = ctx.InferType(expression);
            ctx = ctx.Extend(variable, te, expression);
            Console.WriteLine("# Define {0} := {1}\n\t{0}: {2}", variable, expression.ToString(), te.ToString());
        }

        public void ShowContext()
        {
            Console.WriteLine("# Context: \n\t{0}", ctx.Format("\n\t"));
        }

        public void Eval(Expression expression)
        {
            Console.WriteLine("# Eval {0}\n\t{1}", expression, ctx.Eval(expression));
        }

        public void Check(Expression expression)
        {
            Console.WriteLine("# Check {0}:\n\t{1}", expression, ctx.InferType(expression));
        }
    }
}