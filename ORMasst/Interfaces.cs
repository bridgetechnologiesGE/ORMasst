using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORMasst
{
    public interface IDataBind<Instance> where Instance : class, new()
    {
        MemberInfo Target { get; }
        string? FieldName { get; }
        object? DefaultValue { get; }
        IConverter? Converter { get; }
    }

    public interface IDataMapper<Instance> where Instance: class, new()
    {
        IEnumerable<IDataBind<Instance>> Bindings { get; }
    }

    public interface IDataSource<Instance> where Instance : class, new()
    {
        IDataField<Instance, Property> MapProperty<Property>(Expression<Func<Instance, Property>> mapProperty);
    }

    public interface IConverter
    {
        object? ConvertTo(string fieldName, object? value, Type? targetType);
    }

    public interface IDataField<Instance, FieldValue> where Instance : class, new()
    {
        IDataField<Instance, FieldValue> DefaultIfNull(FieldValue defaultValue);
        IDataField<Instance, FieldValue> MapConverter(IConverter converter);
        IDataSource<Instance> MapField(string fieldName);
    }
}