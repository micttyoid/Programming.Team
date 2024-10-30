using MudBlazor;
using Programming.Team.Core;
using Programming.Team.ViewModels;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Programming.Team.Web.Helpers
{
    public static class Helpers
    {
        private static Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> BuildSort<TEntity>(this ICollection<SortDefinition<TEntity>> sorts)
        {
            if (sorts == null || sorts.Count == 0)
            {
                throw new ArgumentException("No sorting definitions provided.");
            }

            return query =>
            {
                IOrderedQueryable<TEntity>? orderedQuery = null;

                foreach (var sort in sorts)
                {
                    var parameter = Expression.Parameter(typeof(TEntity), "entity");
                    var property = Expression.Property(parameter, sort.SortBy);
                    var lambda = Expression.Lambda(property, parameter);

                    var methodName = orderedQuery == null
                        ? (sort.Descending ? "OrderByDescending" : "OrderBy")
                        : (sort.Descending ? "ThenByDescending" : "ThenBy");

                    var method = typeof(Queryable).GetMethods()
                        .First(m => m.Name == methodName && m.GetParameters().Length == 2);

                    var genericMethod = method.MakeGenericMethod(typeof(TEntity), property.Type);
                    if (genericMethod == null)
                        continue;
                    orderedQuery = genericMethod.Invoke(null, new object[] { orderedQuery ?? query, lambda }) as IOrderedQueryable<TEntity>;
                }

                return orderedQuery!;
            };
        }
        private static Expression<Func<TEntity, bool>> BuildFilterExpression<TEntity>(this ICollection<IFilterDefinition<TEntity>> filters)
        {
            if (filters == null || filters.Count == 0)
                return entity => true; // No filters, return always true

            // Parameter expression for the entity
            var parameter = Expression.Parameter(typeof(TEntity), "entity");

            // Start with an empty expression
            Expression? finalExpression = null;

            // Iterate over each filter to build individual expressions
            foreach (var filter in filters)
            {
                if (filter.Column == null)
                    continue;
                var property = Expression.Property(parameter, filter.Column.PropertyName);
                var constant = Expression.Constant(filter.Value);

                // Create the binary expression based on the filter operator
                Expression? filterExpression = filter.Operator switch
                {
                    FilterOperator.String.Equal or
                    FilterOperator.Number.Equal or
                    FilterOperator.DateTime.Is or
                    FilterOperator.Enum.Is or
                    FilterOperator.Boolean.Is
                        => Expression.Equal(property, constant),

                    FilterOperator.String.NotEqual or
                    FilterOperator.Number.NotEqual or
                    FilterOperator.DateTime.IsNot or
                    FilterOperator.Enum.IsNot
                        => Expression.NotEqual(property, constant),

                    FilterOperator.Number.GreaterThan or
                    FilterOperator.DateTime.After
                        => Expression.GreaterThan(property, constant),

                    FilterOperator.Number.GreaterThanOrEqual or
                    FilterOperator.DateTime.OnOrAfter
                        => Expression.GreaterThanOrEqual(property, constant),

                    FilterOperator.Number.LessThan or
                    FilterOperator.DateTime.Before
                        => Expression.LessThan(property, constant),

                    FilterOperator.Number.LessThanOrEqual or
                    FilterOperator.DateTime.OnOrBefore
                        => Expression.LessThanOrEqual(property, constant),

                    FilterOperator.String.Contains
                        => BuildContainsExpression(property, filter.Value),

                    FilterOperator.String.StartsWith
                        => BuildStartsWithExpression(property, filter.Value),

                    FilterOperator.String.EndsWith
                        => BuildEndsWithExpression(property, filter.Value),

                    FilterOperator.String.Empty or
                    FilterOperator.Number.Empty or
                    FilterOperator.DateTime.Empty
                        => Expression.Equal(property, Expression.Constant(null)),

                    FilterOperator.String.NotEmpty or
                    FilterOperator.Number.NotEmpty or
                    FilterOperator.DateTime.NotEmpty
                        => Expression.NotEqual(property, Expression.Constant(null)),

                    _ => throw new NotSupportedException($"Operator '{filter.Operator}' is not supported")
                };


                if (finalExpression == null)
                {
                    finalExpression = filterExpression;
                }
                else
                {
                    finalExpression = Expression.AndAlso(finalExpression, filterExpression);
                }
            }

            // Convert the final expression into a lambda expression
            return Expression.Lambda<Func<TEntity, bool>>(finalExpression ?? Expression.Constant(true), parameter);
        }
        private static Expression BuildStartsWithExpression(Expression property, object? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (property.Type != typeof(string))
                throw new NotSupportedException("StartsWith operator is only supported for string properties.");

            var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            return Expression.Call(property, method!, Expression.Constant(value));
        }

        private static Expression BuildEndsWithExpression(Expression property, object? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (property.Type != typeof(string))
                throw new NotSupportedException("EndsWith operator is only supported for string properties.");

            var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
            return Expression.Call(property, method!, Expression.Constant(value));
        }
        private static Expression BuildContainsExpression(Expression property, object? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            return Expression.Call(property, method!, Expression.Constant(value));
        }
        public static Func<GridState<TEntity>, Task<GridData<TEntity>>> BindServerDataCommand<TKey, TEntity>
            (this ReactiveCommand<DataGridRequest<TKey, TEntity>, RepositoryResultSet<TKey, TEntity>?> command)
            where TKey: struct
            where TEntity: Entity<TKey>, new()
        {
            Func<GridState<TEntity>, Task<GridData<TEntity>>> func = async state =>
            {
                GridData<TEntity> data = new GridData<TEntity>();
                DataGridRequest<TKey, TEntity> request = new DataGridRequest<TKey, TEntity>();
                if (state.FilterDefinitions.Any())
                {
                    request.Filter = state.FilterDefinitions.BuildFilterExpression();
                }
                if(state.SortDefinitions.Any())
                {
                    request.OrderBy = state.SortDefinitions.BuildSort();
                }
                if (state.PageSize > 0)
                {
                    request.Pager = new Pager() { Page = state.Page + 1, Size = state.PageSize };
                }
                var resp = await command.Execute(request).GetAwaiter();
                if (resp != null)
                {
                    data.Items = resp.Entities.ToArray();
                    data.TotalItems = resp.Count ?? 0;
                }
                return data;
            };
            return func;
        }
        
    }
}
