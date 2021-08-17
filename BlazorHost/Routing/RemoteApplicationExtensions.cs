using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorHost.Routing
{
    public interface IService
    {
        // empty interface used to decorate module services for auto registration
    }

    public static class RemoteApplicationExtensions
    {
        public static IServiceCollection AddAssemblyReferences(this IServiceCollection services)
        {
            foreach (Assembly assembly in AssemblyHandler.RemoteAssemblies)
            {
                var implementationTypes = assembly.GetTypes().Where(x => 
                    x.IsPublic &&
                    !x.IsSubclassOf(typeof(PageModel)) &&
                    !x.IsSubclassOf(typeof(ComponentBase)) && 
                    !x.Name.StartsWith(nameof(Program)) && !x.Name.StartsWith(nameof(Startup)) &&!x.Name.StartsWith(nameof(_Imports)) 
                );
                foreach (var implementationType in implementationTypes)
                {
                    var serviceType = Type.GetType(implementationType.AssemblyQualifiedName.Replace(implementationType.Name, $"I{implementationType.Name}"));
                    services.AddScoped(serviceType ?? implementationType, implementationType);
                }
            }
            return services;
        }
    }
}
