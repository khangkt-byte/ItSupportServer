using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ItSupportServer.src.Shared.Base
{
    /// <summary>
    /// Paginated result for list endpoints
    /// Follows Microsoft REST API Guidelines
    /// </summary>
    public class PaginatedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public List<T> Items { get; set; } = [];
    }

    /// <summary>
    /// Query parameters for pagination, sorting, and searching
    /// </summary>
    public class QueryParameters
    {
        private int _page = 1;
        private int _pageSize = 10;

        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
        }

        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = false;
        public string? Search { get; set; }
    }

    /// <summary>
    /// Extension methods for IQueryable pagination
    /// </summary>
    public static class PaginationExtensions
    {
        public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
            this IQueryable<T> query,
            QueryParameters parameters,
            string defaultSortField = "CreatedAt") where T : class
        {
            if (parameters.Page < 1) parameters.Page = 1;
            if (parameters.PageSize < 1) parameters.PageSize = 10;
            if (parameters.PageSize > 100) parameters.PageSize = 100;

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

            query = ApplySorting(query, parameters.SortBy, parameters.IsDescending, defaultSortField);

            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PaginatedResult<T>
            {
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }

        private static IQueryable<T> ApplySorting<T>(
            IQueryable<T> query,
            string? sortField,
            bool isDescending,
            string defaultSortField) where T : class
        {
            var fieldToSort = sortField ?? defaultSortField;

            var property = typeof(T).GetProperty(fieldToSort,
                System.Reflection.BindingFlags.IgnoreCase |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            if (property == null)
            {
                property = typeof(T).GetProperty(defaultSortField,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);
            }

            if (property == null) return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = isDescending ? "OrderByDescending" : "OrderBy";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                [typeof(T), property.PropertyType],
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
}