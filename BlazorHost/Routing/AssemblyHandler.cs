using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorHost.Routing
{
    public static class AssemblyHandler
    {
        static List<RemoteApplicationInfo> RemoteApplicationInfos { get; } = new List<RemoteApplicationInfo>() {
            new RemoteApplicationInfo() {
                assemblyPath="C:\\_Repos\\BlazorMicroservices\\BlazorApp1\\bin\\Debug\\net5.0\\publish\\BlazorApp1.dll",
                cssPath="C:\\_Repos\\BlazorMicroservices\\BlazorApp1\\bin\\Debug\\net5.0\\publish\\wwwroot\\BlazorApp1.styles.css"
            },
            new RemoteApplicationInfo() {
                assemblyPath="C:\\_Repos\\BlazorMicroservices\\BlazorApp2\\bin\\Debug\\net5.0\\publish\\BlazorApp2.dll",
                cssPath="C:\\_Repos\\BlazorMicroservices\\BlazorApp2\\bin\\Debug\\net5.0\\publish\\wwwroot\\BlazorApp2.styles.css"
            }
        };
        //Här lägger vi allt nedladdat CSS Innehåll
        static string IsolatedCssFilePath = $"{System.IO.Directory.GetCurrentDirectory()}\\wwwroot\\css\\RemoteApplication.css";

        static List<RemoteApplication> _remoteApplications;
        public static List<RemoteApplication> RemoteApplications
        {
            get
            {
                if (_remoteApplications == null)
                {
                    ClearIsolatedCssFile();
                    _remoteApplications = new List<RemoteApplication>();
                    foreach(var remoteApplicationInfo in RemoteApplicationInfos)
                    {
                        _remoteApplications.Add(
                            new RemoteApplication(remoteApplicationInfo)
                        );
                        AppendRemoteCSSToIsolatedCssFile(remoteApplicationInfo);
                    }
                }
                return _remoteApplications;
            }
        }
        public static List<Assembly> RemoteAssemblies {
            get 
            {
                return RemoteApplications.Select(x => x.Assembly).ToList();
            }
        }
        static void ClearIsolatedCssFile()
        {
            var fileInfo = new FileInfo(IsolatedCssFilePath);
            fileInfo.Create().Close();
        }
        static void AppendRemoteCSSToIsolatedCssFile(RemoteApplicationInfo _remoteApplicationInfo)
        {
            var remoteFileInfo = new FileInfo(_remoteApplicationInfo.cssPath);
            if (!remoteFileInfo.Exists) return;
            var fileInfo = new FileInfo(IsolatedCssFilePath);
            if (!fileInfo.Exists) return;

            using (StreamWriter sw = fileInfo.AppendText())
            {
                using (StreamReader sr = remoteFileInfo.OpenText())
                {
                    string cssContent = "";
                    while ((cssContent = sr.ReadLine()) != null)
                    {
                        sw.WriteLine(cssContent);
                    }
                }
                sw.Flush();
                sw.Close();
            }
        }

    }
}
