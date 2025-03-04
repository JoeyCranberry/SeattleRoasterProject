namespace SeattleRoasterProject.Data.Services;

using Microsoft.JSInterop;

public class JsInteropService
{
    private readonly IJSRuntime _jsRuntime;
    
    public JsInteropService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetValueFromStorage(string key)
    {
        return await _jsRuntime.InvokeAsync<string>("GetValueFromStorage", key);
    }

    public async Task AddValueToList<T>(string key, T value)
    {
        await _jsRuntime.InvokeVoidAsync("AddValueToList", key, value );
    }

    public async Task RemoveValueFromListById(string key, string id)
    {
        await _jsRuntime.InvokeVoidAsync("RemoveValueFromList", key, id);
    }
}
