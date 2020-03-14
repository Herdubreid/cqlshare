using Pidgin;
using System.Text.Json;

namespace Celin
{
    public class ParserService
    {
        public AIS.DatabrowserRequest Parse(string qry) =>
            AIS.Data.DataRequest.Parser.Before(Parser.Char(';')).ParseOrThrow(qry.TrimEnd() + ';');
        public string ToString(string qry)
        {
            try
            {
                var request = Parse(qry);
                return JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = true
                });
            }
            catch (ParseException e)
            {
                return e.Message;
            }
        }
    }
}
