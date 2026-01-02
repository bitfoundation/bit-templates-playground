using System.Threading.Channels;
using Bit.TemplatePlayground.Shared.Dtos.Chatbot;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;

namespace Bit.TemplatePlayground.Client.Core.Components.Layout;

public partial class AppAiChatPanel
{
    [CascadingParameter] public BitDir? CurrentDir { get; set; }

    [CascadingParameter] public AppThemeType? CurrentTheme { get; set; }


    [AutoInject] private HubConnection hubConnection = default!;


    private bool isOpen;
    private bool isLoading;
    private string? userInput;
    private bool isSmallScreen;
    private int responseCounter;
    private Channel<string>? channel;
    private AiChatMessage? lastAssistantMessage;
    private List<AiChatMessage> chatMessages = []; // TODO: Persist these values in client-side storage to retain them across app restarts.
    private List<string> followUpSuggestions = [];


    protected override Task OnInitAsync()
    {


        return base.OnInitAsync();
    }

    protected override async Task OnAfterFirstRenderAsync()
    {
        SetDefaultValues();
        StateHasChanged();
        hubConnection.Reconnected += HubConnection_Reconnected;

        await base.OnAfterFirstRenderAsync();
    }


    private async Task HubConnection_Reconnected(string? _)
    {
        if (channel is null) return;
        await RestartChannel();
    }

    private async Task SendPromptMessage(string message)
    {
        followUpSuggestions = [];
        userInput = message;
        StateHasChanged();
        await SendMessage();
    }

    private async Task SendMessage()
    {
        if (channel is null)
        {
            _ = StartChannel();
        }

        isLoading = true;

        var input = userInput;
        userInput = string.Empty;

        chatMessages.Add(new() { Content = input, Role = AiChatMessageRole.User });
        lastAssistantMessage = new() { Role = AiChatMessageRole.Assistant };
        chatMessages.Add(lastAssistantMessage);

        StateHasChanged();

        await channel!.Writer.WriteAsync(input!, CurrentCancellationToken);
    }

    private async Task ClearChat()
    {
        SetDefaultValues();

        await RestartChannel();
    }

    private void SetDefaultValues()
    {
        isLoading = false;
        responseCounter = 0;
        followUpSuggestions = [];
        lastAssistantMessage = new() { Role = AiChatMessageRole.Assistant };
        chatMessages = [
            new()
            {
                Role = AiChatMessageRole.Assistant,
                Content = Localizer[nameof(AppStrings.AiChatPanelInitialResponse)],
            }
        ];
    }

    private async Task HandleOnDismissPanel()
    {
        await StopChannel();
    }

    private async Task HandleOnUserInputEnter(KeyboardEventArgs e)
    {
        if (e.ShiftKey) return;

        await SendMessage();
    }

    private async Task StartChannel()
    {
        channel = Channel.CreateUnbounded<string>(new() { SingleReader = true, SingleWriter = true });

        // The following code streams user's input messages to the server and processes the streamed responses.
        // It keeps the chat ongoing until CurrentCancellationToken is cancelled.
        await foreach (var response in hubConnection.StreamAsync<string>(SharedAppMessages.StartChat,
                                                                         new StartChatRequest()
                                                                         {
                                                                             CultureId = CultureInfo.CurrentCulture.LCID,
                                                                             TimeZoneId = TimeZoneInfo.Local.Id,
                                                                             DeviceInfo = TelemetryContext.Platform,
                                                                             ChatMessagesHistory = chatMessages,
                                                                             ServerApiAddress = AbsoluteServerAddress.GetAddress()
                                                                         },
                                                                         channel.Reader.ReadAllAsync(CurrentCancellationToken),
                                                                         cancellationToken: CurrentCancellationToken))
        {
            int expectedResponsesCount = chatMessages.Count(c => c.Role is AiChatMessageRole.User);

            if (response.Contains(nameof(AiChatFollowUpList.FollowUpSuggestions)))
            {
                followUpSuggestions = JsonSerializer.Deserialize<AiChatFollowUpList>(response)?.FollowUpSuggestions ?? [];
            }
            else
            {
                if (response is SharedAppMessages.MESSAGE_PROCESS_SUCCESS)
                {
                    responseCounter++;
                    isLoading = false;
                }
                else if (response is SharedAppMessages.MESSAGE_PROCESS_ERROR)
                {
                    responseCounter++;
                    if (responseCounter == expectedResponsesCount)
                    {
                        isLoading = false; // Hide loading only if this is an error for the last user's message.
                    }
                    chatMessages[responseCounter * 2].Successful = false;
                }
                else
                {
                    if ((responseCounter + 1) == expectedResponsesCount)
                    {
                        lastAssistantMessage!.Content += response;
                    }
                }
            }

            StateHasChanged();
        }
    }

    private async Task StopChannel()
    {
        if (channel is null) return;

        channel.Writer.Complete();
        channel = null;
    }

    private async Task RestartChannel()
    {
        await StopChannel();
        await StartChannel();
    }


    protected override async ValueTask DisposeAsync(bool disposing)
    {


        hubConnection.Reconnected -= HubConnection_Reconnected;

        await StopChannel();

        await base.DisposeAsync(disposing);
    }
}
