using BlazorState;
using MediatR;
using OfficeOpenXml;
using Pidgin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Pidgin.Parser;
using Unit = MediatR.Unit;

namespace Celin
{
    public partial class AppState
    {
        public class QueryResponseHandler : ActionHandler<QueryResponseAction>
        {
            JsService Js { get; }
            StorageService Storage { get; }
            CqlState CqlState => Store.GetState<CqlState>();
            public override Task<Unit> Handle(QueryResponseAction aAction, CancellationToken aCancellationToken)
            {
                switch (aAction.DataAction)
                {
                    case DataAction.CANCEL:
                        AppState.QueryTask.Cancel();
                        break;
                    case DataAction.CLEAR:
                        CqlState.QueryResponses.Remove(CqlState.QueryResponses.Find(r => r.Id.CompareTo(aAction.DataId) == 0));
                        break;
                    case DataAction.DELETE:
                        CqlState.QueryResponses.Remove(CqlState.QueryResponses.Find(r => r.Id.CompareTo(aAction.DataId) == 0));
                        Storage.Delete(aAction.DataId.ToString());
                        Storage.DeleteData(aAction.DataId.ToString());
                        break;
                    case DataAction.DOWNLOAD:
                        try
                        {
                            var rsp = CqlState.QueryResponses.Find(r => r.Id.CompareTo(aAction.DataId) == 0);
                            var data = JsonSerializer.Deserialize<QueryResponseData<object[]>>(
                                Storage.OpenData(aAction.DataId.ToString()).ReadToEnd(),
                                new JsonSerializerOptions
                                {
                                    Converters = { new ObjectArrayConverter() }
                                });
                            using var xl = new ExcelPackage();
                            using var ws = xl.Workbook.Worksheets.Add(rsp.Title);
                            var rows = data.output ?? data.rowset;
                            var header = new List<string[]>
                            {
                                new string[] { "Query:", rsp.Query },
                                new string[] { "Submitted:", rsp.Submitted.ToString() },
                                rsp.Columns.ToArray()
                            };
                            ws.Cells["A1:B5"].LoadFromArrays(header);
                            ws.Cells["A4:A6"].LoadFromArrays(rows);
                            Js.SaveAs("data.xlsx", xl.GetAsByteArray());
                        }
                        finally { }
                        break;
                }

                CqlState.HasChanged();

                return Unit.Task;
            }
            public QueryResponseHandler(IStore store, StorageService storage, JsService js) : base(store)
            {
                Js = js;
                Storage = storage;
            }
        }
        public class SubmitQueryHandler : ActionHandler<SubmitQueryAction>
        {
            E1Service E1 { get; }
            StorageService Storage { get; }
            CqlState CqlState => Store.GetState<CqlState>();
            AppState State => Store.GetState<AppState>();
            public override async Task<Unit> Handle(SubmitQueryAction aAction, CancellationToken aCancellationToken)
            {
                State.Busy = true;
                var rsp = new QueryResponse
                {
                    Id = Guid.NewGuid(),
                    Query = State.Query,
                    Busy = true,
                    Title = "Working...",
                    Environment = "DEMO",
                    Submitted = DateTime.Now
                };
                try
                {
                    var request = AIS.Data.DataRequest.Parser.Before(Char(';')).ParseOrThrow(State.Query.TrimEnd() + ';');
                    rsp.Demo = request.formServiceDemo != null;
                    CqlState.QueryResponses.Insert(0, rsp);
                    CqlState.HasChanged();
                    AppState.QueryTask = new CancellationTokenSource(60000);
                    var result = await E1.RequestAsync<JsonElement>(request, AppState.QueryTask).ConfigureAwait(false);
                    rsp = CqlState.QueryResponses.Find(r => r.Id.CompareTo(rsp.Id) == 0);
                    var it = result.EnumerateObject();
                    QueryResponseData<JsonElement> data = null;
                    if (it.MoveNext())
                    {
                        var n = it.Current.Name;
                        if (n.StartsWith("fs_"))
                        {
                            rsp.Title = it.Current.Value.GetProperty("title").GetString();
                            var fm = JsonSerializer.Deserialize<Celin.AIS.Form<Celin.AIS.FormData<JsonElement>>>(it.Current.Value.ToString());
                            rsp.Summary = fm.data.gridData.summary;
                            rsp.Count = rsp.Summary.records;
                            if (rsp.Count > 0)
                            {
                                rsp.Columns = new List<string>();
                                foreach (var c in fm.data.gridData.rowset[0].EnumerateObject()) rsp.Columns.Add(c.Name);
                                data = new QueryResponseData<JsonElement> { rowset = fm.data.gridData.rowset };
                            }
                        }
                        else if (n.StartsWith("ds_"))
                        {
                            rsp.Title = n;
                            var ds = JsonSerializer.Deserialize<Celin.AIS.Output<JsonElement>>(it.Current.Value.ToString());
                            if (ds.error != null)
                            {
                                rsp.Error = ds.error.message;
                            }
                            else
                            {
                                rsp.Count = ds.output.Length;
                                if (rsp.Count > 0)
                                {
                                    rsp.Columns = new List<string>();
                                    foreach (var c in ds.output[0].EnumerateObject())
                                    {
                                        if (c.Value.ValueKind == JsonValueKind.Object)
                                        {
                                            foreach (var sc in c.Value.EnumerateObject())
                                            {
                                                rsp.Columns.Add($"{c.Name}.{sc.Name}");
                                            }
                                        }
                                        else
                                        {
                                            rsp.Columns.Add(c.Name);
                                        }
                                    }
                                    data = new QueryResponseData<JsonElement> { output = ds.output };
                                }
                            }
                        }
                        else if (n.CompareTo("sysErrors") == 0)
                        {
                            var errs = JsonSerializer.Deserialize<IEnumerable<Celin.AIS.ErrorWarning>>(it.Current.Value.ToString());
                            foreach (var e in errs)
                            {
                                rsp.Error = e.DESC;
                            }
                        }
                        if (rsp.Count > 0)
                        {
                            using (StreamWriter sw = Storage.Create(rsp.Id.ToString()))
                            {
                                sw.Write(JsonSerializer.Serialize(rsp));
                            }
                            using (StreamWriter sw = Storage.CreateData(rsp.Id.ToString()))
                            {
                                sw.Write(JsonSerializer.Serialize(data));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    rsp = CqlState.QueryResponses.Find(r => r.Id.CompareTo(rsp.Id) == 0);
                    rsp.Error = e.Message;
                }
                finally
                {
                    rsp.Busy = false;
                    CqlState.HasChanged();
                    State.Busy = false;
                }

                return Unit.Value;
            }
            public SubmitQueryHandler(IStore store, StorageService storage, E1Service e1) : base(store)
            {
                Storage = storage;
                E1 = e1;
            }
        }
        public class SelectViewUserHandler : ActionHandler<SelectViewUserAction>
        {
            CqlState CqlState => Store.GetState<CqlState>();
            AppState State => Store.GetState<AppState>();
            public override Task<Unit> Handle(SelectViewUserAction aAction, CancellationToken aCancellationToken)
            {
                State.ViewUser = aAction.ViewUser;

                CqlState.QueryHasChanged(aAction.ViewUser, CqlState.Query[aAction.ViewUser]);

                return Unit.Task;
            }
            public SelectViewUserHandler(IStore store) : base(store) { }
        }
        public class SetQueryHandler : ActionHandler<SetQueryAction>
        {
            ParserService Parse { get; }
            CqlState CqlState => Store.GetState<CqlState>();
            AppState State => Store.GetState<AppState>();
            public override Task<Unit> Handle(SetQueryAction aAction, CancellationToken aCancellationToken)
            {
                State.Query = aAction.Query;
                State.ParseError = string.Empty;
                if (!string.IsNullOrWhiteSpace(aAction.Query))
                {
                    try
                    {
                        AIS.Data.DataRequest.Parser.Before(Char(';')).ParseOrThrow(aAction.Query.TrimEnd() + ';');
                    }
                    catch (Exception e)
                    {
                        State.ParseError = e.Message;
                    }
                }

                CqlState.Query[State.User.Value] = aAction.Query;
                CqlState.QueryHasChanged(State.User.Value, aAction.Query);

                var handler = AppState.Changed;
                handler?.Invoke(State, null);

                return Unit.Task;
            }
            public SetQueryHandler(IStore store, ParserService parse) : base(store)
            {
                Parse = parse;
            }
        }
    }
}
