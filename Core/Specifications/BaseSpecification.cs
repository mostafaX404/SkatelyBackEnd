using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Specifications
{
    public class BaseSpecification<T>(Expression<Func<T,bool>>? _criteria ) : ISpecification<T>
    {
        protected BaseSpecification():this(null){}
        public Expression<Func<T,bool>>? Criteria => _criteria;

        public Expression<Func<T, object>>? OrderBy { get; private set; }

        public Expression<Func<T, object>>? OrderByDesc { get; private set; }

        public bool IsDistinc { get; private set; }

        public int Take { get; private set; }
        public int Skip { get; private set; }


        public bool IsPagingEnabled { get; private set; }

        public void AddOrderBy(Expression<Func<T, object>>? orderBy)
        {
            this.OrderBy = orderBy; 
        }

        public void AddOrderByDesc(Expression<Func<T, object>>? orderByDesc)
        {
            this.OrderByDesc = orderByDesc;
        }

        public IQueryable<T> AppplyCriateria(IQueryable<T> query)
        {
            if(Criteria != null)
            {
                query = query.Where(Criteria);
            }

            return query;
        }

        protected void ApplyDistinc()
        {
            IsDistinc = true;
        }

        protected void ApplyPaging(int skip , int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
    }


    public class BaseSpecification<T, TResult>(Expression<Func<T, bool>>? _criteria) : BaseSpecification<T>(_criteria), ISpecification<T, TResult>
    {
        protected BaseSpecification() : this(null) { }


        public Expression<Func<T, TResult>>? Select { get; private set; }


        protected void AddSelect(Expression<Func<T, TResult>> selectExpression)
        {
            Select = selectExpression;
        }
    
    }

}
