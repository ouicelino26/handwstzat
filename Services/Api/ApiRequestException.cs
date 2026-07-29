using System.Net;

namespace HandWStat.Services.Api;

public sealed class ApiRequestException : Exception
{
    public ApiRequestException(
        string userMessage,
        string technicalCode,
        string? correlationId,
        bool retryable,
        HttpStatusCode? statusCode,
        Exception? innerException = null)
        : base(userMessage, innerException)
    {
        UserMessage = userMessage;
        TechnicalCode = technicalCode;
        CorrelationId = correlationId;
        Retryable = retryable;
        StatusCode = statusCode;
    }

    public string UserMessage { get; }

    public string TechnicalCode { get; }

    public string? CorrelationId { get; }

    public bool Retryable { get; }

    public HttpStatusCode? StatusCode { get; }
}
