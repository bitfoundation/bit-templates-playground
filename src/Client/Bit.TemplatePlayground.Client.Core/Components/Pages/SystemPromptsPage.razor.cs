
using Bit.TemplatePlayground.Shared.Dtos.Chatbot;
using Bit.TemplatePlayground.Shared.Controllers.Chatbot;

namespace Bit.TemplatePlayground.Client.Core.Components.Pages;

public partial class SystemPromptsPage
{
    [AutoInject] private IChatbotController chatbotController = default!;

    private SystemPromptDto? systemPrompt;

    private bool isLoading = true;

    protected override async Task OnAfterFirstRenderAsync()
    {
        await base.OnAfterFirstRenderAsync();

        try
        {
            systemPrompt = await chatbotController.GetSystemPrompt(PromptKind.Support, CurrentCancellationToken);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SaveChanges()
    {
        if (await AuthManager.TryEnterElevatedAccessMode(CurrentCancellationToken))
        {
            (await chatbotController.UpdateSystemPrompt(systemPrompt!, CurrentCancellationToken)).Patch(systemPrompt);
        }
    }
}
