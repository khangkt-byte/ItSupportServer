using Microsoft.EntityFrameworkCore;
using ItSupportServer.Data;
using System.Linq.Dynamic.Core;
using ItSupportServer.src.Shared.Helper;

namespace ItSupportServer.src.Shared.Base
{
    public class BaseCrud<T, TKey>(AppDbContext db) where T : BaseEntity<TKey>
    {
        public IQueryable<T> Get(string[]? fields, string? query, CancellationToken ct = default)
        {
            IQueryable<T> q = db.Set<T>().Where(e => e.DeletedAt == null);
            if (fields is not null && fields.Length > 0 && !string.IsNullOrEmpty(query))
            {
                var slug = ConverToSlug.GetSlug(query);
                var where = string.Join(" OR ", fields.Select(f => $"{f} != null && {f}.Contains(@0)"));
                var q1 = q.Where(where, query);
                var q2 = q.Where(where, slug);
                q = q1.Union(q2);
            }
            return q.AsNoTracking();
        }

        public async Task<T> FindById(TKey Id, bool IsDeleted = false, CancellationToken ct = default)
        {
            if (IsDeleted)
            {
                var data = await db.Set<T>().FirstOrDefaultAsync(d => d.Id!.Equals(Id), ct);
                return data ?? null;
            }
            else
            {
                var data = await db.Set<T>().FirstOrDefaultAsync(d => d.Id!.Equals(Id) && d.DeletedAt == null, ct);
                return data ?? null;
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
            catch (Exception e)
            {
                return false;
            }

        }
    }
}
