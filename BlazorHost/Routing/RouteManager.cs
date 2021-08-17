using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlazorHost.Routing
{
    public class RouteManager
    {
        public RouteManager()
        {
            var routesList = new List<Route>();
            AddPageRoutes(Assembly.GetExecutingAssembly(), routesList);
            foreach (var assembly in AssemblyHandler.RemoteAssemblies)
            {
                AddPageRoutes(assembly, routesList);
                AddAssemblyRoutes(assembly, routesList);
            }
            Routes = routesList.ToArray();
        }
        public Route[] Routes { get; private set; }
        public MatchResult Match(string[] segments)
        {
            if (segments.Length == 0)
            {
                var indexRoute = Routes.SingleOrDefault(x => x.UriSegments.Length == 0 );
                return MatchResult.Match(indexRoute);
            }
            foreach (var route in Routes)
            {
                var matchResult = route.Match(segments);
                if (matchResult.IsMatch)
                {
                    return matchResult;
                }
            }
            return MatchResult.NoMatch();
        }
        private void AddPageRoutes(Assembly assembly, List<Route> routesList)
        {
            var pageComponentTypes = getPageComponentTypes(assembly);
            foreach (var pageType in pageComponentTypes)
            {
                var routeAttributes = pageType.GetCustomAttributes(inherit: true).OfType<RouteAttribute>().ToList();
                foreach (var routeAttribute in routeAttributes)
                {
                    var newRoute = new Route
                    {
                        UriSegments = routeAttribute.Template.Split('/', StringSplitOptions.RemoveEmptyEntries),
                        Handler = pageType
                    };
                    if (!RouteExists(routesList, newRoute))
                    {
                        routesList.Add(newRoute);
                    }
                }
            }
        }
        private void AddAssemblyRoutes(Assembly assembly, List<Route> routesList)
        {
            var pageComponentTypes = getPageComponentTypes(assembly);
            foreach (var pageType in pageComponentTypes)
            {
                var newRoute = new Route
                {
                    UriSegments = pageType.FullName.Replace("Pages.", "").Replace(".Index", "").Split('.'),
                    Handler = pageType
                };
                routesList.Add(newRoute);
            }
        }
        private IEnumerable<Type> getPageComponentTypes(Assembly assembly)
        {
            return assembly.ExportedTypes.Where(t => t.IsSubclassOf(typeof(ComponentBase)) && t.Namespace.Contains(".Pages"));
        }
        private bool RouteExists(List<Route> routesList, Route route2)
        {
            foreach (Route route1 in routesList)
            {
                if ((route1.UriSegments.Length == 0) && (route2.UriSegments.Length == 0)) return true;
                if (string.Join(".", route1.UriSegments) == string.Join(".", route2.UriSegments)) return true;
            }
            return false;
        }
    }
}
