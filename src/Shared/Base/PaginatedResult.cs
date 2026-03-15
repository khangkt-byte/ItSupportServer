using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ItSupportServer.src.Shared.Base
{
    /// <summary>
    /// Paginated result for API responses
    /// Pattern: Immutable value object (DDD)
    /// Reference: Microsoft REST API Guidelines, GitHub API v3
    /// Security: Immutable responses prevent tampering
    /// </summary>
    public record PaginatedResult<T>
    {
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required int TotalCount { get; init; }
        public required int TotalPages { get; init; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public required List<T> Items { get; init; } = [];
    }

    /// <summary>
    /// Query parameters for pagination, sorting, and searching
    /// Pattern: Immutable request object
    /// Reference: ASP.NET Core best practices
    /// </summary>
    public record QueryParameters
    {
        private const int MinPage = 1;
        private const int MinPageSize = 1;
        private const int MaxPageSize = 100;
        private const int DefaultPageSize = 10;

        public int Page { get; init; } = MinPage;
        public int PageSize { get; init; } = DefaultPageSize;
        public string? SortBy { get; init; }
        public bool IsDescending { get; init; } = false;
        public string? Search { get; init; }

        // Optional module-specific filters
        public bool? IsLocked { get; init; }
        public int? DptId { get; init; }
        public int? AreaId { get; init; }
        public long? IssueId { get; init; }
        public string? Status { get; init; }

        /// <summary>
        /// Validated page (always >= 1)
        /// </summary>
        public int ValidatedPage => Page < MinPage ? MinPage : Page;

        /// <summary>
        /// Validated page size (1-100)
        /// </summary>
        public int ValidatedPageSize =>
            PageSize < MinPageSize ? DefaultPageSize :
            PageSize > MaxPageSize ? MaxPageSize :
            PageSize;
    }

    /// <summary>
    /// Extension methods for IQueryable pagination
    /// Pattern: Repository pattern extension
    /// Performance: Single query with Count + Skip/Take
    /// </summary>
    public static class PaginationExtensions
    {
        public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
            this IQueryable<T> query,
            QueryParameters parameters,
            string defaultSortField = "CreatedAt") where T : class
        {
            // ✅ Use validated values
            var page = parameters.ValidatedPage;
            var pageSize = parameters.ValidatedPageSize;

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            query = ApplySorting(query, parameters.SortBy, parameters.IsDescending, defaultSortField);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<T>
            {
                Page = page,
                PageSize = pageSize,
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