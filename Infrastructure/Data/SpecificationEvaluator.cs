using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

public class SpecificationEvaluator<T> where T : BaseEntity
{
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> specs)
    {
        if (specs != null)
        {
            if (specs.Criteria != null)
            {
                query = query.Where(specs.Criteria);
            }

            if (specs.OrderBy != null)
            {
                query = query.OrderBy(specs.OrderBy);
            }

            if (specs.OrderByDesc != null)
            {
                query = query.OrderByDescending(specs.OrderByDesc);
            }

            if (specs.IsDistinc)
            {
                query = query.Distinct();
            }

            if (specs.IsPagingEnabled)
            {
                query = query.Skip(specs.Skip).Take(specs.Take);
            }
        }

        query = specs.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = specs.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        return query;
    }

    public static IQueryable<TResult> GetQuery<TSpec, TResult>(
        IQueryable<T> query,
        ISpecification<T, TResult> specs)
    {

        if (specs.Criteria != null)
        {
            query = query.Where(specs.Criteria);
        }

        if (specs.OrderBy != null)
        {
            query = query.OrderBy(specs.OrderBy);
        }

        if (specs.OrderByDesc != null)
        {
            query = query.OrderByDescending(specs.OrderByDesc);
        }

        var selectQuery = query as IQueryable<TResult>;

        if (specs.Select != null)
        {
            selectQuery = query.Select(specs.Select);
        }

        if (specs.IsDistinc)
        {
            selectQuery = selectQuery?.Distinct();
        }

        if (specs.IsPagingEnabled)
        {
            selectQuery = selectQuery?.Skip(specs.Skip).Take(specs.Take);
        }


        return selectQuery ?? query.Cast<TResult>();


    }
}

