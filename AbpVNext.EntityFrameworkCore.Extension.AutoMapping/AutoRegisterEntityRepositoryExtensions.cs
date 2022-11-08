using EntityFrameworkCore.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AbpVNext.EntityFrameworkCore.Extension.AutoMapping
{
    public static class AutoRegisterEntityRepositoryExtensions
    {
        /// <summary>
        /// 将数据表实体类型对应的仓储注入到IOC容器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="modelAssemblyName">实体类所在dll名称，不包含后缀名(.dll)</param>
        public static void RegisterDbEntityRepositories<TDbContext>(this IServiceCollection services, string modelAssemblyName) where TDbContext : DbContext, IEfCoreDbContext
        {
            foreach (var entityType in GetDbEntityType(modelAssemblyName))
            {
                var keyType = entityType.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEntity<>)).SelectMany(t => t.GetGenericArguments()).First();
                var genericRepositoryType = typeof(IRepository<,>).MakeGenericType(entityType, keyType);
                var impType = typeof(EfCoreRepository<,,>).MakeGenericType(typeof(TDbContext), entityType, keyType);
                services.AddTransient(genericRepositoryType, impType);
            }
        }

        /// <summary>
        /// 获取数据表实体类型列表
        /// </summary>
        /// <param name="modelAssemblyName">实体类所在dll名称，不包含后缀名(.dll)</param>
        /// <returns></returns>
        private static List<Type> GetDbEntityType(string modelAssemblyName)
        {
            var all = AppDomain.CurrentDomain.GetAssemblies();
            var types = AbpEnumerableExtensions.WhereIf(all, !AbpStringExtensions.IsNullOrWhiteSpace(modelAssemblyName), a => a.FullName.Contains(modelAssemblyName))
                .SelectMany(m => m.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsImplement(typeof(IEntity<>))).ToList())
                .Distinct()
                .ToList();
            return types.Where(t => !t.GetCustomAttributes<NotMappedAttribute>().Any()).ToList();
        }
    }
}
