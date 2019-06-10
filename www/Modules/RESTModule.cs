using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using www.SupportClasses;

namespace www
{
    public class RESTModule : Nancy.NancyModule
    {
        public RESTModule(IDatabase db) : base("/rest/{clientGUID:guid}")
        {
            Get("/", x => db.GetUsers(x.clientGUID));

            Post("/", x =>
            {
                var data = this.Bind<UserUpdate>();

                var resp = db.UpdateUser(x.clientGUID, data.UserID, data.NewLastName);

                return resp ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            });
        }

        public class UserUpdate
        {
            public int UserID { get; set; }
            public string NewLastName { get; set; }
        }
    }
}
