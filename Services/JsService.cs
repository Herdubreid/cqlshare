using MediatR;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Celin
{
    public class JsService
    {
        IJSRuntime JsRuntime { get; }
        IMediator Mediator { get; }
        DotNetObjectReference<JsService> Ref { get; }
        [JSInvokable]
        public void TextChanged(string _, string query)
        {
            Mediator.Send(new AppState.SetQueryAction { Query = query });
        }
        public void SaveAs(string filename, byte[] data)
        {
            JsRuntime.InvokeVoidAsync("window.saveAsFile", filename, Convert.ToBase64String(data));
        }
        public void InitEditor(string id, string text)
        {
            JsRuntime.InvokeVoidAsync("window.cqlEditor.init", Ref, id, text);
        }
        public void SetEditorText(string id, string text)
        {
            JsRuntime.InvokeVoidAsync("window.cqlEditor.setText", id, text);
        }
        public async Task<string> GetEditorTextAsync(string id)
        {
            return await JsRuntime.InvokeAsync<string>("window.cqlEditor.getText", id);
        }
        public void InitCqlViewer(string id, string text)
        {
            JsRuntime.InvokeVoidAsync("window.cqlViewer.init", id, text);
        }
        public void SetCqlViewerText(string id, string text)
        {
            JsRuntime.InvokeVoidAsync("window.cqlViewer.setText", id, text);
        }
        public void InitJsonViewer(string id, string text)
        {
            JsRuntime.InvokeVoidAsync("window.jsonViewer.init", id, text);
        }
        public void SetJsonText(string id, string text)
        {
            JsRuntime.InvokeVoidAsync("window.jsonViewer.setText", id, text);
        }
        public JsService(IJSRuntime jsRuntime, IMediator mediator)
        {
            JsRuntime = jsRuntime;
            Mediator = mediator;
            Ref = DotNetObjectReference.Create(this);
        }
    }
}
