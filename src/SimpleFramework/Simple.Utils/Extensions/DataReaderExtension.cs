﻿using Simple.Utils.Exceptions;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace Simple.Utils.Extensions
{
    /// <summary>DataReader扩展类</summary>
    public static class DataReaderExtension
    {
        public static List<TResult> ToList<TResult>(this IDataReader dr) where TResult : class, new()
        {
            try
            {
                IDataReaderEntityBuilder<TResult> eblist = IDataReaderEntityBuilder<TResult>.CreateBuilder(dr);
                List<TResult> list = new List<TResult>();
                if (dr == null) return list;
                while (dr.Read()) list.Add(eblist.Build(dr));
                return list;
            }
            catch (Exception ex)
            {
                throw new FatalException($"dr 转换 {typeof(TResult)} 异常", ex);
            }
            finally
            {
                dr.Close(); dr.Dispose(); //释放资源
            }
        }

        public static TResult ToEntity<TResult>(this IDataReader dr) where TResult : class, new()
        {
            if (dr == null || !dr.Read()) return null;

            try
            {
                IDataReaderEntityBuilder<TResult> eb = IDataReaderEntityBuilder<TResult>.CreateBuilder(dr);
                TResult entity = eb.Build(dr);
                return entity;
            }
            catch (Exception ex)
            {
                throw new FatalException($"dr 转换 {typeof(TResult)} 异常", ex);
            }
            finally
            {
                dr.Close(); dr.Dispose();
            }
        }

        public class IDataReaderEntityBuilder<Entity>
        {
            private static readonly MethodInfo getValueMethod =
            typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });

            private static readonly MethodInfo isDBNullMethod =
                typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });

            private Load handler;

            private IDataReaderEntityBuilder()
            { }

            private delegate Entity Load(IDataRecord dataRecord);

            public static IDataReaderEntityBuilder<Entity> CreateBuilder(IDataRecord dataRecord)
            {
                IDataReaderEntityBuilder<Entity> dynamicBuilder = new IDataReaderEntityBuilder<Entity>();
                DynamicMethod method = new DynamicMethod("DynamicCreateEntity", typeof(Entity),
                        new Type[] { typeof(IDataRecord) }, typeof(Entity), true);
                ILGenerator generator = method.GetILGenerator();
                LocalBuilder result = generator.DeclareLocal(typeof(Entity));
                generator.Emit(OpCodes.Newobj, typeof(Entity).GetConstructor(Type.EmptyTypes));
                generator.Emit(OpCodes.Stloc, result);
                for (int i = 0; i < dataRecord.FieldCount; i++)
                {
                    PropertyInfo propertyInfo = typeof(Entity).GetProperty(dataRecord.GetName(i));
                    Label endIfLabel = generator.DefineLabel();
                    if (propertyInfo != null && propertyInfo.GetSetMethod() != null)
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldc_I4, i);
                        generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                        generator.Emit(OpCodes.Brtrue, endIfLabel);
                        generator.Emit(OpCodes.Ldloc, result);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldc_I4, i);
                        generator.Emit(OpCodes.Callvirt, getValueMethod);

                        Type propertyType = propertyInfo.PropertyType;
                        Type nullableType = Nullable.GetUnderlyingType(propertyType);
                        if (nullableType != null)
                        {
                            generator.Emit(OpCodes.Unbox_Any, nullableType);
                            generator.Emit(OpCodes.Newobj, propertyType.GetConstructor(new[] { nullableType }));
                        }
                        else
                        {
                            generator.Emit(OpCodes.Unbox_Any, dataRecord.GetFieldType(i));
                        }

                        generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                        generator.MarkLabel(endIfLabel);
                    }
                }
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ret);
                dynamicBuilder.handler = (Load)method.CreateDelegate(typeof(Load));
                return dynamicBuilder;
            }

            public Entity Build(IDataRecord dataRecord)
            {
                return handler(dataRecord);
            }
        }
    }
}