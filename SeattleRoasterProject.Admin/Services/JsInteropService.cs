using Microsoft.JSInterop;

namespace SeattleRoasterProject.Admin.Services;

public class JsInteropService
{
    private readonly IJSRuntime _jsRuntime;

    public JsInteropService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeFlowbite()
    {
        await _jsRuntime.InvokeVoidAsync("InitializeFlowbite");
    }

    public async Task MakeModalDraggable(string modalSelector)
    {
        await _jsRuntime.InvokeVoidAsync("MakeModalDraggable", modalSelector);
    }
}
