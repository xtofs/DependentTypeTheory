using System;
using System.Collections.Immutable;
using Xunit;

namespace xtofs.dtt.tests
{
    public class UnitTest1
    {

        private IImmutableDictionary<IVariable, (Expression type, Expression value)> ctx = Context.Empty;

        public UnitTest1()
        {
            ctx = ctx.Extend(Variable.Var("N"), Expression.Universe(0));
            ctx = ctx.Extend(Variable.Var("z"), Expression.Var("N"));
            ctx = ctx.Extend(Variable.Var("s"), Expression.Pi(Expression.Var("N"), Expression.Var("N")));

            var N = Expression.Var("N");
            var N2N = Expression.Pi(N, N);

            ctx = ctx.Define(Variable.Var("triple"),
                Expression.Lambda(Variable.Var("f"), N2N,
                    Expression.Lambda(Variable.Var("x"), N,
                        Expression.App(Expression.Var("f"),
                            Expression.App(Expression.Var("f"),
                                Expression.App(Expression.Var("f"),
                                    Expression.Var("x")))))));
        }

        [Fact]
        public void Test1()
        {
            var N = Expression.Var("N");

            var e = Expression.App(Expression.Var("triple"),
                        Expression.App(Expression.Var("triple"),
                            Expression.Var("s")));

            var actual = ctx.InferType(e);

            var expected = Expression.Pi(N, N);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test2()
        {
            var s = Expression.Var("s");
            var z = Expression.Var("z");
            var e = Expression.App(Expression.Var("triple"),
                        Expression.App(Expression.Var("triple"), s));

            var actual = ctx.Eval(Expression.App(e, z));

            var expected =
                Expression.App(s, Expression.App(s, Expression.App(s,
                    Expression.App(s, Expression.App(s, Expression.App(s,
                        Expression.App(s, Expression.App(s, Expression.App(s,
                            z)))))))));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test3()
        {
            var a = Expression.Lambda(Variable.Var("x"), Expression.Var("N"), Expression.Var("x"));
            var b = Expression.Lambda(Variable.Var("y"), Expression.Var("N"), Expression.Var("x"));

            Assert.NotEqual(a, b);
        }
    }
}
