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

    public async Task ShowModal(string elementSelector)
    {
        await _jsRuntime.InvokeVoidAsync("ShowModal", elementSelector);
    }

    public async Task HideModal(string elementSelector)
    {
        await _jsRuntime.InvokeVoidAsync("HideModal", elementSelector);
    }

    public async Task FocusElement(string elementId)
    {
        await _jsRuntime.InvokeVoidAsync("FocusElement", elementId);
    }

    public async Task ScrollToTop()
    {
        await _jsRuntime.InvokeVoidAsync("ScrollToTop");
    }

    public async Task ScrollToElement(string elementId)
    {
        await _jsRuntime.InvokeVoidAsync("ScrollToElement", elementId);
    }

    public async Task UnfocusElement(string elementId)
    {
        await _jsRuntime.InvokeVoidAsync("UnfocusElement", elementId);
    }

    public async Task SetToggleButtonActive(string elementId)
    {
        await _jsRuntime.InvokeVoidAsync("SetToggleButtonActive", elementId);
    }

    public async Task SetToggleButtonInactive(string elementId)
    {
        await _jsRuntime.InvokeVoidAsync("SetToggleButtonInactive", elementId);
    }

    public async Task ShowNewToast(string elementId)
    {
        await _jsRuntime.InvokeVoidAsync("ShowNewToast", elementId);
    }

    public async Task HideToast(string elementId)
    {
        await _jsRuntime.InvokeVoidAsync("HideToast", elementId);
    }
}
