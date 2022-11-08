using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EntityFrameworkCore.Extension
{
    public static class AutoMapping
    {
        /// <summary>
        /// 获取数据表实体类型列表
        /// </summary>
        /// <param name="constraintType">实体定义约束类型</param>
        /// <param name="modelAssemblyName">实体类所在dll名称，不包含后缀名(.dll)</param>
        /// <returns></returns>
        private static List<Type> GetDbEntityType(Type constraintType, string modelAssemblyName)
        {
            var all = AppDomain.CurrentDomain.GetAssemblies();
            var types = all.WhereIf(!modelAssemblyName.IsNullOrWhiteSpace(), a => a.FullName.Contains(modelAssemblyName))
                .SelectMany(m => m.GetTypes().Where(t => t.IsClass && !t.IsAbstract && (t.IsImplement(constraintType) || t.IsSubclass(constraintType))).ToList())
                .Distinct()
                .ToList();
            return types.Where(t => !t.GetCustomAttributes<NotMappedAttribute>().Any()).ToList();
        }

        /// <summary>
        /// 自动添加实体到DbSet
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="constraintType">实体定义约束类型</param>
        /// <param name="modelAssemblyName">实体类所在dll名称，不包含后缀名(.dll)</param>
        public static void AutoMappingEntityTypes(this ModelBuilder modelBuilder, Type constraintType, string modelAssemblyName)
        {
            var types = GetDbEntityType(constraintType, modelAssemblyName);
            if (types != null && types.Any())
            {
                types.ForEach(t =>
                {
                    if (!t.GetCustomAttributes<NotMappedAttribute>().Any())
                    {
                        var entityType = modelBuilder.Model.FindEntityType(t);
                        if (entityType == null)
                        {
                            modelBuilder.Model.AddEntityType(t);
                        }
                    }
                });
            }
        }
        /// <summary>
        /// 自动添加实体到DbSet
        /// </summary>
        /// <typeparam name="TConstraint">实体定义约束类</typeparam>
        /// <param name="modelBuilder"></param>
        /// <param name="modelAssemblyName">实体类所在dll名称，不包含后缀名(.dll)</param>
        public static void AutoMappingEntityTypes<TConstraint>(this ModelBuilder modelBuilder, string modelAssemblyName)
        {
            var types = GetDbEntityType(typeof(TConstraint), modelAssemblyName);
            if (types != null && types.Any())
            {
                types.ForEach(t =>
                {
                    if (!t.GetCustomAttributes<NotMappedAttribute>().Any())
                    {
                        var entityType = modelBuilder.Model.FindEntityType(t);
                        if (entityType == null)
                        {
                            modelBuilder.Model.AddEntityType(t);
                        }
                    }
                });
            }
        }

        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrEmpty(source) || string.IsNullOrWhiteSpace(source);
        }
        /// <summary>
        /// 判断是否实现指定接口
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool IsImplement(this Type entityType, Type interfaceType)
        {
            return interfaceType.IsInterface && entityType.GetTypeInfo().GetInterfaces().Any(t =>
                t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == interfaceType);
        }
        /// <summary>
        /// 判断是否实现指定接口
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool IsImplement<TInterface>(this Type entityType)
        {
            return entityType.IsImplement(typeof(TInterface));
        }
        /// <summary>
        /// 判断是否继承指定类
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static bool IsSubclass(this Type entityType, Type parentType)
        {
            return parentType.IsClass && entityType.IsSubclassOf(parentType);
        }
        /// <summary>
        /// 判断是否继承指定类
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool IsSubclass<TParent>(this Type entityType) where TParent : class
        {
            return entityType.IsSubclass(typeof(TParent));
        }

        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool precondition, Func<T, bool> predicate)
        {
            if(precondition)
                return source.Where(predicate);
            return source;
        }
    }
}