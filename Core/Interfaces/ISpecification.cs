using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ISpecification<T>
    {
        public Expression<Func<T, bool>>? Criteria { get; }

        public Expression<Func<T, object>>? OrderBy { get; }

        public Expression<Func<T, object>>? OrderByDesc { get; }

        bool IsDistinc { get; }

        int Take {  get; }
        int Skip { get; }

        IQueryable<T> AppplyCriateria(IQueryable<T> query);

        bool IsPagingEnabled { get; }
    }

    public interface ISpecification<T, TResult> : ISpecification<T>
    {
        Expression<Func<T, TResult>>? Select { get; }


    }
}
