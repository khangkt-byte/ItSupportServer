using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Area
{
    public record AreaDto(
        int AreaId,
        string Name,
        string? Description
        );

    public record CreateAreaDto(
        string Name,
        string? Description,
        DateTime CreatedAt
        );

    public record UpdateAreaDto(
        string AreaId,
        string? Name,
        string? Description,
        DateTime UpdatedAt
        );
}
