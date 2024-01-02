using Maggsoft.Core.Entities;
using Maggsoft.Data.Mssql;
using Maggsoft.Mssql.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Maggsoft.Mssql.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IMssqlRepository<T> GetRepository<T>() where T : BaseEntity, IEntity;

        bool BeginNewTransaction();
        Task<bool> BeginNewTransactionAsync();
        bool RollBackTransaction();
        Task<bool> RollBackTransactionAsync();

        int SaveChanges();
        Task<int> SaveChangesAsync();


        void Commit();
        Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}