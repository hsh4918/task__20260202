using System.Text;

namespace emergency_contact_system.Middleware;

public sealed class LoggingMiddleware(
    RequestDelegate next,
    ILogger<LoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requestBody = await ReadRequestBodyAsync(context.Request);

        var originalBody = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);
        }
        finally
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            await responseBody.CopyToAsync(originalBody, context.RequestAborted);
            context.Response.Body = originalBody;

            using (logger.BeginScope(new Dictionary<string, object?>
            {
                ["RequestBody"] = requestBody,
                ["ResponseBody"] = responseText
            }))
            {
                logger.LogDebug(
                    "Request {Method} {Path} responded {StatusCode} | RequestBody: {RequestBody} | ResponseBody: {ResponseBody}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    requestBody,
                    responseText);
            }
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
        {
            return string.Empty;
        }

        request.EnableBuffering();
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

}
