namespace Shared.Http;

using System.Collections;
using System.Net;
using System.Text.RegularExpressions;

public class HttpRouter
{
    public const int RESPONSE_NOT_SENT = 0;
    public const int RESPONSE_SENT = 1;

    private readonly List<HttpMiddleware> _globalMiddleware = new();
    private readonly List<RouteEntry> _routes = new();
    private readonly List<HttpRouter> _subRouters = new();
    private string _basePath;

    public HttpRouter(string basePath = "")
    {
        _basePath = basePath.TrimEnd('/');
    }

    /// <summary>
    /// Adds global middleware that runs for all routes.
    /// </summary>
    public HttpRouter Use(HttpMiddleware middleware)
    {
        _globalMiddleware.Add(middleware);
        return this;
    }

    /// <summary>
    /// Mounts a sub-router at the specified path prefix.
    /// </summary>
    public HttpRouter Mount(string path, HttpRouter router)
    {
        router._basePath = $"{_basePath}{path.TrimEnd('/')}".TrimEnd('/');
        _subRouters.Add(router);
        return this;
    }

    /// <summary>
    /// Registers a route with the specified method, path, and middleware.
    /// </summary>
    public HttpRouter Route(string method, string path, params HttpMiddleware[] middleware)
    {
        // Store the relative path - the full path is computed during matching
        _routes.Add(new RouteEntry(method.ToUpperInvariant(), path, middleware));
        return this;
    }

    /// <summary>
    /// Registers a GET route.
    /// </summary>
    public HttpRouter Get(string path, params HttpMiddleware[] middleware)
    {
        return Route("GET", path, middleware);
    }

    /// <summary>
    /// Registers a POST route.
    /// </summary>
    public HttpRouter Post(string path, params HttpMiddleware[] middleware)
    {
        return Route("POST", path, middleware);
    }

    /// <summary>
    /// Registers a PUT route.
    /// </summary>
    public HttpRouter Put(string path, params HttpMiddleware[] middleware)
    {
        return Route("PUT", path, middleware);
    }

    /// <summary>
    /// Registers a DELETE route.
    /// </summary>
    public HttpRouter Delete(string path, params HttpMiddleware[] middleware)
    {
        return Route("DELETE", path, middleware);
    }

    /// <summary>
    /// Registers a PATCH route.
    /// </summary>
    public HttpRouter Patch(string path, params HttpMiddleware[] middleware)
    {
        return Route("PATCH", path, middleware);
    }

    /// <summary>
    /// Handles an incoming HTTP request.
    /// </summary>
    public async Task<int> HandleAsync(HttpListenerRequest req, HttpListenerResponse res, Hashtable? props = null)
    {
        props ??= new Hashtable();
        var path = req.Url?.AbsolutePath ?? "/";
        var method = req.HttpMethod.ToUpperInvariant();

        // Build middleware pipeline
        var middleware = new List<HttpMiddleware>(_globalMiddleware);

        // Find matching route
        RouteEntry? matchedRoute = null;
        foreach (var route in _routes)
        {
            var fullPath = $"{_basePath}{route.Path}";
            var routeParams = MatchRoute(fullPath, path, method, route.Method);
            if (routeParams != null)
            {
                matchedRoute = route;
                props["req.params"] = routeParams;
                break;
            }
        }

        // Check sub-routers if no direct route matched
        if (matchedRoute == null)
        {
            foreach (var subRouter in _subRouters)
            {
                if (path.StartsWith(subRouter._basePath, StringComparison.OrdinalIgnoreCase) ||
                    subRouter._basePath == "")
                {
                    var result = await subRouter.HandleAsync(req, res, props);
                    if (result == RESPONSE_SENT)
                    {
                        return RESPONSE_SENT;
                    }
                }
            }
        }

        if (matchedRoute != null)
        {
            middleware.AddRange(matchedRoute.Middleware);
        }

        if (middleware.Count == 0)
        {
            return RESPONSE_NOT_SENT;
        }

        // Execute middleware pipeline using immutable recursion
        await ExecuteMiddlewareAsync(middleware, 0, req, res, props);

        return matchedRoute != null ? RESPONSE_SENT : RESPONSE_NOT_SENT;
    }

    /// <summary>
    /// Executes middleware at the specified index and creates next function for subsequent middleware.
    /// Uses immutable recursion pattern to avoid race conditions.
    /// </summary>
    private static async Task ExecuteMiddlewareAsync(
        IReadOnlyList<HttpMiddleware> middleware,
        int index,
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props)
    {
        if (index >= middleware.Count)
        {
            return;
        }

        var current = middleware[index];
        await current(req, res, props, () => ExecuteMiddlewareAsync(middleware, index + 1, req, res, props));
    }

    /// <summary>
    /// Matches a route pattern against a path, supporting parametrized paths.
    /// </summary>
    private static Hashtable? MatchRoute(string pattern, string path, string requestMethod, string routeMethod)
    {
        if (!string.Equals(requestMethod, routeMethod, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Convert route pattern to regex
        // e.g., "/users/:id" becomes "^/users/(?<id>[^/]+)$"
        var regexPattern = "^" + Regex.Replace(pattern, @":(\w+)", @"(?<$1>[^/]+)") + "$";
        var match = Regex.Match(path, regexPattern, RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return null;
        }

        var routeParams = new Hashtable();
        var regex = new Regex(@":(\w+)");
        var paramMatches = regex.Matches(pattern);

        foreach (Match paramMatch in paramMatches)
        {
            var paramName = paramMatch.Groups[1].Value;
            routeParams[paramName] = match.Groups[paramName].Value;
        }

        return routeParams;
    }

    private class RouteEntry
    {
        public string Method { get; }
        public string Path { get; }
        public HttpMiddleware[] Middleware { get; }

        public RouteEntry(string method, string path, HttpMiddleware[] middleware)
        {
            Method = method;
            Path = path;
            Middleware = middleware;
        }
    }
}
