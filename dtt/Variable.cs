using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace xtofs.dtt
{
    public interface IVariable : IEquatable<IVariable>
    {
    }

    public static class Variable
    {
        public static IVariable Var(string name) => new StringVariable(name);

        internal static IVariable Dummy = DummyVariable.Instance;

        private static int id;


        /// <summary>
        /// [refresh x] generates a fresh variable name whose preferred form is [x].
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static IVariable Refresh(this IVariable variable)
        {
            switch (variable)
            {
                case StringVariable(var name):
                    return new GenSymVariable(name, System.Threading.Interlocked.Increment(ref id));
                case GenSymVariable(var genname, _):
                    return new GenSymVariable(genname, System.Threading.Interlocked.Increment(ref id));
                case DummyVariable _:
                    return new GenSymVariable("_", System.Threading.Interlocked.Increment(ref id));
            }
            throw new NotImplementedException();
        }
    }

    public class StringVariable : IVariable, IEquatable<StringVariable>
    {
        public string Name { get; }

        public StringVariable(string name)
        {
            if (!name.All(ch => char.IsLetter(ch)))
                throw new ArgumentException("identifier must be alphanumeric");
            Name = name;
        }

        public void Deconstruct(out string name)
        {
            name = Name;
        }
        public override string ToString() => $"{Name}";


        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object other)
        {
            return other is StringVariable s && Equals(s);
        }

        public bool Equals(IVariable other)
        {
            return other is StringVariable s && Equals(s);
        }

        public bool Equals(StringVariable other)
        {
            return other.Name.Equals(Name);
        }
    }

    internal class GenSymVariable : IVariable, IEquatable<GenSymVariable>
    {
        internal GenSymVariable(string name, int id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }
        public int Id { get; }

        public void Deconstruct(out string name, out int id)
        {
            name = Name;
            id = Id;
        }

        public override string ToString() => $"{Name}{Id}";

        #region equality
        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object other)
        {
            return other is GenSymVariable s && Equals(s);
        }

        public bool Equals(IVariable other)
        {
            return other is GenSymVariable s && Equals(s);
        }

        public bool Equals(GenSymVariable other)
        {
            return other.Id == this.Id && other.Name.Equals(Name);
        }
        #endregion
    }

    internal class DummyVariable : IVariable, IEquatable<DummyVariable>
    {
        public static IVariable Instance = new DummyVariable();
        private DummyVariable() { }

        public override string ToString() => "_";

        #region equality
        // two dummy variables are never the same. 

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object other)
        {
            return false;
        }

        public bool Equals(IVariable other)
        {
            return false;
        }

        public bool Equals(DummyVariable other)
        {
            return false;
        }
        #endregion
    }
}
