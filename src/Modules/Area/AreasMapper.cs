using ItSupportServer.Data.Models;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Area
{
    [Mapper]
    public partial class AreasMapper
    {
        [MapperIgnoreSource(nameof(Areas.Id))]
        [MapperIgnoreSource(nameof(Areas.Employees))]
        [MapperIgnoreSource(nameof(Areas.CreatedAt))]
        [MapperIgnoreSource(nameof(Areas.UpdatedAt))]
        [MapperIgnoreSource(nameof(Areas.DeletedAt))]
        public partial AreaDto MapToAreaDto(Areas area);

        [MapperIgnoreTarget(nameof(Areas.AreaId))]
        [MapperIgnoreTarget(nameof(Areas.Employees))]
        [MapperIgnoreTarget(nameof(Areas.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Areas.DeletedAt))]
        public partial Areas MapToArea(CreateAreaDto areaDto);

        [MapperIgnoreTarget(nameof(Areas.Employees))]
        [MapperIgnoreTarget(nameof(Areas.CreatedAt))]
        [MapperIgnoreTarget(nameof(Areas.DeletedAt))]
        public partial void MapToArea(UpdateAreaDto areaDto, Areas area);

        public partial IQueryable<AreaDto> ProjectToAreaDto(IQueryable<Areas> area);
    }
}
