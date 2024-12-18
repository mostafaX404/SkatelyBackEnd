using Core.Entities;
using Core.Interfaces;

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
        }

        return query;
    }

    public static IQueryable<TResult> GetQuery<TSpec, TResult>(
        IQueryable<T> query,
        ISpecification<T, TResult> specs)
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

            var selectQuery = query as IQueryable<TResult>;

            if (specs.Select != null)
            {
                selectQuery = query.Select(specs.Select);
            }

            if (specs.IsDistinc)
            {
                selectQuery = selectQuery?.Distinct();
            }

            return selectQuery ?? query.Cast<TResult>();
        }

        return query.Cast<TResult>();
    }
}
