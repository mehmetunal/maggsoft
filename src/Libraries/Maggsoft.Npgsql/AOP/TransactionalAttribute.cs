using AspectCore.DynamicProxy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Maggsoft.Npgsql.AOP
{
    /// 
    ///Provide transactional consistency for units of work
    /// 
    #region OLD
    //public class TransactionalAttribute : AbstractInterceptorAttribute
    //{
    //    IUnitOfWork _unitOfWork { get; set; }

    //    public async override Task Invoke(AspectContext context, AspectDelegate next)
    //    {
    //        try
    //        {
    //            _unitOfWork = context.ServiceProvider.GetService(typeof(IUnitOfWork)) as IUnitOfWork;
    //            _unitOfWork.BeginNewTransaction();
    //            await next(context);
    //            _unitOfWork.SaveChanges();
    //        }
    //        catch (Exception)
    //        {
    //            _unitOfWork.RollBackTransaction();
    //            throw;
    //        }
    //    }
    //} 
    #endregion
    public class TransactionalAttribute : AbstractInterceptorAttribute
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var dbContext = context.ServiceProvider.GetService(typeof(DbContext)) as DbContext;
            if (dbContext != null && dbContext.Database.CurrentTransaction == null)
            {
                await dbContext.Database.BeginTransactionAsync();
                try
                {
                    await next(context);
                    await dbContext.SaveChangesAsync();
                    await dbContext.Database.CommitTransactionAsync();
                }
                catch
                {
                    await dbContext.Database.RollbackTransactionAsync();
                    throw;
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
