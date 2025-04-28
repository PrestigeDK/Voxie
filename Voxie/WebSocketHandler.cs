using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketHandler
{
    private readonly ConcurrentDictionary<string, WebSocket> _connectedSockets = new();

    public async Task HandleConnectionAsync(WebSocket socket, string connectionId)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message} from {connectionId}");

                    await BroadcastMessageAsync(connectionId, message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"Closing connection: {connectionId}");
                    await CloseConnectionAsync(connectionId, socket);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await CloseConnectionAsync(connectionId, socket);
        }
    }

    private async Task BroadcastMessageAsync(string senderId, string message)
    {
        var serverMsg = Encoding.UTF8.GetBytes($"[{senderId}]: {message}");

        foreach (var pair in _connectedSockets)
        {
            if (pair.Value.State == WebSocketState.Open)
            {
                await pair.Value.SendAsync(new ArraySegment<byte>(serverMsg), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    private async Task CloseConnectionAsync(string connectionId, WebSocket socket)
    {
        if (socket.State != WebSocketState.Closed)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
        _connectedSockets.TryRemove(connectionId, out _);
    }

    public void AddSocket(string connectionId, WebSocket socket)
    {
        _connectedSockets.TryAdd(connectionId, socket);
    }
}