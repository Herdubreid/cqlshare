using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Celin
{
    public class E1Service : AIS.Server
    {
        public E1Service(IConfiguration config, ILogger<E1Service> logger, IHttpClientFactory httpFactory)
            : base(config["e1Url"], logger, httpFactory.CreateClient())
        {
            AuthRequest.username = config["e1Username"];
            AuthRequest.password = config["e1Password"];
        }
    }
}
