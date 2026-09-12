using System.Reflection;

namespace Microsoft.JSInterop;

public static partial class IJSRuntimeExtensions
{
    extension(IJSRuntime jsRuntime)
    {
        public ValueTask<string> GetTimeZone()
        {
            return jsRuntime.InvokeAsync<string>("App.getTimeZone");
        }

        public ValueTask<string> GoogleRecaptchaGetResponse()
        {
            return jsRuntime.InvokeAsync<string>("grecaptcha.getResponse");
        }

        public ValueTask<string> GoogleRecaptchaReset()
        {
            return jsRuntime.InvokeAsync<string>("grecaptcha.reset");
        }

        public async ValueTask<PushNotificationSubscriptionDto> GetPushNotificationSubscription(string vapidPublicKey)
        {
            return await jsRuntime.InvokeAsync<PushNotificationSubscriptionDto>("App.getPushNotificationSubscription", vapidPublicKey);
        }

        /// <summary>
        /// The return value would be false during pre-rendering
        /// </summary>
        public bool IsInitialized()
        {
            if (jsRuntime is null)
                return false;

            var type = jsRuntime.GetType();

            return type.Name switch
            {
                "UnsupportedJavaScriptRuntime" => false, // pre-rendering
                "RemoteJSRuntime" /* blazor server */ => (bool)type.GetProperty("IsInitialized")!.GetValue(jsRuntime)!,
                "WebViewJSRuntime" /* blazor hybrid */ => type.GetField("_ipcSender", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(jsRuntime) is not null,
                _ => true // blazor wasm
            };
        }

        /// <summary>
        /// Clears web browser / web view storages
        /// </summary>
        public async Task ClearWebStorages()
        {
            await jsRuntime.InvokeVoidAsync("App.clearWebStorages");
        }
    }
}
