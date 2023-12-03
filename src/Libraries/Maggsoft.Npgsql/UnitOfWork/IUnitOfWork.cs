using System;
using Maggsoft.Data.Npgsql;
using Maggsoft.Core.Entities;
using Maggsoft.Npgsql.Repository;
using System.Threading.Tasks;
using System.Threading;

namespace Maggsoft.Npgsql.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    INpgsqlRepository<T> GetRepository<T>() where T : BaseEntity, IEntity;

    bool BeginNewTransaction();
    Task<bool> BeginNewTransactionAsync();
    bool RollBackTransaction();
    Task<bool> RollBackTransactionAsync();

    int SaveChanges();
    Task<int> SaveChangesAsync();

    void Commit();
    Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));
}
