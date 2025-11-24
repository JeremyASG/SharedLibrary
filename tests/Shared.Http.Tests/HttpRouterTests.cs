namespace Shared.Http.Tests;

using System.Collections;
using System.Net;

public class HttpRouterTests
{
    [Fact]
    public async Task Route_MatchesExactPath()
    {
        // Arrange
        var router = new HttpRouter();
        var middlewareCalled = false;
        
        router.Get("/test", async (req, res, props, next) =>
        {
            middlewareCalled = true;
            await next();
        });

        var props = new Hashtable();

        // Act - test internal matching logic by calling with a simple context
        var result = await InvokeRouterWithPath(router, "GET", "/test", props);

        // Assert
        Assert.True(middlewareCalled);
        Assert.Equal(HttpRouter.RESPONSE_SENT, result);
    }

    [Fact]
    public async Task Route_MatchesParametrizedPath()
    {
        // Arrange
        var router = new HttpRouter();
        string? capturedId = null;
        
        router.Get("/users/:id", async (req, res, props, next) =>
        {
            var routeParams = props["req.params"] as Hashtable;
            capturedId = routeParams?["id"]?.ToString();
            await next();
        });

        var props = new Hashtable();

        // Act
        var result = await InvokeRouterWithPath(router, "GET", "/users/123", props);

        // Assert
        Assert.Equal("123", capturedId);
        Assert.Equal(HttpRouter.RESPONSE_SENT, result);
    }

    [Fact]
    public async Task Route_MatchesMultipleParameters()
    {
        // Arrange
        var router = new HttpRouter();
        string? capturedUserId = null;
        string? capturedPostId = null;
        
        router.Get("/users/:userId/posts/:postId", async (req, res, props, next) =>
        {
            var routeParams = props["req.params"] as Hashtable;
            capturedUserId = routeParams?["userId"]?.ToString();
            capturedPostId = routeParams?["postId"]?.ToString();
            await next();
        });

        var props = new Hashtable();

        // Act
        var result = await InvokeRouterWithPath(router, "GET", "/users/42/posts/99", props);

        // Assert
        Assert.Equal("42", capturedUserId);
        Assert.Equal("99", capturedPostId);
        Assert.Equal(HttpRouter.RESPONSE_SENT, result);
    }

    [Fact]
    public async Task GlobalMiddleware_RunsForAllRoutes()
    {
        // Arrange
        var router = new HttpRouter();
        var globalMiddlewareCalled = false;
        var routeMiddlewareCalled = false;
        
        router.Use(async (req, res, props, next) =>
        {
            globalMiddlewareCalled = true;
            await next();
        });
        
        router.Get("/test", async (req, res, props, next) =>
        {
            routeMiddlewareCalled = true;
            await next();
        });

        var props = new Hashtable();

        // Act
        await InvokeRouterWithPath(router, "GET", "/test", props);

        // Assert
        Assert.True(globalMiddlewareCalled);
        Assert.True(routeMiddlewareCalled);
    }

    [Fact]
    public async Task MiddlewarePipeline_ExecutesInOrder()
    {
        // Arrange
        var router = new HttpRouter();
        var order = new List<int>();
        
        router.Use(async (req, res, props, next) =>
        {
            order.Add(1);
            await next();
            order.Add(4);
        });
        
        router.Get("/test", async (req, res, props, next) =>
        {
            order.Add(2);
            await next();
            order.Add(3);
        });

        var props = new Hashtable();

        // Act
        await InvokeRouterWithPath(router, "GET", "/test", props);

        // Assert
        Assert.Equal(new[] { 1, 2, 3, 4 }, order);
    }

    [Fact]
    public async Task SubRouter_HandlesNestedRoutes()
    {
        // Arrange
        var mainRouter = new HttpRouter();
        var apiRouter = new HttpRouter();
        var routeCalled = false;
        
        apiRouter.Get("/users", async (req, res, props, next) =>
        {
            routeCalled = true;
            await next();
        });
        
        mainRouter.Mount("/api", apiRouter);

        var props = new Hashtable();

        // Act
        await InvokeRouterWithPath(mainRouter, "GET", "/api/users", props);

        // Assert
        Assert.True(routeCalled);
    }

    [Fact]
    public async Task Route_ReturnsNotSent_WhenNoMatch()
    {
        // Arrange
        var router = new HttpRouter();
        
        router.Get("/existing", async (req, res, props, next) =>
        {
            await next();
        });

        var props = new Hashtable();

        // Act
        var result = await InvokeRouterWithPath(router, "GET", "/nonexistent", props);

        // Assert
        Assert.Equal(HttpRouter.RESPONSE_NOT_SENT, result);
    }

    [Fact]
    public async Task Route_MatchesCorrectHttpMethod()
    {
        // Arrange
        var router = new HttpRouter();
        var getCalled = false;
        var postCalled = false;
        
        router.Get("/test", async (req, res, props, next) =>
        {
            getCalled = true;
            await next();
        });
        
        router.Post("/test", async (req, res, props, next) =>
        {
            postCalled = true;
            await next();
        });

        var props = new Hashtable();

        // Act
        await InvokeRouterWithPath(router, "POST", "/test", props);

        // Assert
        Assert.False(getCalled);
        Assert.True(postCalled);
    }

    [Fact]
    public async Task Props_PersistsDataBetweenMiddleware()
    {
        // Arrange
        var router = new HttpRouter();
        string? capturedValue = null;
        
        router.Use(async (req, res, props, next) =>
        {
            props["testKey"] = "testValue";
            await next();
        });
        
        router.Get("/test", async (req, res, props, next) =>
        {
            capturedValue = props["testKey"]?.ToString();
            await next();
        });

        var props = new Hashtable();

        // Act
        await InvokeRouterWithPath(router, "GET", "/test", props);

        // Assert
        Assert.Equal("testValue", capturedValue);
    }

    /// <summary>
    /// Helper method to invoke router with a simulated HTTP context.
    /// Uses an actual HttpListener to generate real request/response objects.
    /// </summary>
    private static async Task<int> InvokeRouterWithPath(HttpRouter router, string method, string path, Hashtable props)
    {
        // Create a simple test context using an actual HttpListener
        var port = GetAvailablePort();
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        int result = HttpRouter.RESPONSE_NOT_SENT;
        
        try
        {
            // Start the request handling task
            var handleTask = Task.Run(async () =>
            {
                var context = await listener.GetContextAsync();
                result = await router.HandleAsync(context.Request, context.Response, props);
                context.Response.Close();
            });

            // Send the request
            using var client = new HttpClient();
            var request = new HttpRequestMessage(new HttpMethod(method), $"http://localhost:{port}{path}");
            await client.SendAsync(request);

            // Wait for the handler to complete
            await handleTask;
        }
        finally
        {
            listener.Stop();
            listener.Close();
        }

        return result;
    }

    private static int GetAvailablePort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
