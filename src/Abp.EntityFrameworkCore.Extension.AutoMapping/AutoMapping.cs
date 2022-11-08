using Abp.Domain.Entities;
using EntityFrameworkCore.Extension;
using Microsoft.EntityFrameworkCore;

namespace Abp.EntityFrameworkCore.Extension.AutoMapping
{
    public static class AutoMapping
    {
        /// <summary>
        /// 自动添加实体到DbSet
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="constraintType">实体定义约束类型</param>
        /// <param name="modelAssemblyName">实体类所在dll名称，不包含后缀名(.dll)</param>
        public static void AutoMappingEntityTypes(this ModelBuilder modelBuilder, string modelAssemblyName)
        {
            modelBuilder.AutoMappingEntityTypes<IEntity>(modelAssemblyName);
        }
    }
}