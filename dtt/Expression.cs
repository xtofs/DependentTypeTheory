using System;

namespace xtofs.dtt
{

    // http://math.andrej.com/2012/11/08/how-to-implement-dependent-type-theory-i/



    //     (** Abstract syntax of expressions. *)
    // type expr =
    //   | Var of variable
    //   | Universe of int
    //   | Pi of abstraction
    //   | Lambda of abstraction
    //   | App of expr * expr

    // (** An abstraction [(x,t,e)] indicates that [x] of type [t] is bound in [e]. *)
    // and abstraction = variable * expr * expr
    // We choose a concrete syntax that is similar to that of Coq:

    // universes are written Type 0, Type 1, Type 2, …
    // the dependent product is written forall x : A, B,
    // a function is written fun x : A => B,
    // application is juxtaposition e1 e2.
    // If x does not appear freely in B, then we write A -> B instead of forall x : A, B.
    public interface IExpression
    {
    }

    public static class Expression
    {
        public static VariableExpression Var(string name) => new VariableExpression(Variable.Var(name));

        public static VariableExpression Var(IVariable variable) => new VariableExpression(variable);

        public static VariableExpression Refresh(this VariableExpression variable) => Var(variable.Variable.Refresh());

        public static UniverseExpression Universe(int n) => new UniverseExpression(n);

        public static PiExpression Pi(IVariable v, IExpression a, IExpression b) => new PiExpression(v, a, b);

        public static PiExpression Pi(IExpression a, IExpression b) => new PiExpression(Variable.Dummy, a, b);

        public static LambdaExpression Lambda(IVariable v, IExpression a, IExpression b) => new LambdaExpression(v, a, b);

        public static AppExpression App(IExpression a, IExpression b) => new AppExpression(a, b);

        public static bool IsFreeVariable(this IExpression expression, IVariable v)
        {
            switch (expression)
            {
                case VariableExpression(var x):
                    {
                        return x.Equals(v);
                    }
                case UniverseExpression(var gen):
                    {
                        return false;
                    }
                case PiExpression(var x, var t1, var t2):
                    {
                        return t1.IsFreeVariable(v) || (!x.Equals(v) && t2.IsFreeVariable(v));
                    }
                case LambdaExpression(var x, var t, var e):
                    {
                        return t.IsFreeVariable(v) || (!x.Equals(v) && e.IsFreeVariable(v));
                    }
                case AppExpression(var e1, var e2):
                    {
                        return e1.IsFreeVariable(v) || e2.IsFreeVariable(v);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

    }

    public class VariableExpression : IExpression
    {
        public IVariable Variable { get; }

        public VariableExpression(IVariable variable)
        {
            this.Variable = variable;
        }

        // public static implicit operator VariableExpression(IVariable variable) => new VariableExpression(variable);

        public void Deconstruct(out IVariable variable)
        {
            variable = Variable;
        }

        public override string ToString() => $"{Variable}";
    }

    public class UniverseExpression : IExpression
    {
        public int Generation { get; }

        public UniverseExpression(int generation)
        {
            this.Generation = generation;
        }

        public void Deconstruct(out int generation)
        {
            generation = Generation;
        }

        static char[] subscripts = "₀₁₂₃₄₅₆₇₈₉".ToCharArray();

        public override string ToString() => Generation < 10 ? $"Type{subscripts[Generation]}" : $"Type{Generation}";
    }

    public class PiExpression : IExpression
    {
        public PiExpression(IVariable v, IExpression t, IExpression e)
        {
            V = v;
            T = t;
            E = e;
        }

        public void Deconstruct(out IVariable v, out IExpression t, out IExpression e)
        {
            v = V;
            t = T;
            e = E;
        }

        public IVariable V { get; }
        public IExpression T { get; }
        public IExpression E { get; }

        public override string ToString() => E.IsFreeVariable(V) ? $"forall {V} : {T}, {E}" : $"({T} -> {E})";
    }

    public class LambdaExpression : IExpression
    {
        public LambdaExpression(IVariable v, IExpression t, IExpression e)
        {
            V = v;
            T = t;
            E = e;
        }

        public void Deconstruct(out IVariable v, out IExpression t, out IExpression e)
        {
            v = V;
            t = T;
            e = E;
        }

        public IVariable V { get; }
        public IExpression T { get; }
        public IExpression E { get; }

        public override string ToString() => $"fun {V} : {T} => {E}";
    }

    public class AppExpression : IExpression
    {
        public AppExpression(IExpression f, IExpression e)
        {
            F = f;
            E = e;
        }

        public void Deconstruct(out IExpression t, out IExpression e)
        {
            t = F;
            e = E;
        }

        public IExpression F { get; }
        public IExpression E { get; }

        public override string ToString() => $"({F} {E})";
    }
}
