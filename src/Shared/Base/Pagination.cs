using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace ItSupportServer.src.Shared.Base
{
    public class Pagination<T>
    {
        public static async Task<PaginatedResult<List<T>>> PaginationAsync(IQueryable<T> items, int pageNumber, int pageSize, SortOBJ? query)
        {
            if (query is not null &&
    !string.IsNullOrEmpty(query.FilterName) &&
    !string.IsNullOrEmpty(query.FilterValue))
            {
                var property = typeof(T).GetProperty(query.FilterName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    object? typedValue = null;

                    try
                    {
                        if (propertyType == typeof(bool))
                        {
                            if (bool.TryParse(query.FilterValue, out bool boolVal))
                                typedValue = boolVal;
                        }
                        else if (propertyType == typeof(int))
                        {
                            if (int.TryParse(query.FilterValue, out int intVal))
                                typedValue = intVal;
                        }
                        else if (propertyType == typeof(decimal))
                        {
                            if (decimal.TryParse(query.FilterValue, out decimal decVal))
                                typedValue = decVal;
                        }
                        else if (propertyType == typeof(DateTime))
                        {
                            if (DateTime.TryParse(query.FilterValue, out DateTime dateVal))
                                typedValue = dateVal;
                        }
                        else
                        {
                            typedValue = query.FilterValue;
                        }

                        if (typedValue != null)
                        {

                            if (propertyType == typeof(string))
                            {
                                items = items.Where($"{query.FilterName}.ToLower().Contains(@0)", typedValue.ToString()!.ToLower());
                            }
                            else
                            {
                                items = items.Where($"{query.FilterName} == @0", typedValue);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Filter parse error: {ex.Message}");
                    }
                }
            }
            var totalItems = await items.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (query is not null && !string.IsNullOrEmpty(query.FieldName))
            {
                items = items.OrderBy($"{query.FieldName} {(query.Isdesc == true ? "desc" : "asc")}");
            }
            else
            {
                items = items.OrderBy("createdAt desc");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await items.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            return new PaginatedResult<List<T>>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Data = result
            };
        }



        public static PaginatedResult<List<T>> PaginationList(IEnumerable<T> items, int pageNumber, int pageSize, SortOBJ? query)
        {
            // Sử dụng IEnumerable để lazy evaluation
            IEnumerable<T> filteredItems = items;

            // --- 1. FILTERING (LỌC) ---
            if (query is not null && !string.IsNullOrEmpty(query.FilterName) && !string.IsNullOrEmpty(query.FilterValue))
            {
                var property = typeof(T).GetProperty(query.FilterName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    // --- KHAI BÁO & PARSE GIÁ TRỊ (Phần bị thiếu trước đó) ---
                    object? typedValue = null;
                    bool parseSuccess = true;

                    try
                    {
                        if (propertyType == typeof(bool))
                            parseSuccess = bool.TryParse(query.FilterValue, out bool val) && (typedValue = val) is not null;
                        else if (propertyType == typeof(int))
                            parseSuccess = int.TryParse(query.FilterValue, out int val) && (typedValue = val) is not null;
                        else if (propertyType == typeof(decimal))
                            parseSuccess = decimal.TryParse(query.FilterValue, out decimal val) && (typedValue = val) is not null;
                        else if (propertyType == typeof(DateTime))
                            parseSuccess = DateTime.TryParse(query.FilterValue, out DateTime val) && (typedValue = val) is not null;
                        else if (propertyType == typeof(Guid))
                            parseSuccess = Guid.TryParse(query.FilterValue, out Guid val) && (typedValue = val) is not null;
                        else
                            typedValue = query.FilterValue; // Mặc định string
                    }
                    catch
                    {
                        parseSuccess = false;
                    }

                    // --- LOGIC LỌC TỐI ƯU ---
                    if (parseSuccess && typedValue != null)
                    {
                        // Lưu giá trị vào biến cục bộ để đảm bảo an toàn cho Lambda Closure
                        var finalValue = typedValue;

                        if (propertyType == typeof(string))
                        {
                            string searchVal = finalValue.ToString()!;
                            filteredItems = filteredItems.Where(x =>
                            {
                                var val = property.GetValue(x) as string;
                                // Dùng IndexOf thay vì ToLower() để giảm tải bộ nhớ
                                return val != null && val.IndexOf(searchVal, StringComparison.OrdinalIgnoreCase) >= 0;
                            });
                        }
                        else
                        {
                            filteredItems = filteredItems.Where(x =>
                            {
                                var val = property.GetValue(x);
                                return val != null && val.Equals(finalValue);
                            });
                        }
                    }
                }
            }

            var defaultSortProperty = typeof(T).GetProperty("CreatedAt", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                         ?? typeof(T).GetProperty("CreateAt", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            // --- 2. SORTING (SẮP XẾP) ---
            if (query != null && !string.IsNullOrEmpty(query.FieldName))
            {
                var sortProperty = typeof(T).GetProperty(query.FieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (sortProperty != null)
                {
                    // Sắp xếp theo Field được truyền vào
                    filteredItems = query.Isdesc == true
                        ? filteredItems.OrderByDescending(x => sortProperty.GetValue(x))
                        : filteredItems.OrderBy(x => sortProperty.GetValue(x));
                }
                else
                {
                    // Nếu không tìm thấy Field truyền vào, quay về mặc định
                    if (defaultSortProperty != null)
                    {
                        filteredItems = filteredItems.OrderByDescending(x => defaultSortProperty.GetValue(x));
                    }
                }
            }
            else
            {
                // Không truyền Sort, dùng mặc định
                if (defaultSortProperty != null)
                {
                    filteredItems = filteredItems.OrderByDescending(x => defaultSortProperty.GetValue(x));
                }
            }

            // --- 3. PAGINATION (PHÂN TRANG TỐI ƯU) ---
            // Duyệt qua IEnumerable để đếm (chậm hơn List.Count nhưng đỡ tốn RAM copy list)
            var totalItems = filteredItems.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var pagedData = filteredItems
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToList(); // Chỉ tạo list cho trang hiện tại

            return new PaginatedResult<List<T>>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Data = pagedData
            };
        }
    }



    public class PaginatedResult<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public T? Data { get; set; }
    }
    public class SortOBJ
    {
        public string? FieldName { get; set; }
        public bool? Isdesc { get; set; } = false;
        public string? FilterName { get; set; }
        public string? FilterValue { get; set; }
    }
}
