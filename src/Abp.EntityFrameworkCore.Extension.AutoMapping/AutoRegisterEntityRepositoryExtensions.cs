using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Repositories;
using EntityFrameworkCore.Extension;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Abp.EntityFrameworkCore.Extension.AutoMapping
{
    public static class AutoRegisterEntityRepositoryExtensions
    {
        /// <summary>
        /// 将数据表实体类型对应的仓储注入到IOC容器
        /// </summary>
        /// <param name="iocManager"></param>
        public static void RegisterDbEntityRepositories<TDbContext>(this IIocManager iocManager, string modelAssemblyName) where TDbContext : DbContext
        {
            foreach (var entityType in GetDbEntityType(typeof(IEntity<>), modelAssemblyName))
            {
                var keyType = entityType.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEntity<>)).SelectMany(t => t.GetGenericArguments()).First();
                var genericRepositoryType = typeof(IRepository<,>).MakeGenericType(entityType, keyType);
                var impType = typeof(EfCoreRepositoryBase<,,>).MakeGenericType(typeof(TDbContext), entityType, keyType);
                iocManager.RegisterIfNot(genericRepositoryType, impType, lifeStyle: DependencyLifeStyle.Transient);
            }
        }

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
    }
}
