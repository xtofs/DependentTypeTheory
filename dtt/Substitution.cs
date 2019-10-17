using System;
using System.Collections.Immutable;

namespace xtofs.dtt
{
    public static class Substitution
    {

        public static ImmutableDictionary<IVariable, Expression> Empty =
            ImmutableDictionary<IVariable, Expression>.Empty;

        public static ImmutableDictionary<IVariable, Expression> Single(IVariable variable, Expression value) =>
            Empty.Add(variable, value);

        public static IImmutableDictionary<IVariable, Expression> SetOrAdd(this IImmutableDictionary<IVariable, Expression> subst,
        IVariable variable, Expression expression)
        {
            return subst.SetItem(variable, expression);
        }


        /// <summary>
        /// [subst [(x1,e1); ...; (xn;en)] e] performs the given substitution of
        ///     expressions [e1], ..., [en] for variables [x1], ..., [xn] in expression [e].
        /// let rec subst s = function
        ///   | Var x -> (try List.assoc x s with Not_found -> Var x)
        ///   | Universe k -> Universe k
        ///   | Pi a -> Pi (subst_abstraction s a)
        ///   | Lambda a -> Lambda (subst_abstraction s a)
        ///   | App (e1, e2) -> App (subst s e1, subst s e2)
        /// and subst_abstraction s (x, t, e) =
        ///   let x' = refresh x in
        ///     (x', subst s t, subst ((x, Var x') :: s) e)
        /// </summary>
        public static Expression Apply(this IImmutableDictionary<IVariable, Expression> subst, Expression expression)
        {
            switch (expression)
            {
                case VariableExpression v:
                    {
                        return subst.TryGetValue(v.Variable, out var e) ? e : v;
                    }
                case UniverseExpression u:
                    {
                        return u;
                    }
                case PiExpression(var x, var t, var e):
                    {
                        var x1 = x.Refresh();
                        return Expression.Pi(x1, subst.Apply(t), subst.SetOrAdd(x, Expression.Var(x1)).Apply(e));
                    }
                case LambdaExpression(var x, var t, var e):
                    {
                        var x1 = x.Refresh();
                        return Expression.Lambda(x1, subst.Apply(t), subst.SetOrAdd(x, Expression.Var(x1)).Apply(e));
                    }
                case AppExpression(var f, var e):
                    {
                        return Expression.App(subst.Apply(f), subst.Apply(e));
                    }
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
