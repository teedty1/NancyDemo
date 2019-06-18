using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BundlerMinifier;
using Nancy;
using NUglify;
using NUglify.Css;
using NUglify.JavaScript;

namespace www.Modules
{
    public class ResourcesModule : NancyModule
    {
        public ResourcesModule(IRootPathProvider root) : base("/{clientGUID:guid}")
        {
            var clientGUID = Guid.Empty;
            this.Before.AddItemToStartOfPipeline(context =>
            {
                //All the routes have "
                clientGUID = (Guid)context.Parameters["clientGUID"];

                return null;
            });

            Get("/bundle.css", x =>
            {
                var b = buildBundle(root, clientGUID, BundleType.CSS);
                return Response.AsText(b, "text/css");
            });


            Get("/bundle.js", x =>
            {
                var b = buildBundle(root, clientGUID, BundleType.JS);
                return Response.AsText(b, "application/javascript");
            });


            Get("/views/{name*}", x =>
            {
                string viewName = x.name;

                var path = getViewPath(root, clientGUID, viewName, out string eTag);

                if (path == "")
                    return HttpStatusCode.NotFound;

                return View[path]
                    .WithHeader("View", path)
                    .WithHeader("Cache-Control", "max-age=3600, public")
                    .WithHeader("ETag", eTag);
            });
        }

        private string buildBundle(IRootPathProvider root, Guid clientGUID, BundleType type)
        {
            string fileSystemBasePath = root.GetRootPath();
            var path = "";

            //Find the clientGUID file
            //There can only be one root
            var clientFSRoot = Directory.EnumerateFiles(fileSystemBasePath, clientGUID.ToString(), SearchOption.AllDirectories).First();

            //Get the root for the client in the application itself
            var clientAppRoot = clientFSRoot.Replace(fileSystemBasePath, "").Replace("\\", "/");

            var splitPath = clientAppRoot.Split('/');

            var typeDir = "_css";
            if (type == BundleType.JS)
                typeDir = "_js";

            var files = new List<string>();

            foreach (var p in splitPath)
            {
                path = path + p + "/";

                var dir = new DirectoryInfo(root.GetRootPath().Replace("\\", "/") + path + typeDir);
                if (!dir.Exists)
                    continue;

                if (type == BundleType.CSS)
                {
                    files.AddRange(dir.EnumerateFiles("*.less", SearchOption.AllDirectories).Select(x => x.FullName));
                    files.AddRange(dir.EnumerateFiles("*.css", SearchOption.AllDirectories).Select(x => x.FullName));
                }
                if (type == BundleType.JS)
                {
                    files.AddRange(dir.EnumerateFiles("*.js", SearchOption.AllDirectories).Select(x => x.FullName));
                }
            }

            var sb = new StringBuilder();
            foreach (var f in files)
                sb.AppendLine(File.ReadAllText(f));

            string ret = null;
            if (type == BundleType.CSS)
            {
                var minified = Uglify.Css(sb.ToString(), new CssSettings
                {
                    CommentMode = CssComment.None,
                    OutputMode = OutputMode.MultipleLines,
                });

                ret = minified.Code;
            }

            if (type == BundleType.JS)
            {
                var minified = Uglify.Js(sb.ToString(), new CodeSettings
                {
                    OutputMode = OutputMode.MultipleLines,
                });

                ret = minified.Code;
            }

            return ret?
                .Replace("@ClientGUID", clientGUID.ToString());
        }

        private string getViewPath(IRootPathProvider root, Guid clientGUID, string viewName, out string etag)
        {
            string fileSystemBasePath = root.GetRootPath();
            bool found = false;
            string path = "";
            etag = "";

            //Find the clientGUID file
            //There can only be one root
            var clientFSRoot = Directory.EnumerateFiles(fileSystemBasePath, clientGUID.ToString(), SearchOption.AllDirectories).First();

            var basePath = clientFSRoot;

            //start at the bottom (client level) and work your way up to the top of the tree looking for the view.
            try
            {
                var maxLoop = 50;
                var loopCount = 0;
                while (!found && ++loopCount < maxLoop)
                {
                    path = Path.Combine(basePath, "_views", viewName);

                    found = File.Exists(path);
                    if (!found)
                        basePath = Directory.GetParent(basePath).FullName;
                }

                etag = "\"" + DateTime.UtcNow.Ticks + "\"";
            }
            catch (Exception)
            { return ""; }

            //Remove the File System path and just return the relative path
            return path.Replace(fileSystemBasePath, "").Replace("\\", "/").TrimStart('/');
        }

        private enum BundleType { JS, CSS }
    }
}
