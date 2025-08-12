namespace OJS.Services.Common.Data
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Query;
    using OJS.Services.Common.Models.Users;
    using OJS.Data.Models.Common;
    using OJS.Services.Infrastructure;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IDataService<TEntity> : IService
        where TEntity : class, IEntity
    {
        Task Add(TEntity entity);

        Task AddMany(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        Task<int> Update(Expression<Func<TEntity, bool>> filter, Expression<Func<SetPropertyCalls<TEntity>,SetPropertyCalls<TEntity>>> setPropertyCalls);

        Task<int> Update(
            Expression<Func<TEntity, bool>> filterExpression,
            Expression<Func<TEntity, TEntity>> updateExpression,
            int batchSize);

        void UpdateMany(IEnumerable<TEntity> entities);

        void Delete(TEntity entity);

        void Delete(Expression<Func<TEntity, bool>>? filter = null);

        Task<int> Delete(Expression<Func<TEntity, bool>> filterExpression, int batchSize);

        void DeleteMany(IEnumerable<TEntity> entities);

        Task ExecuteDelete(Expression<Func<TEntity, bool>> filter);

        Task DeleteById(object id);

        void Attach(TEntity entity);

        void Detach(TEntity entity);

        EntityEntry<TEntity> GetEntry(TEntity entity);

        Task<IEnumerable<TEntity>> All(
            Expression<Func<TEntity, bool>>? filter = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool descending = false,
            int? skip = null,
            int? take = null);

        Task<IEnumerable<TResult>> AllTo<TResult>(
            Expression<Func<TEntity, bool>>? filter = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool descending = false,
            int? skip = null,
            int? take = null)
            where TResult : class;

        ValueTask<TEntity?> Find(params object[] keyValues);

        Task<TEntity?> OneById(object id);

        Task<TResult?> OneByIdTo<TResult>(object id)
            where TResult : class;

        Task<TEntity?> One(Expression<Func<TEntity, bool>> filter);

        Task<TResult?> OneTo<TResult>(Expression<Func<TEntity, bool>> filter)
            where TResult : class;

        Task<int> Count(Expression<Func<TEntity, bool>>? filter = null);

        Task<bool> Exists(Expression<Func<TEntity, bool>>? filter = null);

        Task<bool> ExistsById(object id);

        Task SaveChanges();

        // TODO: Refactor services to not use the following methods as public and remove them from here
        IQueryable<TEntity> GetByIdQuery(object id);

        IQueryable<TEntity> GetQuery(
            Expression<Func<TEntity, bool>>? filter = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool descending = false,
            int? skip = null,
            int? take = null);

        IQueryable<TEntity> GetQueryForUser(
            UserInfoModel user,
            Expression<Func<TEntity, bool>>? filter = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool descending = false,
            int? skip = null,
            int? take = null);

        IDataService<TEntity> IgnoreQueryFilters();

        IDataService<TEntity> WithQueryFilters();

        Task ExecuteSqlCommandWithTimeout(string query, int timeoutInSeconds);
    }
}