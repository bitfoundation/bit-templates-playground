using Bit.TemplatePlayground.Shared.Features.Todo;

namespace Bit.TemplatePlayground.Server.Api.Features.Todo;

/// <summary>
/// More info at src/Server/Bit.TemplatePlayground.Server.Api/Features/Mappers.md
/// </summary>
[Mapper]
public static partial class TodoMapper
{
    public static partial IQueryable<TodoItemDto> Project(this IQueryable<TodoItem> query);
    public static partial TodoItemDto Map(this TodoItem source);
    public static partial TodoItem Map(this TodoItemDto source);
    public static partial void Patch(this TodoItemDto source, TodoItem destination);
}
