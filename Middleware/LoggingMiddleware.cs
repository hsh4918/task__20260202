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
        // Always enable buffering and read the raw request body as text (may include multipart boundaries and binary data)
        request.EnableBuffering();

        // rewind in case upstream has read
        try
        {
            request.Body.Position = 0;
        }
        catch
        {
            // ignore if not seekable
        }

        using var ms = new MemoryStream();
        await request.Body.CopyToAsync(ms);
        var bytes = ms.ToArray();

        // rewind so downstream can read
        try
        {
            request.Body.Position = 0;
        }
        catch
        {
            // ignore
        }

        if (bytes.Length == 0) return string.Empty;

        // Try decode as UTF8; binary data may produce replacement chars
        var bodyText = Encoding.UTF8.GetString(bytes);

        const int maxLength = 4096;
        if (bodyText.Length > maxLength)
        {
            return bodyText.Substring(0, maxLength) + "...(truncated)";
        }

        return bodyText;
    }

}
