using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace www.SupportClasses
{
    public class VueFilesToJS
    {
        private Dictionary<string, Component> Components = new Dictionary<string, Component>();
        private string WsRoot;
        private bool SquashWS;

        public static string Compile(string wsRootPath, string vueFile, bool squashWS = true)
        {
            if (wsRootPath.EndsWith(@"\"))
                wsRootPath = wsRootPath.Substring(0, wsRootPath.Length - 1);
            var inst = new VueFilesToJS() { WsRoot = wsRootPath, SquashWS = squashWS };
            return inst.ProcRoot(vueFile);
        }

        private VueFilesToJS()
        {
        }

        private string ProcRoot(string vueFile)
        {
            var res = ParseVueFile(vueFile, null);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("function() {");

            foreach (var c in Components.Values)
                c.Render(sb);

            sb.AppendLine("  return {\n  template: " + JSStringEncode(res.Template) + ",\n" + res.Script.Substring(1).Trim() + ";");
            sb.AppendLine("}");

            return sb.ToString();
        }

        // wsroot = "c:\web-sites\tjek" (no ending \)
        private ParseVueFileResult ParseVueFile(string vueFile, Component fromComp)
        {
            var cp = ResolvePath(vueFile);
            var vfWin = WsRoot + cp.Replace("/", @"\") + JustFN(vueFile);
            var x = System.IO.File.ReadAllText(vfWin).Trim();

            if (!x.EndsWith("</script>"))
                throw new Exception(".vue file '" + vueFile + "' does not end with </script>");
            x = x.Substring(0, x.Length - 9).Trim();
            var i = x.LastIndexOf("<script");
            if (i < 0)
                throw new Exception("Start <script> tag not found in .vue file '" + vueFile + "'");
            var y = x.Substring(i + 7).Trim();
            x = x.Substring(0, i).Trim();

            // REM get script
            string Scrpt = null;
            string sfWin = null;
            if (y.StartsWith(">"))
            {
                Scrpt = y.Substring(1).Trim();
                sfWin = vfWin;
            }
            else
            {
                if (!y.StartsWith("src=\""))
                    throw new Exception("Invalid <script> tag in .vue file '" + vueFile + "' - must be <script> or <script src=\"...\">");
                y = y.Substring(5);
                i = y.IndexOf('"');
                if (i < 0)
                    throw new Exception("Invalid <script> tag in .vue file '" + vueFile + "' - must be <script> or <script src=\"...\">");
                var sfWeb = y.Substring(0, i).Trim();
                var sfPath = ResolvePath(sfWeb, cp);
                sfWin = WsRoot + sfPath.Replace("/", @"\") + JustFN(sfWeb);
                y = y.Substring(i + 1);
                if (y != ">")
                    throw new Exception("Invalid <script> tag in .vue file '" + vueFile + "' - must be <script> or <script src=\"...\">");
                Scrpt = System.IO.File.ReadAllText(sfWin).Trim();
            }

            // REM validate script
            // import Home from './components/Home.vue';
            while (Scrpt.StartsWith("import ", StringComparison.InvariantCultureIgnoreCase))
            {
                i = Scrpt.IndexOf(";");
                var z = Scrpt.Substring(7, i - 7).Trim();
                Scrpt = Scrpt.Substring(i + 1).Trim();
                i = z.IndexOf(" ");
                var CompName = z.Substring(0, i);
                z = z.Substring(i + 1).Trim();
                if (!z.StartsWith("from "))
                    throw new Exception("Invalid import statement in file '" + sfWin + "'");
                z = z.Substring(5).Trim();
                if (!((z.StartsWith("'") & z.EndsWith("'")) | (z.StartsWith("\"") & z.EndsWith("\""))))
                    throw new Exception("Invalid import statement in file '" + sfWin + "'");
                z = z.Substring(1, z.Length - 2);
                ProcComponent(CompName, ResolvePath(z, cp) + JustFN(z), fromComp);
            }

            if (!Scrpt.StartsWith("export "))
                throw new Exception("Script (after any import statements) does not start with 'export default {' in file '" + sfWin + "'");
            Scrpt = Scrpt.Substring(7).Trim();
            if (!Scrpt.StartsWith("default"))
                throw new Exception("Script (after any import statements) does not start with 'export default {' in file '" + sfWin + "'");
            Scrpt = Scrpt.Substring(7).Trim();
            if (!Scrpt.StartsWith("{"))
                throw new Exception("Script (after any import statements) does not start with 'export default {' in file '" + sfWin + "'");
            if (Scrpt.EndsWith(";"))
                Scrpt = Scrpt.Substring(0, Scrpt.Length - 1).Trim();

            // REM validate remaining = template
            if (!x.StartsWith("<template>"))
                throw new Exception(".vue file '" + vueFile + "' does not start with <template>");
            x = x.Substring(10);
            if (!x.EndsWith("</template>"))
                throw new Exception(".vue file '" + vueFile + "' does not end with </template> (after script is extracted)");
            x = x.Substring(0, x.Length - 11).Trim();

            if (SquashWS)
                x = SquashWhiteSpace(x);

            return new ParseVueFileResult() { Template = x, Script = Scrpt };
        }

        private static char[] WhiteSpace = new[] { '\t', '\n' };
        private string SquashWhiteSpace(string x)
        {
            var sb = new System.Text.StringBuilder();
            var p = 0;
            int i;
            do
            {
                i = x.IndexOfAny(WhiteSpace, p);
                if (i < 0)
                {
                    sb.Append(x.Substring(p)); return sb.ToString();
                }

                sb.Append(x.Substring(p, i - p));
                sb.Append(" ");
                p = i + 1;
                while (p < x.Length && WhiteSpace.Contains(x[p]))
                    p += 1;
            }
            while (true);
        }

        // REM cp='/',  '/kurt',  '/kurt/viggo'...
        private string ResolvePath(string fn, string cp = "/")
        {
            if (fn.Contains(@"\"))
                throw new Exception(@"invalid \ character in file name");
            if (fn.StartsWith("/"))
            {
                fn = fn.Substring(1); cp = "/";
            }
            int i;
            var firstRound = true;
            while (true)
            {
                if (fn.StartsWith("../"))
                {
                    fn = fn.Substring(3); cp = cp.Substring(0, cp.LastIndexOf("/")); continue;
                }
                if (fn.StartsWith("./"))
                {
                    fn = fn.Substring(2); continue;
                }
                i = fn.IndexOf("/");
                if (i < 0)
                    break;
                if (i == 0)
                    throw new Exception("invalid / in path");
                cp += fn.Substring(0, i + 1);
                fn = fn.Substring(i + 1);
            }
            return cp;
        }

        private string JustFN(string fn)
        {
            var i = fn.LastIndexOf("/");
            if (i < 0)
                return fn;
            return fn.Substring(i + 1);
        }

        private static string JSStringEncode(string x)
        {
            return "'" + (x.Replace(@"\", @"\\"));//.Replace(Constants.vbCrLf, @"\n").Replace(Constants.vbCr, @"\n").Replace(Constants.vbLf, @"\n").Replace(Constants.vbTab, @"\t").Replace("'", @"\'")) + "'";
        }

        private struct ParseVueFileResult
        {
            public string Template;
            public string Script;
        }

        private void ProcComponent(string compName, string vueFile, Component parentComp)
        {
            Component c = null;
            if (Components.TryGetValue(compName, out c))
            {
                if (string.Compare(vueFile, c.File, true) != 0)
                    throw new Exception("Two file components ('" + vueFile + "' / '" + c.File + "') referenced with same import name (" + compName + ")");
                if (parentComp != null)
                    parentComp.SubComps.Add(c);
                return;
            }
            foreach (var c2 in Components.Values)
            {
                if (string.Compare(vueFile, c2.File, true) == 0)
                {
                    var comp = new Component() { Name = compName, File = vueFile, AliasFor = c2 };
                    if (parentComp != null)
                        parentComp.SubComps.Add(comp);
                    Components.Add(compName, comp);
                    return;
                }
            }
            c = new Component() { Name = compName, File = vueFile };
            if (parentComp != null)
                parentComp.SubComps.Add(c);
            Components.Add(compName, c);
            c.pvfr = ParseVueFile(vueFile, c);
        }

        private class Component
        {
            public string Name;
            public string File;
            public Component AliasFor = null;
            public ParseVueFileResult pvfr;
            public List<Component> SubComps = new List<Component>();
            public bool Rendered = false;

            public void Render(System.Text.StringBuilder toSB)
            {
                if (Rendered)
                    return;
                if (AliasFor != null)
                {
                    AliasFor.Render(toSB);
                    toSB.AppendLine("  var " + Name + "=" + AliasFor.Name + ";");
                }
                else
                {
                    foreach (var c in SubComps)
                        c.Render(toSB);
                    toSB.AppendLine("  var " + Name + "={template:" + JSStringEncode(pvfr.Template) + ",");
                    toSB.AppendLine(pvfr.Script.Substring(1).Trim() + ";");
                }
                Rendered = true;
            }
        }
    }

}
