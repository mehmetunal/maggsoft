using AspectCore.DynamicProxy;
using Maggsoft.Npgsql.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Maggsoft.Npgsql.AOP
{
    public class TransactionScopeAttribute : AbstractInterceptorAttribute
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            if (context.ServiceProvider.GetService(typeof(DbContext)) is DbContext dbContext && dbContext.Database.CurrentTransaction == null)
            {
                //var executionStrategy = dbContext.Database.CreateExecutionStrategy();

                //_ = executionStrategy.Execute(async () =>
                //{
                //if (!dbContext.ChangeTracker.Entries().Any(p => p.State == EntityState.Added || p.State == EntityState.Modified || p.State == EntityState.Deleted
                // || p.State == EntityState.Unchanged))
                //    await next(context);

                using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
                {
                    try
                    {
                        await next(context);
                        await dbContext.SaveChangesAsync();
                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        throw ex;
                    }
                }
                //});
            }
            else
            {
                await next(context);
            }
        }
    }
}
