using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using System.ComponentModel;

namespace ORMasst
{
    internal class DataField<Instance, FieldValue> : IDataField<Instance, FieldValue>, IDataBind<Instance>
        where Instance : class, new()
    {
        public MemberInfo Target { get; }
        public DataMapper<Instance> Context { get; }
        public string? FieldName { get; private set; }
        public FieldValue? DefaultValue { get; private set; }
        public IConverter? Converter { get; private set; }
        object? IDataBind<Instance>.DefaultValue => DefaultValue;

        public DataField(DataMapper<Instance> context, Expression<Func<Instance, FieldValue>> mapProperty)
        {
            Context = context;

            if (mapProperty.Body is MemberExpression me)
                if (me.Member is FieldInfo)
                    Target = me.Member;
                else if (me.Member is PropertyInfo)
                    Target = me.Member;
                else
                    throw new ArgumentException("Expression must refer to field or property!");
            else
                throw new ArgumentException("Expression must be of type MemberExpression!");

            context.mBindings.Add(this);
        }

        public IDataField<Instance, FieldValue> DefaultIfNull(FieldValue defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        public IDataField<Instance, FieldValue> MapConverter(IConverter converter)
        {
            Converter = converter;
            return this;
        }

        public IDataSource<Instance> MapField(string fieldName)
        {
            FieldName = fieldName;
            return this.Context;
        }
    }

    public abstract class DataMapper<Instance> : IDataMapper<Instance>, IDataSource<Instance>
        where Instance : class, new()
    {
        internal List<IDataBind<Instance>> mBindings = new();

        IEnumerable<IDataBind<Instance>> IDataMapper<Instance>.Bindings => mBindings.ToArray();

        protected IDataField<Instance, Property> MapProperty<Property>(Expression<Func<Instance, Property>> mapProperty)
            => (this as IDataSource<Instance>).MapProperty(mapProperty);

        IDataField<Instance, Property> IDataSource<Instance>.MapProperty<Property>(Expression<Func<Instance, Property>> mapProperty)
            => new DataField<Instance, Property>(this, mapProperty);
    }

    public static class GenericMapper
    {
        private sealed class DynMapper<Instance> : DataMapper<Instance>
            where Instance : class, new()
        { }

        private static T MakeAndGet<T>(this T item, Action<T> main)
        {
            main(item);
            return item;
        }

        public static DataMapper<Instance> MakeMapper<Instance>(Action<DataMapper<Instance>> init)
        where Instance : class, new() => new DynMapper<Instance>().MakeAndGet(init);

        public static IDataField<Instance, FieldValue> MapProperty<Instance, FieldValue>(this IDataMapper<Instance> mapper, Expression<Func<Instance, FieldValue>> mapProperty)
        where Instance : class, new()
           => mapper is DynMapper<Instance> inner ? ((IDataSource<Instance>)inner).MapProperty(mapProperty) : throw new ArgumentException("Only for internal use!");
    }
}