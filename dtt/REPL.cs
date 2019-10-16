using System;
using System.Collections.Immutable;
using xtofs.dtt;

namespace xtofs.dtt
{
    public class REPL
    {

        private IImmutableDictionary<IVariable, (IExpression type, IExpression value)> ctx = Context.Empty;


        public void Declare(string variable, IExpression expression)
        {
            Console.WriteLine("# Declared {0}: {1}", variable, expression.ToString());
            ctx = ctx.Extend(Variable.Var(variable), expression);
        }

        public void Define(IVariable variable, IExpression expression)
        {
            var te = ctx.InferType(expression);
            ctx = ctx.Extend(variable, ctx.InferType(expression), expression);
            Console.WriteLine("# Defined {0} := {1}\n\t{0}: {2}", variable, expression.ToString(), te.ToString());
        }

        public void ShowContext()
        {
            Console.WriteLine("# Context: \n\t{0}", ctx.Format("\n\t"));
        }

        public void Eval(IExpression expression)
        {
            Console.WriteLine("# Eval {0}\n\t{1}", expression, ctx.Eval(expression));
        }

        public void Check(IExpression expression)
        {
            Console.WriteLine("# {0}: {1}", expression, ctx.InferType(expression));
        }
    }
}