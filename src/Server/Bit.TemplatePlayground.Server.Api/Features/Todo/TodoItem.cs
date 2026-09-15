
namespace Bit.TemplatePlayground.Server.Api.Features.Todo;

public partial class TodoItem
{
    public string Id { get; set; } = default!;
    public DateTimeOffset? UpdatedAt { get; set; }

    [Required]
    public string? Title { get; set; }
    public bool IsDone { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
    public Guid UserId { get; set; }
}
