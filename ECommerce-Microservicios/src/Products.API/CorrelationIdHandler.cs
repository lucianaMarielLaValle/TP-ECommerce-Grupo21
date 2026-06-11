namespace Products.API;

public class CorrelationIdHandler : DelegatingHandler
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly IHttpContextAccessor _accessor;

    public CorrelationIdHandler(IHttpContextAccessor accessor) => _accessor = accessor;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = _accessor.HttpContext?.Items["CorrelationId"]?.ToString();
        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.Remove(HeaderName);
            request.Headers.Add(HeaderName, correlationId);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}