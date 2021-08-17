using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorHost.Routing
{
    public class CustomizedRouter : IComponent, IHandleAfterRender, IDisposable
    {
        RenderHandle _renderHandle;
        bool _navigationInterceptionEnabled;
        string _location;

        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private INavigationInterception NavigationInterception { get; set; }
        [Inject] RouteManager RouteManager { get; set; }

        [Parameter] public RenderFragment NotFound { get; set; }
        [Parameter] public RenderFragment<RouteData> Found { get; set; }

        [Parameter] public Assembly AppAssembly { get; set; }
        [Parameter] public List<Assembly> AdditionalAssemblies { get; set; }
        [Parameter] public bool PreferExaxtMatches { get; set; }

        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
            _location = NavigationManager.Uri;
            NavigationManager.LocationChanged += HandleLocationChanged;
        }

        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);

            if (Found == null)
            {
                throw new InvalidOperationException($"The {nameof(CustomizedRouter)} component requires a value for the parameter {nameof(Found)}.");
            }

            if (NotFound == null)
            {
                throw new InvalidOperationException($"The {nameof(CustomizedRouter)} component requires a value for the parameter {nameof(NotFound)}.");
            }
            Refresh();
            return Task.CompletedTask;
        }

        public Task OnAfterRenderAsync()
        {
            if (!_navigationInterceptionEnabled)
            {
                _navigationInterceptionEnabled = true;
                return NavigationInterception.EnableNavigationInterceptionAsync();
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= HandleLocationChanged;
        }

        private void HandleLocationChanged(object sender, LocationChangedEventArgs args)
        {
            _location = args.Location;
            Refresh();
        }

        private void Refresh()
        {
            var relativeUri = NavigationManager.ToBaseRelativePath(_location);
            var parameters = ParseQueryString(relativeUri);

            if (relativeUri.IndexOf('?') > -1)
            {
                relativeUri = relativeUri.Substring(0, relativeUri.IndexOf('?'));
            }

            var segments = relativeUri.Trim().Split('/', StringSplitOptions.RemoveEmptyEntries);
            var matchResult = RouteManager.Match(segments);

            if (matchResult.IsMatch)
            {
                var routeData = new RouteData(
                    matchResult.MatchedRoute.Handler,
                    parameters);

                _renderHandle.Render(Found(routeData));
            }
            else
            {
                _renderHandle.Render(NotFound);
            }
        }

        private Dictionary<string, object> ParseQueryString(string uri)
        {
            var queryString = new Dictionary<string, object>();
            var keyValuePairs = uri.Substring(uri.IndexOf("?") + 1).Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string keyValuePair in keyValuePairs)
            {
                if (keyValuePair != "" && keyValuePair.Contains("="))
                {
                    var keyAndValue = keyValuePair.Split('=');
                    queryString.Add(keyAndValue[0], keyAndValue[1]);
                }
            }
            return queryString;
        }
    }
}