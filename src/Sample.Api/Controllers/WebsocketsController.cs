using System.Buffers;
using System.Net.WebSockets;

using Microsoft.AspNetCore.Mvc;

namespace Sample.Api.Controllers;

[ApiController]
public class WeatherForecastController : ControllerBase
{
    private static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/ws")]
    public async Task Get(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request received");
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            _logger.LogInformation("WebSocket request received");
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket, cancellationToken);
        }
        else
        {
            _logger.LogInformation("Not a websocket request");
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private static async Task Echo(WebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = _arrayPool.Rent(1024 * 4);
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), cancellationToken);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                cancellationToken);

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            
            _arrayPool.Return(buffer);
        }
        
        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}
