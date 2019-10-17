using System;

namespace xtofs.dtt
{



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
    public abstract class Expression : IEquatable<Expression>
    {
        public abstract bool Equals(Expression other);

        public static VariableExpression Var(string name) => new VariableExpression(Variable.Var(name));

        public static VariableExpression Var(IVariable variable) => new VariableExpression(variable);

        public static UniverseExpression Universe(int n) => new UniverseExpression(n);

        public static PiExpression Pi(IVariable v, Expression a, Expression b) => new PiExpression(v, a, b);

        public static PiExpression Pi(Expression a, Expression b) => new PiExpression(Variable.Dummy, a, b);

        public static LambdaExpression Lambda(IVariable v, Expression a, Expression b) => new LambdaExpression(v, a, b);

        public static AppExpression App(Expression a, Expression b) => new AppExpression(a, b);
    }

    public static class ExpressionExtensions
    {
        public static bool IsFreeVariable(this Expression expression, IVariable v)
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

    public class VariableExpression : Expression, IEquatable<VariableExpression>
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

        #region equality
        public override int GetHashCode() => Variable.GetHashCode();

        public override bool Equals(object other)
        {
            return other is VariableExpression s && Equals(s);
        }

        public override bool Equals(Expression other)
        {
            return other is VariableExpression s && Equals(s);
        }

        public bool Equals(VariableExpression other)
        {
            return other.Variable.Equals(this.Variable);
        }
        #endregion
    }

    public class UniverseExpression : Expression, IEquatable<UniverseExpression>
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

        #region equality
        public override int GetHashCode() => Generation.GetHashCode();

        public override bool Equals(object other)
        {
            return other is UniverseExpression s && Equals(s);
        }

        public override bool Equals(Expression other)
        {
            return other is UniverseExpression s && Equals(s);
        }

        public bool Equals(UniverseExpression other)
        {
            return other.Generation == this.Generation;
        }
        #endregion
    }

    public class PiExpression : Expression, IEquatable<PiExpression>
    {
        public PiExpression(IVariable v, Expression t, Expression e)
        {
            V = v;
            T = t;
            E = e;
        }

        public void Deconstruct(out IVariable v, out Expression t, out Expression e)
        {
            v = V;
            t = T;
            e = E;
        }

        public IVariable V { get; }
        public Expression T { get; }
        public Expression E { get; }

        // public override string ToString() => E.IsFreeVariable(V) ? $"forall {V} : {T}, {E}" : $"({T} -> {E})";
        public override string ToString() => $"forall {V} : {T}, {E}";

        #region equality
        public override int GetHashCode() => HashCode.Combine(V, T, E);

        public override bool Equals(object other)
        {
            return other is PiExpression s && Equals(s);
        }

        public override bool Equals(Expression other)
        {
            return other is PiExpression s && Equals(s);
        }

        public bool Equals(PiExpression other)
        {
            var oe = Substitution.Single(this.V, Expression.Var(other.V)).Apply(other.E);
            return this.T.Equals(other.T) && this.E.Equals(oe);
        }
        #endregion
    }

    public class LambdaExpression : Expression, IEquatable<LambdaExpression>
    {
        public LambdaExpression(IVariable v, Expression t, Expression e)
        {
            V = v;
            T = t;
            E = e;
        }

        public void Deconstruct(out IVariable v, out Expression t, out Expression e)
        {
            v = V;
            t = T;
            e = E;
        }

        public IVariable V { get; }
        public Expression T { get; }
        public Expression E { get; }

        public override string ToString() => $"fun {V} : {T} => {E}";

        #region equality
        public override int GetHashCode() => HashCode.Combine(V, T, E);

        public override bool Equals(object other)
        {
            return other is LambdaExpression s && Equals(s);
        }

        public override bool Equals(Expression other)
        {
            return other is LambdaExpression s && Equals(s);
        }

        public bool Equals(LambdaExpression other)
        {
            var oe = Substitution.Single(this.V, Expression.Var(other.V)).Apply(other.E);
            return this.T.Equals(other.T) && this.E.Equals(oe);
        }
        #endregion
    }

    public class AppExpression : Expression, IEquatable<AppExpression>
    {
        public AppExpression(Expression f, Expression e)
        {
            F = f;
            E = e;
        }

        public void Deconstruct(out Expression t, out Expression e)
        {
            t = F;
            e = E;
        }

        public Expression F { get; }
        public Expression E { get; }

        public override string ToString() => $"({F} {E})";

        #region equality
        public override int GetHashCode() => HashCode.Combine(F, E);

        public override bool Equals(object other)
        {
            Console.WriteLine(other);
            return other is AppExpression s && Equals(s);
        }

        public override bool Equals(Expression other)
        {
            return other is AppExpression s && Equals(s);
        }

        public bool Equals(AppExpression other)
        {
            return this.F.Equals(other.F) && this.E.Equals(other.E);
        }
        #endregion}
    }
}
