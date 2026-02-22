using System.Net;
using System.Net.Http;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using RevitTranslator.Common.Messages;
using TranslationService.Models;
using TranslationService.Utils;
using Xunit;
using Assert = Xunit.Assert;

namespace RevitTranslator.Tests;

public class DeeplTranslationClientTests : IDisposable
{
    private static readonly DeeplSettingsDescriptor TestSettings = new()
    {
        DeeplApiKey = "test-api-key",
        IsPaidPlan = false,
        SourceLanguage = null,
        TargetLanguage = new LanguageDescriptor("English", "en", "EN-US")
    };

    private const string TranslationUrl = "https://api-free.deepl.com/v2/translate";
    private const string UsageUrl = "https://api-free.deepl.com/v2/usage";

    private TokenCancellationRequestedMessage? _receivedCancellationMessage;

    public DeeplTranslationClientTests()
    {
        StrongReferenceMessenger.Default.Register<TokenCancellationRequestedMessage>(this,
            (_, message) => _receivedCancellationMessage = message);
    }

    public void Dispose()
    {
        StrongReferenceMessenger.Default.UnregisterAll(this);
        _receivedCancellationMessage = null;
    }

    private DeeplTranslationClient CreateClient(StubHttpMessageHandler handler) =>
        new(new HttpClient(handler), TestSettings, TranslationUrl, UsageUrl, TimeSpan.Zero);

    [Fact]
    public async Task TranslateTextAsync_Returns_TranslatedText_On200()
    {
        var handler = new StubHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"translations":[{"detected_source_language":"FR","text":"Bonjour"}]}""",
                Encoding.UTF8, "application/json")
        });
        var client = CreateClient(handler);

        var result = await client.TranslateTextAsync("hello", CancellationToken.None);

        Assert.Equal("Bonjour", result);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task TranslateTextAsync_ReturnsNull_On5xx()
    {
        var handler = new StubHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = CreateClient(handler);

        var result = await client.TranslateTextAsync("hello", CancellationToken.None);

        Assert.Null(result);
        Assert.Null(_receivedCancellationMessage);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task TranslateTextAsync_RetriesOn429_ThenSucceeds()
    {
        var handler = new StubHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage((HttpStatusCode) 429));
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"translations":[{"detected_source_language":"FR","text":"Bonjour"}]}""",
                Encoding.UTF8, "application/json")
        });
        var client = CreateClient(handler);

        var result = await client.TranslateTextAsync("hello", CancellationToken.None);

        Assert.Equal("Bonjour", result);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task TranslateTextAsync_ReturnsNull_AfterRetryLimit()
    {
        var handler = new StubHttpMessageHandler();
        for (var i = 0; i < 6; i++)
        {
            handler.Enqueue(new HttpResponseMessage((HttpStatusCode) 429));
        }
        var client = CreateClient(handler);

        var result = await client.TranslateTextAsync("hello", CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(6, handler.RequestCount);
    }

    [Fact]
    public async Task TranslateTextAsync_SendsCancellationMessage_On403()
    {
        var handler = new StubHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage((HttpStatusCode) 403));
        var client = CreateClient(handler);

        var result = await client.TranslateTextAsync("hello", CancellationToken.None);

        Assert.Null(result);
        Assert.NotNull(_receivedCancellationMessage);
        Assert.Contains("Authorisation failed", _receivedCancellationMessage.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TranslateTextAsync_SendsCancellationMessage_On456()
    {
        var handler = new StubHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage((HttpStatusCode) 456));
        var client = CreateClient(handler);

        var result = await client.TranslateTextAsync("hello", CancellationToken.None);

        Assert.Null(result);
        Assert.NotNull(_receivedCancellationMessage);
        Assert.Contains("Quota exceeded", _receivedCancellationMessage.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TranslateTextAsync_ReturnsNull_WhenTokenCancelled()
    {
        var handler = new StubHttpMessageHandler();
        var client = CreateClient(handler);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await client.TranslateTextAsync("hello", cts.Token);

        Assert.Null(result);
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task CheckUsageAsync_ParsesResponse_OnSuccess()
    {
        var handler = new StubHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"character_count":12345,"character_limit":500000}""",
                Encoding.UTF8, "application/json")
        });
        var client = CreateClient(handler);

        var result = await client.CheckUsageAsync();

        Assert.True(result);
        Assert.Equal(12345, client.Usage);
        Assert.Equal(500000, client.Limit);
    }

    [Fact]
    public async Task CheckUsageAsync_ReturnsFalse_OnFailure()
    {
        var handler = new StubHttpMessageHandler();
        handler.Enqueue(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = CreateClient(handler);

        var result = await client.CheckUsageAsync();

        Assert.False(result);
        Assert.Equal(-1, client.Usage);
        Assert.Equal(-1, client.Limit);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();
        public int RequestCount { get; private set; }

        public void Enqueue(HttpResponseMessage response) => _responses.Enqueue(response);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(_responses.Count > 0
                ? _responses.Dequeue()
                : new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }
    }
}
