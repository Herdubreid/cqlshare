using BlazorState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Celin
{
    public enum DataAction
    {
        CANCEL,
        CLEAR,
        DOWNLOAD,
        DELETE
    }
    public partial class AppState
    {
        public class QueryResponseAction : IAction
        {
            public DataAction DataAction { get; set; }
            public Guid DataId { get; set; }
        }
        public class SubmitQueryAction : IAction { }
        public class SelectViewUserAction : IAction
        {
            public int ViewUser { get; set; }
        }
        public class SetQueryAction : IAction
        {
            public string Query { get; set; }
        }
    }
}
