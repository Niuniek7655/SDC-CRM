using System.Net;
using System.Text;

namespace SDC.CRM.Mobile.Tests.TestDoubles;

/// <summary>
/// In-memory replacement for the network: returns a prepared response and records every request,
/// so HTTP-level behavior (status codes, headers, JSON) is tested without a server.
/// </summary>
internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];

    public static StubHttpMessageHandler Returning(HttpStatusCode statusCode, string? json = null)
        => new(_ => new HttpResponseMessage(statusCode)
        {
            Content = json is null ? null : new StringContent(json, Encoding.UTF8, "application/json"),
        });

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(respond(request));
    }
}

