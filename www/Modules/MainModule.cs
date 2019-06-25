using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using www.SupportClasses;

namespace www
{
    public class MainModule : Nancy.NancyModule
    {
        public MainModule(IDatabase db)
        {
            Get("/", x => View["_views/SSVE", new
            {
                Timestamp = DateTime.Now
            }]);

            Get("/{clientGUID:guid}/razor", async (x, ct) => View["Data/_views/Index", new ClientModel
            {
                Title = "Joe",
                ClientGUID = (Guid)x.clientGUID
            }]);


            Get("/{clientGUID:guid}/razor/2", async (x, ct) => View["Data/Clients/Two/_views/Index", new ClientModel
            {
                Title = "Client #2",
                ClientGUID = (Guid)x.clientGUID
            }]);


            Get("/{clientGUID:guid}/home", x => View["Data/_views/ClientHome", new
            {
                clientGUID = (Guid)x.clientGUID
            }]);

            Post("/", x =>
                    {
                        var data = this.Bind<PostData>();

                        return View["_views/PostData", data];
                    });

            Post("/name/{name}", x =>
            {
                var data = this.Bind<PostData>();
                data.FirstName = x.name;
                return View["_views/PostData", data];
            });

            Get("/topUsers", x => db.GetTopUsers());

            Get("/topUsernames", x => db.GetTopUsernames());


            Get("/razor", async (x, ct) =>
                    {
                        return View["_views/RazorTest", new { }];
                    //var result = await razor.RenderToStringAsync("_views/RazorTest", new { });

                    //return Response.AsText(result, "text/html");
                });
        }
    }

    public class PostData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid myGuid { get; set; }
        public int MyInt { get; set; }
    }

    public class ClientModel
    {
        public string Title { get; set; }
        public Guid ClientGUID { get; set; }
    }
}
