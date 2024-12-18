using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {

        Task<T> GetByIdAsync(int id);

        Task<IReadOnlyList<T>> ListAllAsync();

        Task<T?> GetEntityWithSpec(ISpecification<T> spec);

        Task<IReadOnlyList<T>> ListWithSpecAsync(ISpecification<T> spec);

        Task<TResult?> GetEntityWithSpec<TResult>(ISpecification<T,TResult> spec);

        Task<IReadOnlyList<TResult>> ListWithSpecAsync<TResult>(ISpecification<T,TResult> spec);

        void Add (T entity);

        void Update (T entity); 

        void Delete (T entity);

        Task<bool> SaveAllAsync();

        bool Exists(int id);

        Task<int> CountAsync(ISpecification<T> spec);


    }
}
