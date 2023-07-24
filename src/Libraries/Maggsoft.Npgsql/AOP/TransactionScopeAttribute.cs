using AspectCore.DynamicProxy;
using Maggsoft.Aspect.Core.Aspects;
using Maggsoft.Npgsql.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Maggsoft.Npgsql.AOP
{
    /*

     [AttributeUsage(AttributeTargets.Method)]
 public class UnitOfWorkAttribute : AspectAttribute
 {
     private IUnitOfWork _unitOfWork;
     private IDatabaseTransaction _transaction;

     public override AspectAttribute LoadDependencies(IServiceProvider serviceProvider)
     {
         _unitOfWork ??= serviceProvider.GetRequiredService<IUnitOfWork>();
         return base.LoadDependencies(serviceProvider);
     }

     public override void OnBefore(MethodExecutionArgs args) => _transaction = _unitOfWork.BeginTransaction();

     public override async Task OnBeforeAsync(MethodExecutionArgs args) => _transaction = await _unitOfWork.BeginTransactionAsync();

     public override void OnSuccess(MethodExecutionArgs args) => _transaction.Commit();

     public override async Task OnSuccessAsync(MethodExecutionArgs args) => await _transaction.CommitAsync();

     public override void OnException(MethodExecutionArgs args) => _transaction.Rollback();

     public override async Task OnExceptionAsync(MethodExecutionArgs args) => await _transaction.RollbackAsync();
 }
     */
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
