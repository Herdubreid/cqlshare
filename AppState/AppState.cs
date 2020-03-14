using BlazorState;
using System;
using System.Text.Json.Serialization;
using System.Threading;

namespace Celin
{
    public partial class AppState : State<AppState>, IDisposable
    {
        IStore Store { get; }
        public static event EventHandler Changed;
        [JsonIgnore]
        public static CancellationTokenSource QueryTask { get; set; }
        public string ParseError { get; set; }
        public int? User { get; set; }
        public int? ViewUser { get; set; }
        public bool Busy { get; set; }
        public bool Downloading { get; set; }
        public string Query { get; set; }
        public override void Initialize() { }
        public void Dispose()
        {
            var state = Store.GetState<CqlState>();

            state.Users.Remove(User.Value);
            state.Query.Remove(User.Value);

            state.HasChanged();
        }
        public AppState(IStore store)
        {
            Store = store;

            var state = Store.GetState<CqlState>();
            User = ++state.UserCount;
            state.Users.Add(User.Value, string.Empty);
            state.Query.Add(User.Value, string.Empty);

            state.HasChanged();
        }
    }
}
