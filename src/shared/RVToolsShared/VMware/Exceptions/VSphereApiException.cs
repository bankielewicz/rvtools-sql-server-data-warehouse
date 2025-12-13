using System.Net;

namespace RVToolsShared.VMware.Exceptions;

/// <summary>
/// Exception thrown when a vSphere API call fails.
/// </summary>
public class VSphereApiException : Exception
{
    /// <summary>
    /// The HTTP status code returned by the API (if applicable).
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// The API endpoint that was called.
    /// </summary>
    public string? Endpoint { get; }

    /// <summary>
    /// The vCenter server address.
    /// </summary>
    public string? ServerAddress { get; init; }

    /// <summary>
    /// The raw response body (if available).
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Error details from vSphere API response.
    /// </summary>
    public VSphereErrorInfo? ErrorInfo { get; }

    /// <summary>
    /// Creates a new VSphereApiException.
    /// </summary>
    public VSphereApiException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates a new VSphereApiException with an inner exception.
    /// </summary>
    public VSphereApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a new VSphereApiException with HTTP details.
    /// </summary>
    public VSphereApiException(
        string message,
        HttpStatusCode statusCode,
        string? endpoint = null,
        string? serverAddress = null,
        string? responseBody = null,
        VSphereErrorInfo? errorInfo = null)
        : base(message)
    {
        StatusCode = statusCode;
        Endpoint = endpoint;
        ServerAddress = serverAddress;
        ResponseBody = responseBody;
        ErrorInfo = errorInfo;
    }

    /// <summary>
    /// Creates a new VSphereApiException with HTTP details and inner exception.
    /// </summary>
    public VSphereApiException(
        string message,
        HttpStatusCode statusCode,
        Exception innerException,
        string? endpoint = null,
        string? serverAddress = null,
        string? responseBody = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Endpoint = endpoint;
        ServerAddress = serverAddress;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Creates an exception for authentication failure.
    /// </summary>
    public static VSphereApiException AuthenticationFailed(string serverAddress, string? details = null)
    {
        var message = $"Authentication failed for vCenter '{serverAddress}'";
        if (!string.IsNullOrEmpty(details))
            message += $": {details}";

        return new VSphereApiException(message, HttpStatusCode.Unauthorized)
        {
            ServerAddress = serverAddress
        };
    }

    /// <summary>
    /// Creates an exception for connection failure.
    /// </summary>
    public static VSphereApiException ConnectionFailed(string serverAddress, Exception innerException)
    {
        return new VSphereApiException(
            $"Failed to connect to vCenter '{serverAddress}': {innerException.Message}",
            innerException)
        {
            ServerAddress = serverAddress
        };
    }

    /// <summary>
    /// Creates an exception for session not established.
    /// </summary>
    public static VSphereApiException SessionNotEstablished(string serverAddress)
    {
        return new VSphereApiException(
            $"No session established with vCenter '{serverAddress}'. Call CreateSessionAsync first.")
        {
            ServerAddress = serverAddress
        };
    }

    /// <summary>
    /// Creates an exception for session expired.
    /// </summary>
    public static VSphereApiException SessionExpired(string serverAddress)
    {
        return new VSphereApiException(
            $"Session expired for vCenter '{serverAddress}'. Re-authenticate to continue.",
            HttpStatusCode.Unauthorized)
        {
            ServerAddress = serverAddress
        };
    }

    /// <summary>
    /// Creates an exception for a generic API error.
    /// </summary>
    public static VSphereApiException ApiError(
        string serverAddress,
        string endpoint,
        HttpStatusCode statusCode,
        string? responseBody = null)
    {
        var message = $"vSphere API error: {endpoint} returned {(int)statusCode} {statusCode}";
        return new VSphereApiException(message, statusCode, endpoint, serverAddress, responseBody);
    }

    /// <summary>
    /// Creates an exception for timeout.
    /// </summary>
    public static VSphereApiException Timeout(string serverAddress, string endpoint, int timeoutSeconds)
    {
        return new VSphereApiException(
            $"Request to vCenter '{serverAddress}' endpoint '{endpoint}' timed out after {timeoutSeconds} seconds.",
            HttpStatusCode.RequestTimeout,
            endpoint,
            serverAddress);
    }
}

/// <summary>
/// Error information from vSphere API response.
/// </summary>
public class VSphereErrorInfo
{
    /// <summary>
    /// Error type/code from vSphere.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Additional error details.
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }
}
