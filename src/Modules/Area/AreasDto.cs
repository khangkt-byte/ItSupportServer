using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Area
{
    public record AreaDto(
        string Name,
        string? Description
        );

    public record CreateAreaDto(
        string Name,
        string? Description
        );

    public record UpdateAreaDto(
        string AreaId,
        string? Name,
        string? Description
        );
}
