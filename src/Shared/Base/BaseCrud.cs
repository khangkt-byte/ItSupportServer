using ItSupportServer.Data;
using ItSupportServer.src.Shared.Helper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ItSupportServer.src.Shared.Base
{
    public class BaseCrud<T, TKey>(AppDbContext db) where T : BaseEntity<TKey>
    {
        public IQueryable<T> Get(
            string[]? fields,
            string? query)
        {
            var q = db.Set<T>()
                      .Where(e => e.DeletedAt == null);

            if (fields == null || fields.Length == 0 || string.IsNullOrWhiteSpace(query))
                return q.AsNoTracking();

            var slug = ConvertToSlug.GetSlug(query);

            var parameter = Expression.Parameter(typeof(T), "e");
            Expression? predicate = null;

            foreach (var field in fields)
            {
                var property = typeof(T).GetProperty(field);
                if (property == null || property.PropertyType != typeof(string))
                    continue;

                var member = Expression.Property(parameter, property);

                // e.Field != null
                var notNull = Expression.NotEqual(
                    member,
                    Expression.Constant(null, typeof(string)));

                // EF.Functions.ILike(e.Field, %query%)
                Expression BuildILike(string value)
                {
                    return Expression.Call(
                        typeof(NpgsqlDbFunctionsExtensions),
                        nameof(NpgsqlDbFunctionsExtensions.ILike),
                        Type.EmptyTypes,
                        Expression.Property(null, typeof(EF), nameof(EF.Functions)),
                        member,
                        Expression.Constant($"%{value}%"));
                }

                var condition =
                    Expression.AndAlso(
                        notNull,
                        Expression.OrElse(
                            BuildILike(query),
                            BuildILike(slug)));

                predicate = predicate == null
                    ? condition
                    : Expression.OrElse(predicate, condition);
            }
            if (predicate == null)
                return q.AsNoTracking();

            var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
            return q.Where(lambda).AsNoTracking();
        }

        public async Task<T> FindByIdAsync(TKey Id, bool IsDeleted = false, CancellationToken ct = default)
        {
            if (IsDeleted)
            {
                var data = await db.Set<T>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id!.Equals(Id), ct);
                return data ?? null!;
            }
            else
            {
                var data = await db.Set<T>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id!.Equals(Id) && d.DeletedAt == null, ct);
                return data ?? null!;
            }
        }

        public async Task<bool> Delete(TKey Id, bool SoftDelete = true, CancellationToken ct = default)
        {
            try
            {
                var existing = await db.Set<T>().FindAsync(Id, ct);
                if (existing is null) return false;
                if (SoftDelete)
                {
                    existing.DeletedAt = DateTime.UtcNow;
                    db.Set<T>().UpdateRange(existing);
                    await db.SaveChangesAsync(ct);
                    return true;
                }
                else
                {
                    db.Set<T>().RemoveRange(existing);
                    await db.SaveChangesAsync(ct);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
