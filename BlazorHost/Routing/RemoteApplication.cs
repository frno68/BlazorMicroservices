using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BlazorHost.Routing
{
    public class RemoteApplication
    {
        RemoteApplicationInfo _remoteApplicationInfo { get; set; }
        public RemoteApplication(RemoteApplicationInfo remoteApplicationInfo)
        {
            _remoteApplicationInfo = remoteApplicationInfo;
            Assembly = Assembly.LoadFrom(remoteApplicationInfo.assemblyPath);
        }
        public Assembly Assembly { get; }
        public List<Type> Menus { 
            get {
                return Assembly.GetTypes().Where(m => m.Name.Equals("Menu")).ToList();
            }
        }
        public List<Type> NavLinks
        {
            get
            {
                return Assembly.GetTypes().Where(n => n.Name.Equals("Menu")).ToList();
            }
        }
    }
}
