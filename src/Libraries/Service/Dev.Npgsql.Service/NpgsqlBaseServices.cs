using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dev.Core.Entities;
using Dev.Core.Extensions;
using Dev.Core.Infrastructure;
using Dev.Core.Model;
using Dev.Core.Model.Pagination;
using Dev.Data.Npgsql;
using Dev.Framework.Exceptions;
using Dev.Framework.Mapper;
using Dev.Npgsql.Repository;
using Dev.Services.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dev.Npgsql.Service
{
    public partial class NpgsqlBaseServices<TTable, TResultDto, TEditDto, TAddDto> where TTable : BaseEntity,
        new()
        where TResultDto : BaseDevModel,
        new()
        where TEditDto : BaseDevModel,
        new()
        where TAddDto : BaseDevModel
    {
        #region Properties

        protected readonly IMapper Mapper;
        private readonly IEventPublisher EventPublisher;
        public readonly INpgsqlRepository<TTable> Repository;

        #endregion

        #region Ctor

        protected NpgsqlBaseServices()
        {
            Mapper = EngineContext.Current.Resolve<IMapper>()
                     ?? throw new ArgumentNullException($"{nameof(IMapper)} is null");
            EventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
            Repository = EngineContext.Current.Resolve<INpgsqlRepository<TTable>>();
        }

        #endregion

        #region Method

        public virtual async Task<int> CountAsync()
        {
            return await Repository.Table.CountAsync();
        }

        public virtual async Task<PagedList<TResultDto>> GetAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, Expression<Func<TTable, object>> @order = null)
        {
            var query = Repository.Table;

            if (!showHidden)
                query = query.Where(p => p.IsPublish);

            query = query.Where(p => !p.IsDeleted);

            if (@order == null)
                query = query.OrderBy(@order);
            else
                query = query.OrderBy(v => v.DisplayOrder);

            var result = query.ProjectTo<TResultDto>(AutoMapperConfiguration.MapperConfiguration).ToPagedListAsync(pageIndex, pageSize);

            return await result;
        }

        public virtual async Task<PagedList<TResultDto>> GetAsync(PaginationFilter paginationFilter, bool showHidden = false)
        {
            var query = Repository.Table;

            if (!showHidden)
                query = query.Where(p => p.IsPublish);

            query = query.Where(p => !p.IsDeleted);

            query = query.OrderBy(v => v.DisplayOrder);

            var result = query.ProjectTo<TResultDto>(AutoMapperConfiguration.MapperConfiguration).ToPagedListAsync(paginationFilter);

            return await result;
        }

        public virtual async Task<TResultDto> GetByIdAsync(Guid id)
        {
            var result = await Repository.FindByIdAsync(id);
            return Mapper.Map<TResultDto>(result);
        }
            
        public virtual async Task<TResultDto> AddAsync(TAddDto tAddDto, bool publishEvent = true)
        {
            if (tAddDto == null)
                throw new NotFoundException($"{nameof(tAddDto)}");

            var domainEntity = Mapper.Map<TTable>(tAddDto);

            domainEntity.CreatedDate = DateTime.UtcNow;
            domainEntity.CreatorIP = RemoteIp;

            await Repository.AddAsync(domainEntity);

            //event notification
            if (publishEvent)
                await EventPublisher.EntityInsertedAsync(domainEntity);

            return Mapper.Map<TResultDto>(domainEntity);
        }

        public virtual async Task<TResultDto> UpdateAsync(TEditDto tEditDto, bool publishEvent = true)
        {
            if (tEditDto == null)
                throw new NotFoundException($"{nameof(tEditDto)}");

            var domainEntity = Mapper.Map<TTable>(tEditDto);

            var dbData = await Repository.FindByIdAsync(domainEntity.Id);
            if (dbData == null)
                throw new NotFoundException($"{domainEntity.Id} is null data");

            var mapperData = Mapper.Map(domainEntity, dbData);

            mapperData.ModifiedDate = DateTime.UtcNow;
            mapperData.ModifierIP = RemoteIp;

            if (string.IsNullOrEmpty(mapperData.CreatorIP))
                mapperData.CreatorIP = RemoteIp;

            //event notification
            if (publishEvent)
                await EventPublisher.EntityUpdatedAsync(mapperData);

            var result = await Repository.UpdateAsync(mapperData);

            return Mapper.Map<TResultDto>(result);
        }

        /*Sadece değişen gincellenmesini istiyorsak*/
        //public virtual async Task<TResultDto> UpdateAsync(TEditDto tEditDto)
        //{
        //    if (tEditDto == null)
        //        throw new ArgumentNullException($"{nameof(tEditDto)}");

        //    var domainEntity = Mapper.Map<TTable>(tEditDto);

        //    var dbData = await Repository.FindByIdAsync(domainEntity.Id);
        //    if (dbData == null)
        //        throw new ArgumentNullException($"{domainEntity.Id} is null data");


        //    dbData daki propertylere tEditDto içinceki propertyleri atmamız yeterli  savechange TransactionalAttribute yapıyor
        //    dbContext ctr  this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; kaldırmamız gerekecek

        //    //var mapperData = Mapper.Map(domainEntity, dbData);

        //    //mapperData.ModifiedDate = DateTime.UtcNow;
        //    //mapperData.ModifierIP = RemoteIp;

        //    //if (string.IsNullOrEmpty(mapperData.CreatorIP))
        //    //    mapperData.CreatorIP = RemoteIp;

        //    //// event notification
        //    //// await _mediator.EntityUpdated(vendor);

        //    //var result = await Repository.UpdateAsync(mapperData);

        //    //return Mapper.Map<TResultDto>(result);
        //}

        public virtual async Task<TResultDto> DeleteAsync(Guid id, bool publishEvent = true)
        {
            var result = await Repository.DeleteAsync(id);

            //event notification
            if (publishEvent)
                await EventPublisher.EntityDeletedAsync(result);

            return Mapper.Map<TResultDto>(result);
        }

        #endregion

        #region Prop

        protected virtual string RemoteIp => EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext.Connection.RemoteIpAddress.ToString();

        #endregion
    }
}
