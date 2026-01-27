using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ItSupportServer.src.Shared.Base
{
    /// <summary>
    /// Paginated response following Microsoft REST API Guidelines
    /// </summary>
    public class PagedResponse<T>
    {
        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indicates if there is a previous page
        /// </summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Indicates if there is a next page
        /// </summary>
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// Data items for the current page
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// HATEOAS navigation links
        /// </summary>
        public PageLinks? Links { get; set; }
    }

    /// <summary>
    /// HATEOAS pagination links
    /// </summary>
    public class PageLinks
    {
        public string? First { get; set; }
        public string? Previous { get; set; }
        public string? Next { get; set; }
        public string? Last { get; set; }
    }

    /// <summary>
    /// Query parameters for pagination, sorting, and filtering
    /// </summary>
    public class QueryParameters
    {
        private int _page = 1;
        private int _pageSize = 10;

        /// <summary>
        /// Page number (default: 1, min: 1)
        /// </summary>
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Page size (default: 10, min: 1, max: 100)
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
        }

        /// <summary>
        /// Sort field name (e.g., "name", "createdAt")
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (true = descending, false = ascending)
        /// </summary>
        public bool IsDescending { get; set; } = false;

        /// <summary>
        /// Search query for full-text search
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Filter criteria (field:value pairs)
        /// </summary>
        public Dictionary<string, string>? Filters { get; set; }
    }

    /// <summary>
    /// Extension methods for IQueryable pagination
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Applies pagination to IQueryable with sorting
        /// </summary>
        public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
            this IQueryable<T> query,
            QueryParameters parameters,
            string? defaultSortField = null,
            string? baseUrl = null) where T : class
        {
            // Validate parameters
            if (parameters.Page < 1) parameters.Page = 1;
            if (parameters.PageSize < 1) parameters.PageSize = 10;
            if (parameters.PageSize > 100) parameters.PageSize = 100;

            // Get total count BEFORE sorting/paging
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

            // Apply sorting
            query = ApplySorting(query, parameters.SortBy, parameters.IsDescending, defaultSortField);

            // Apply pagination
            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            var response = new PagedResponse<T>
            {
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };

            // Generate HATEOAS links if baseUrl provided
            if (!string.IsNullOrEmpty(baseUrl))
            {
                response.Links = GenerateLinks(baseUrl, parameters, totalPages);
            }

            return response;
        }

        /// <summary>
        /// Applies sorting with fallback to default field
        /// </summary>
        private static IQueryable<T> ApplySorting<T>(
            IQueryable<T> query,
            string? sortField,
            bool isDescending,
            string? defaultSortField) where T : class
        {
            // If no sort field specified, use default
            var fieldToSort = sortField ?? defaultSortField ?? "Id";

            // Validate property exists
            var property = typeof(T).GetProperty(fieldToSort,
                System.Reflection.BindingFlags.IgnoreCase |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            if (property == null)
            {
                // Fall back to default if invalid property
                fieldToSort = defaultSortField ?? "Id";
                property = typeof(T).GetProperty(fieldToSort,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);
            }

            if (property == null) return query; // No valid sort field

            // Build expression tree for type-safe sorting
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = isDescending ? "OrderByDescending" : "OrderBy";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.PropertyType },
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        /// <summary>
        /// Generates HATEOAS navigation links
        /// </summary>
        private static PageLinks GenerateLinks(string baseUrl, QueryParameters parameters, int totalPages)
        {
            var links = new PageLinks();
            
            if (parameters.Page > 1)
            {
                links.First = BuildUrl(baseUrl, parameters, 1);
                links.Previous = BuildUrl(baseUrl, parameters, parameters.Page - 1);
            }

            if (parameters.Page < totalPages)
            {
                links.Next = BuildUrl(baseUrl, parameters, parameters.Page + 1);
                links.Last = BuildUrl(baseUrl, parameters, totalPages);
            }

            return links;
        }

        private static string BuildUrl(string baseUrl, QueryParameters parameters, int page)
        {
            var query = $"?page={page}&pageSize={parameters.PageSize}";
            if (!string.IsNullOrEmpty(parameters.SortBy))
                query += $"&sortBy={parameters.SortBy}&isDescending={parameters.IsDescending}";
            return $"{baseUrl.TrimEnd('/')}{query}";
        }
    }
}