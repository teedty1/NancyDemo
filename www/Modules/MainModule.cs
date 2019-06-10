using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            Get("/{clientGUID:guid}/home", x => View["_views/ClientHome", new
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
        }
    }

    public class PostData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid myGuid { get; set; }
        public int MyInt { get; set; }
    }
}
