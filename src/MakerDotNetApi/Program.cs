using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI/Swagger generation for easy import into Power Automate Custom Connector
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// GET /hello -> returns a JSON message
app.MapGet("/hello", () => Results.Json(new { message = "Hello from .NET 👋" }));

// POST /summarize -> accepts { "text": "..." } and returns { "summary": "..." }
app.MapPost("/summarize", (SummarizeRequest req) =>
{
    if (req is null || string.IsNullOrWhiteSpace(req.Text))
        return Results.BadRequest(new { error = "Request must include a non-empty 'text' field." });

    var summary = Summarizer.Summarize(req.Text);
    return Results.Json(new SummarizeResponse(summary));
});

app.Run();

// DTOs
public record SummarizeRequest([property: JsonPropertyName("text")] string Text);
public record SummarizeResponse([property: JsonPropertyName("summary")] string Summary);

// Simple summarizer (naive): returns the first sentence if present, otherwise truncates the text.
static class Summarizer
{
    public static string Summarize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        // Try to find the first sentence terminator.
        var periodIdx = text.IndexOfAny(new[] {'.','!','?'});
        if (periodIdx > 0 && periodIdx < 240) return text.Substring(0, periodIdx + 1).Trim();
        // Otherwise, truncate to ~200 characters without cutting mid-word.
        var max = 200;
        if (text.Length <= max) return text.Trim();
        var truncated = text.Substring(0, max);
        var lastSpace = truncated.LastIndexOf(' ');
        if (lastSpace > 0) truncated = truncated.Substring(0, lastSpace);
        return truncated.Trim() + "...";
    }
}

/*
Power Automate Custom Connector guidance:

- When you run this app (e.g., `dotnet run`), ASP.NET Core exposes OpenAPI/Swagger at `/swagger/v1/swagger.json` and
  a Swagger UI at `/swagger` by default when running in Development. Power Automate can import the OpenAPI JSON
  to create a custom connector.

- For a production scenario, secure the API (API key, Azure AD, OAuth2 client credentials).
  Add authentication middleware and then configure the connector's security accordingly.

- Example usage:
  1) Host the app (App Service, Azure Functions, or any HTTPS endpoint).
  2) In Power Automate > Data > Custom connectors > New > Import an OpenAPI file, provide the public URL
     to `/swagger/v1/swagger.json`.
  3) Map the `POST /summarize` action to a flow step; pass the `text` value from previous steps.

- Keep responses small and predictable (the `summary` field) for better use in flows.
*/
