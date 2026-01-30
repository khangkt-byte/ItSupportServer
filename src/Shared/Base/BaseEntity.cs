using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.src.Shared.Base
{
    public abstract class BaseEntity<T> : IHasId<T>, IAuditableEntity
    {
        public abstract T Id { get; }

        /// <summary>
        /// Timestamp when entity was created (set automatically by AuditInterceptor)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when entity was last updated (set automatically by AuditInterceptor)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Timestamp when entity was soft deleted (set automatically on soft delete)
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
