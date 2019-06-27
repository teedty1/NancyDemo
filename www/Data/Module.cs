using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;

namespace www.Data
{
    public class Module : NancyModule
    {
        public Module() : base("/data")
        {
            Get("/{clientGUID:guid}/home", async (x, ct) => View["Data/_views/Index", new ClientModel
            {
                Title = "Joe",
                ClientGUID = (Guid)x.clientGUID
            }]);
        }
    }
}
