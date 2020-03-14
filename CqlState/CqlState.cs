using BlazorState;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace Celin
{
    public class QueryChangedArgs : EventArgs
    {
        public int User { get; set; }
        public string Query { get; set; }
    }
    public partial class CqlState : State<CqlState>
    {
        public readonly static SemaphoreSlim AddingUser = new SemaphoreSlim(1, 1);
        public StorageService Storage { get; }
        public int UserCount { get; set; } = 0;
        public Dictionary<int, string> Users { get; } = new Dictionary<int, string>();
        public Dictionary<int, string> Query { get; } = new Dictionary<int, string>();
        public List<QueryResponse> QueryResponses { get; set; }
        public event EventHandler<QueryChangedArgs> QueryChanged;
        public void QueryHasChanged(int user, string query)
        {
            QueryChangedArgs args = new QueryChangedArgs
            {
                User = user,
                Query = query
            };
            var handler = QueryChanged;
            handler?.Invoke(this, args);
        }
        public event EventHandler Changed;
        public void HasChanged(EventArgs e = null)
        {
            var handler = Changed;
            handler?.Invoke(this, e);
        }
        public override void Initialize()
        {
            if (QueryResponses == null)
            {
                QueryResponses = new List<QueryResponse>();
                foreach (var fname in Storage.ListFiles)
                {
                    using (StreamReader sr = File.OpenText(fname))
                    {
                        QueryResponses.Add(JsonSerializer.Deserialize<QueryResponse>(sr.ReadToEnd()));
                    }
                }
            }
        }
        public CqlState(StorageService storage)
        {
            Storage = storage;
        }
    }
}
