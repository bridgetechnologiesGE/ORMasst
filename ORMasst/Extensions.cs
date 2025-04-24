using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ORMasst
{
    public static class DbExtensions
    {
        private static object? ToType(this object src, Type? targetType)
        {
            var outType = Nullable.GetUnderlyingType(targetType!) ?? targetType!;

            if (src == null)
                return targetType!.IsValueType ? Activator.CreateInstance(targetType) : null;

            if (outType.IsEnum)
                return Enum.TryParse(outType, src.ToString(), true, out var value) ? value :
                    throw new ArgumentException($"Cannot convert {src} in {nameof(targetType)}!");

            else
                return Convert.ChangeType(src, outType);
        }

        private static (int ordinal, Action<object?, object?> setter) ToDbMap<Instance>(this IDataBind<Instance> bind)
            where Instance : class, new()
        {
            if (bind.Target is FieldInfo fi)
                return (
                    ordinal: -1,
                    setter: (Action<object?, object?>)((object? obj, object? value) => fi.SetValue(obj,
                    bind.Converter == null ? value!.ToType(fi.FieldType) : bind.Converter.
                    ConvertTo(bind.FieldName!, value, fi.FieldType)))
                );
            else if (bind.Target is PropertyInfo pi)
                return (
                    ordinal: -1,
                    setter: (Action<object?, object?>)((object? obj, object? value) => pi.SetValue(obj,
                    bind.Converter == null ? value!.ToType(pi.PropertyType) : bind.Converter.
                    ConvertTo(bind.FieldName!, value, pi.PropertyType)))
                );
            else
                throw new ArgumentException($"Target {bind.Target.Name} is not valid (must be Property or Field!)");
        }

        public static async IAsyncEnumerable<Instance> LoadEntitiesAsync<Mapper, Instance>(
            this DbConnection conn, string query, params DbParameter[] args)
            where Mapper : IDataMapper<Instance>, new()
            where Instance : class, new()
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.Parameters.AddRange(args);

                await foreach (var item in cmd.LoadEntitiesAsync<Mapper, Instance>())
                    yield return item;
            }
        }

        public static async IAsyncEnumerable<Instance> LoadEntitiesAsync<Mapper, Instance>(this DbCommand command)
            where Mapper : IDataMapper<Instance>, new()
            where Instance : class, new()
        {
            var mapper = default(Mapper);

            if (Activator.CreateInstance(typeof(Mapper)) is Mapper mp)
                mapper = mp;

            else
                throw new ArgumentException($"Error Invoking parameterless constructor for mapper {nameof(Mapper)}!");

            await foreach (var item in command.LoadEntitiesAsync(mapper))
                yield return item;
        }

        public static async IAsyncEnumerable<Instance> LoadEntitiesAsync<Instance>(this DbCommand command, IDataMapper<Instance> mapper)
            where Instance : class, new()
        {
            var binds = mapper.Bindings.ToDictionary(b => b, b => b.ToDbMap());

            using (var rdr = await command.ExecuteReaderAsync())
            {
                var fMap = Enumerable.Range(0, rdr.VisibleFieldCount).ToDictionary(
                    id => rdr.GetName(id), StringComparer.InvariantCultureIgnoreCase);

                if (binds.Keys.Any(k => !fMap.ContainsKey(k.FieldName!)))
                    throw new AggregateException("Key mapping fails", binds.Keys.
                        Where(k => !fMap.ContainsKey(k.FieldName!)).Select(fn =>
                        new ArgumentException($"Field {fn.FieldName} missing!")));

                foreach (var k in binds.Keys)
                    binds[k] = (fMap[k.FieldName!], binds[k].setter);

                while (await rdr.ReadAsync())
                {
                    var item = new Instance();

                    foreach (var p in binds)
                    {
                        var value = rdr.GetValue(p.Value.ordinal);

                        if (value == DBNull.Value)
                            value = null;

                        value = value ?? p.Key.DefaultValue;

                        p.Value.setter(item, value);
                    }

                    yield return item;
                }
            }
        }

        public static IEnumerable<Instance> LoadEntities<Mapper, Instance>(
            this DbConnection conn, string query, params DbParameter[] args)
            where Mapper : IDataMapper<Instance>, new()
            where Instance : class, new()
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.Parameters.AddRange(args);

                foreach (var item in cmd.LoadEntities<Mapper, Instance>())
                    yield return item;
            }
        }

        public static IEnumerable<Instance> LoadEntities<Mapper, Instance>(this DbCommand command)
            where Mapper : IDataMapper<Instance>, new()
            where Instance : class, new()
        {
            var mapper = default(Mapper);

            if (Activator.CreateInstance(typeof(Mapper)) is Mapper mp)
                mapper = mp;

            else
                throw new ArgumentException($"Error Invoking parameterless constructor for mapper {nameof(Mapper)}!");

            foreach (var item in command.LoadEntities(mapper))
                yield return item;
        }

        public static IEnumerable<Instance> LoadEntities<Instance>(this DbCommand command, IDataMapper<Instance> mapper)
            where Instance : class, new()
        {
            var binds = mapper.Bindings.ToDictionary(b => b, b => b.ToDbMap());

            using (var rdr = command.ExecuteReader())
            {
                var fMap = Enumerable.Range(0, rdr.VisibleFieldCount).ToDictionary(
                    id => rdr.GetName(id), StringComparer.InvariantCultureIgnoreCase);

                if (binds.Keys.Any(k => !fMap.ContainsKey(k.FieldName!)))
                    throw new AggregateException("Key mapping fails", binds.Keys.
                        Where(k => !fMap.ContainsKey(k.FieldName!)).Select(fn =>
                        new ArgumentException($"Field {fn.FieldName} missing!")));

                foreach (var k in binds.Keys)
                    binds[k] = (fMap[k.FieldName!], binds[k].setter);

                while (rdr.Read())
                {
                    var item = new Instance();

                    foreach (var p in binds)
                    {
                        var value = rdr.GetValue(p.Value.ordinal);

                        if (value == DBNull.Value)
                            value = null;

                        value = value ?? p.Key.DefaultValue;

                        p.Value.setter(item, value);
                    }

                    yield return item;
                }
            }
        }

        public static async Task<IEnumerable<T>> ToArrayAsync<T>(this IAsyncEnumerable<T> data)
        {
            var res = new List<T>();

            if (data != null) 
            {
                await foreach (var item in data)
                    res.Add(item);
            }

            return res;
        }
    }
}