using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using Nancy.ViewEngines;

namespace www.SupportClasses
{
    public class NancyRazorViewEngine : IViewEngine
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public NancyRazorViewEngine(IServiceProvider dotNetServices)
        {
            var scope = dotNetServices.CreateScope();
            _razorViewEngine = scope.ServiceProvider.GetService<IRazorViewEngine>();
            _tempDataProvider = scope.ServiceProvider.GetService<ITempDataProvider>();
            _serviceProvider = scope.ServiceProvider;
        }

        public void Initialize(ViewEngineStartupContext viewEngineStartupContext)
        {
        }

        public Response RenderView(ViewLocationResult viewLocationResult, dynamic model, IRenderContext renderContext)
        {
            var viewName = viewLocationResult.Location + "/" + viewLocationResult.Name;

            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider,
                User = renderContext?.Context?.CurrentUser
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).Wait();
                var response = new HtmlResponse
                {
                    Contents = stream =>
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(sw.ToString());
                            writer.Flush();
                        }
                    }
                };
                return response;
            }
        }

        public IEnumerable<string> Extensions => new[] { "cshtml" };
    }

    public class NancyViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            yield return "/{0}.cshtml";
        }
    }
}
