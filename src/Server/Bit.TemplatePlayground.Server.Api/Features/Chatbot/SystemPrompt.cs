using Bit.TemplatePlayground.Shared.Features.Chatbot;

namespace Bit.TemplatePlayground.Server.Api.Features.Chatbot;

public class SystemPrompt
{
    public Guid Id { get; set; }

    public PromptKind PromptKind { get; set; }

    [Required]
    public string? Markdown { get; set; }

    public long Version { get; set; }
}
