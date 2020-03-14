using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Celin
{
    public class QueryResponseData<T>
    {
        public IEnumerable<T> rowset { get; set; }
        public IEnumerable<T> output { get; set; }
    }
    public class QueryResponse
    {
        public Guid Id { get; set; }
        public string Query { get; set; }
        public string Title { get; set; }
        public List<string> Columns { get; set; }
        public string Environment { get; set; }
        public DateTime Submitted { get; set; }
        public AIS.Summary Summary { get; set; }
        public int Count { get; set; }
        public bool Demo { get; set; }
        public string Error { get; set; }
        [JsonIgnore]
        public bool Busy { get; set; }
    }
}
