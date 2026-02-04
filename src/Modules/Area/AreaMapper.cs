using ItSupportServer.Data.Models.Entities;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Area
{
    [Mapper]
    public partial class AreaMapper
    {
        [MapperIgnoreSource(nameof(Areas.Id))]
        [MapperIgnoreSource(nameof(Areas.Employees))]
        [MapperIgnoreSource(nameof(Areas.DeletedAt))]
        [MapperIgnoreSource(nameof(Areas.IssueLogs))]
        public partial AreaDto MapToAreaDto(Areas area);

        [MapperIgnoreTarget(nameof(Areas.AreaId))]
        [MapperIgnoreTarget(nameof(Areas.Employees))]
        [MapperIgnoreTarget(nameof(Areas.CreatedAt))]
        [MapperIgnoreTarget(nameof(Areas.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Areas.DeletedAt))]
        [MapperIgnoreTarget(nameof(Areas.IssueLogs))]
        public partial Areas MapToArea(CreateAreaDto areaDto);

        [MapperIgnoreTarget(nameof(Areas.AreaId))]
        [MapperIgnoreTarget(nameof(Areas.Employees))]
        [MapperIgnoreTarget(nameof(Areas.CreatedAt))]
        [MapperIgnoreTarget(nameof(Areas.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Areas.DeletedAt))]
        [MapperIgnoreTarget(nameof(Areas.IssueLogs))]
        public partial void MapToArea(UpdateAreaDto areaDto, Areas area);

        public partial IQueryable<AreaDto> ProjectToAreaDto(IQueryable<Areas> area);
    }
}
