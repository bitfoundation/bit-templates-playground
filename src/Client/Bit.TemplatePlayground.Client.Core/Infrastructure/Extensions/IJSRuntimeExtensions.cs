namespace Microsoft.JSInterop;

public static partial class IJSRuntimeExtensions
{
    extension(IJSRuntime jsRuntime)
    {
        public ValueTask<string> GoogleRecaptchaGetResponse()
        {
            return jsRuntime.InvokeAsync<string>("grecaptcha.getResponse");
        }

        public ValueTask<string> GoogleRecaptchaReset()
        {
            return jsRuntime.InvokeAsync<string>("grecaptcha.reset");
        }

        /// <summary>
        /// The return value would be false during pre-rendering
        /// </summary>
        public bool IsInitialized()
        {
            return jsRuntime is not null && jsRuntime.IsRuntimeInvalid() is false;
        }
    }
}
