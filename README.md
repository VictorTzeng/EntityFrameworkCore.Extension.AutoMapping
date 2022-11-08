# EntityFrameworkCore.Extension.AutoMapping
## 一款EntityFrameworkCore自动映射数据实体类到数据库上下文的扩看类，并且支持abp和abp vnext的自动注入实体对应的仓储类(Repository)

## 如何使用（How to use）
### 在DbContext.cs中重写OnModelCreating方法：
```
using EntityFrameworkCore.Extension;
... //此处省略其它代码
public class XmateDbContext:DbContext
{
  ... //此处省略其它代码
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    var modelAssemblyName = "XMate.Models";//实体类所在类库的名称，不包含扩展名(.dll)
    modelBuilder.AutoMappingEntityTypes(modelAssemblyName);
    base.OnModelCreating(modelBuilder);//这个必须加，否则报错
    
    ...//此处省略其它代码
  }
}
```

### 在Abp中实现自动注入实体类对应的Repository
```
using Abp.EntityFrameworkCore.Extension.AutoMapping;
... //此处省略其它代码
public class XmateModule:AbpModule
{
  ... //此处省略其它代码
  //重写Initialize方法
  public override void Initialize()
  {
      ... //此处省略其它代码
      var modelAssemblyName = "XMate.Models";//实体类所在类库的名称，不包含扩展名(.dll)
      IocManager.RegisterDbEntityRepositories(modelAssemblyName);
  }
}

```

### 在Abp VNext中实现自动注入实体类对应的Repository
```
using AbpVNext.EntityFrameworkCore.Extension.AutoMapping;
... //此处省略其它代码
public class XmateModule:AbpModule
{
  ... //此处省略其它代码
  //重写ConfigureServices方法
  public override void ConfigureServices(ServiceConfigurationContext context)
  {
      ... //此处省略其它代码
      var modelAssemblyName = "XMate.Models";//实体类所在类库的名称，不包含扩展名(.dll)
      context.Services.RegisterDbEntityRepositories(modelAssemblyName);
  }
}

```
