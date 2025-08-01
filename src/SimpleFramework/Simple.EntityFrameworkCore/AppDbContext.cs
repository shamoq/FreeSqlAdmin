using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Simple.Utils.Helper;

namespace Simple.EntityFrameworkCore
{
    public abstract class AppDbContext : DbContext
    {
        public bool UnitOfWork { protected get; set; }

        protected readonly string connectString;

        public AppDbContext(string connectString)
        {
            this.connectString = connectString;
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (ConfigHelper.GetValue<bool>("ShowEfCoreCommand"))
                optionsBuilder.UseLoggerFactory(new EFLoggerFactory());

            base.OnConfiguring(optionsBuilder);
        }

        public async Task RunWithTran(Func<IDbContextTransaction, Task<bool>> func)
        {
            var trans = Database.BeginTransaction();
            try
            {
                var result = await func(trans);
                if (result)
                {
                    await this.SaveChangesAsync(); // 假设 this 是 DbContext 实例
                    await trans.CommitAsync(); // 提交事务
                }
                else
                {
                    await trans.RollbackAsync(); // 回滚事务
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                LogHelper.Error("事务执行出现异常", ex);
            }
        }

        public async Task RunWithTran(Func<IDbContextTransaction, Task> func)
        {
            await RunWithTran(async (tran) =>
           {
               await func(tran);
               return true;
           });
        }
    }
}