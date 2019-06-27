using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using www.SupportClasses;

namespace www.Modules
{
    public class SwaggerDemoModule : SwaggerNancyModule
    {
        public SwaggerDemoModule() : base("/swagger")
        {
            Get("/", x => "Welcome to Swagger");
        }
    }
}
