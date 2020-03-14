using BlazorState;
using System;

namespace Celin
{
    public class AppStateComponent : BlazorStateComponent, IDisposable
    {
        protected AppState AppState => Store.GetState<AppState>();
        protected CqlState CqlState => Store.GetState<CqlState>();
        protected void Update(object sender, EventArgs args) => InvokeAsync(StateHasChanged);
        protected override void OnInitialized()
        {
            base.OnInitialized();
            AppState.Changed += Update;
            CqlState.Changed += Update;
        }
        public new void Dispose()
        {
            AppState.Changed -= Update;
            CqlState.Changed -= Update;
            base.Dispose();
        }
    }
}
