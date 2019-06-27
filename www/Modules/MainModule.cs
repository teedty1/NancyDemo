using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using www.SupportClasses;

namespace www
{
    public class MainModule : NancyModule
    {
        public MainModule(IDatabase db)
        {
            Get("/", x => View["_views/Index", new
            {
                Timestamp = DateTime.Now
            }]);

            Post("/nancyFormPost", x =>
            {
                var data = this.Bind<PostData>();
                data.Guid = Guid.NewGuid();
                data.Timestamp = DateTime.Now;

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
        public Guid Guid { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ClientModel
    {
        public string Title { get; set; }
        public Guid ClientGUID { get; set; }
    }
}
