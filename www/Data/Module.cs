using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nancy;

namespace www.Data
{
    public class Module : NancyModule
    {
        public Module(IRootPathProvider root) : base("/data")
        {
            Get("/{clientGUID:guid}/home", x =>
            {
                Guid clientGUID = x.clientGUID;
                var clientFile = Directory.EnumerateFiles(root.GetRootPath(), clientGUID.ToString(), SearchOption.AllDirectories).First();
                var clientTitle = File.ReadAllText(clientFile);

                return View["Data/_views/Index", new ClientModel
                {
                    Title = clientTitle,
                    ClientGUID = clientGUID
                }];
            });
        }
    }
}
