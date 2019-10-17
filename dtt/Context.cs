using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace xtofs.dtt
{
    public static class Context
    {
        public static IImmutableDictionary<IVariable, (Expression type, Expression value)> Empty =
                  ImmutableDictionary<IVariable, (Expression type, Expression value)>.Empty;

        public static bool TryGetType(
            this IImmutableDictionary<IVariable, (Expression type, Expression value)> context,
            IVariable variable,
            out Expression type)
        {
            if (context.TryGetValue(variable, out var pair))
            {
                type = pair.type;
                return true;
            }
            else
            {
                type = null;
                return false;
            }
        }

        public static bool TryGetVal(
            this IImmutableDictionary<IVariable, (Expression type, Expression value)> context,
            IVariable variable,
            out Expression value)
        {
            if (context.TryGetValue(variable, out var pair))
            {
                value = pair.value;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public static IImmutableDictionary<IVariable, (Expression type, Expression value)> Extend(
           this IImmutableDictionary<IVariable, (Expression type, Expression value)> context,
           IVariable variable,
           Expression type,
           Expression value = null)
        {
            return context.SetItem(variable, (type, value));
        }

        public static IImmutableDictionary<IVariable, (Expression type, Expression value)> Define(
           this IImmutableDictionary<IVariable, (Expression type, Expression value)> context,
           IVariable variable,
           Expression expression)
        {
            var type = context.InferType(expression);
            return context.SetItem(variable, (type, expression));
        }

        /// <summary>
        /// [infer_type ctx e] infers the type of expression [e] in context [ctx]. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <returns>a type expression</returns>
        public static Expression InferType(
               this IImmutableDictionary<IVariable, (Expression type, Expression value)> context,
               Expression expression)
        {
            switch (expression)
            {
                case VariableExpression(var v):
                    {
                        if (context.TryGetType(v, out var ty))
                        {
                            return ty;
                        }
                        throw new ArgumentException($"unknown identifier {v} in {expression}", nameof(expression));
                    }
                case UniverseExpression(var gen):
                    {
                        return Expression.Universe(gen + 1);
                    }
                case PiExpression(var x, var t1, var t2):
                    {
                        var k1 = context.InferUniverse(t1);
                        var k3 = context.Extend(x, t1).InferUniverse(t2);
                        return Expression.Universe(Math.Max(k1, k3));
                    }
                case LambdaExpression(var x, var t, var e):
                    {
                        // var _ = context.InferUniverse(t);
                        var te = context.Extend(x, t).InferType(e);
                        return Expression.Pi(x, t, te);
                    }
                case AppExpression(var e1, var e2):
                    {
                        var (x, s, t) = context.InferPi(e1);
                        var te = context.InferType(e2);
                        if (!context.Equal(s, te))
                        {
                            throw new ArgumentException($"expressions {s} and {te} are not equal", nameof(expression));
                        }
                        var r = Substitution.Single(x, e2).Apply(t);
                        return r;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public static int InferUniverse(
              this IImmutableDictionary<IVariable, (Expression type, Expression value)> context,
              Expression expression)
        {
            var u = context.InferType(expression);
            switch (context.Normalize(u))
            {
                case UniverseExpression(var k):
                    return k;
                default:
                    throw new ArgumentException($"Type expected instead of {expression}", nameof(expression));
            }
        }

        public static PiExpression InferPi(
           this IImmutableDictionary<IVariable, (Expression type, Expression value)> context,
           Expression expression)
        {
            var u = context.InferType(expression);
            switch (context.Normalize(u))
            {
                case PiExpression p:
                    return p;
                default:
                    throw new ArgumentException(nameof(expression), "Function expected");
            }
        }

        public static bool Equal(
           this IImmutableDictionary<IVariable, (Expression type, Expression value)> context,
           Expression a, Expression b)
        {
            bool Equal(Expression a, Expression b)
            {
                switch ((a, b))
                {
                    case (VariableExpression(var x1), VariableExpression(var x2)):
                        return x1.Equals(x2);

                    case (UniverseExpression(var k1), UniverseExpression(var k2)):
                        return k1.Equals(k2);

                    case (LambdaExpression(var x, var t1, var e1), LambdaExpression(var y, var t2, var e2)):
                        return context.Equal(t1, t2) && context.Equal(e1, Substitution.Single(y, Expression.Var(x)).Apply(e2));

                    case (PiExpression(var x, var t1, var e1), PiExpression(var y, var t2, var e2)):
                        return context.Equal(t1, t2) && context.Equal(e1, Substitution.Single(y, Expression.Var(x)).Apply(e2));

                    case (AppExpression(var a1, var a2), AppExpression(var b1, var b2)):
                        return context.Equal(a1, b1) && context.Equal(a2, b2);
                    default:
                        return false;
                }
            }

            return Equal(context.Normalize(a), context.Normalize(b));
        }

        /// <summary>
        /// [normalize ctx e] normalizes the given expression [e] in context [ctx]. It removes
        /// all redexes and it unfolds all definitions. It performs normalization under binders.
        /// </summary>
        /// <param name="IImmutableDictionary<VariableExpression, (IExpression type, IExpression value)"></param>
        /// <returns></returns>
        public static Expression Normalize(
            this IImmutableDictionary<IVariable, (Expression type, Expression value)> context,
            Expression expression)
        {
            switch (expression)
            {
                case VariableExpression(var v):
                    {
                        if (context.TryGetVal(v, out var val))
                        {
                            return val == null ? Expression.Var(v) : context.Normalize(val);
                        }
                        else
                        {
                            throw new ArgumentException(nameof(expression), $"unknown identifier {v}");
                        }
                    }
                case UniverseExpression u:
                    {
                        return u;
                    }
                case PiExpression(var x, var t, var e):
                    {
                        t = context.Normalize(t);
                        return Expression.Pi(x, t, context.Extend(x, t).Normalize(e));
                    }
                case LambdaExpression(var x, var t, var e):
                    {
                        t = context.Normalize(t);
                        return Expression.Lambda(x, t, context.Extend(x, t).Normalize(e));
                    }
                case AppExpression(var e1, var e2):
                    {
                        e2 = context.Normalize(e2);
                        if (context.Normalize(e1) is LambdaExpression(var x, var _, var e1p))
                        {
                            return context.Normalize(Substitution.Single(x, e2).Apply(e1p));
                        }
                        else
                        {
                            return Expression.App(e1, e2);
                        }
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public static string Format(this IImmutableDictionary<IVariable, (Expression type, Expression value)> context, string sep = "; ")
        {
            return string.Join(sep, from triple in context select Format(triple));
        }

        private static string Format(KeyValuePair<IVariable, (Expression type, Expression value)> triple)
        {
            var (key, (type, value)) = triple;
            return $"{key}: {type}{(value == null ? "" : $" = {value}")}";
        }

        public static Expression Eval(this IImmutableDictionary<IVariable, (Expression type, Expression value)> context, Expression expr)
        {
            return context.Normalize(expr);
        }
    }
}
