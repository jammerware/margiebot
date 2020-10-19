using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MargieBot.WebSockets
{
    public delegate void MargieBotWebSocketMessageReceivedEventHandler(object sender, string message);

    public class MargieBotWebSocket : IDisposable
    {
        ClientWebSocket _webSocket = null;
        private static UTF8Encoding _encoding = new UTF8Encoding();

        public async Task Connect(string uri)
        {
            await Connect(new Uri(uri));
        }

        public async Task Connect(Uri uri)
        {
            _webSocket?.Dispose();
            _webSocket = new ClientWebSocket();

            await _webSocket.ConnectAsync(uri, CancellationToken.None);
            var task = Task.Run(async () => { await Listen(); });

            OnOpen?.Invoke(this, EventArgs.Empty);
        }

        public async Task Disconnect()
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
            _webSocket.Dispose();
            OnClose?.Invoke(this, EventArgs.Empty);
        }

        public async Task Send(string message)
        {
            await _webSocket.SendAsync(new ArraySegment<byte>(_encoding.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        #region Events
        public event EventHandler OnClose;
        public event MargieBotWebSocketMessageReceivedEventHandler OnMessage;
        public event EventHandler OnOpen;
        #endregion

        #region Internal utility
		private async Task Listen()
		{
			ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[1024]);
            WebSocketReceiveResult result = null;

			while (_webSocket.State == WebSocketState.Open)
			{
				using (var ms = new MemoryStream())
				{
					do
					{
						result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);

						if (result.MessageType == WebSocketMessageType.Close)
						{
							await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
							break;
						}
						else
						{
							ms.Write(buffer.Array, buffer.Offset, result.Count);
						}
					}
					while (!result.EndOfMessage);

					ms.Seek(0, SeekOrigin.Begin);

					if (result.MessageType == WebSocketMessageType.Text)
					{
						using (var reader = new StreamReader(ms, Encoding.UTF8))
						{
							var stringData = await reader.ReadToEndAsync();

#if DEBUG
							Console.WriteLine($"Receive: {stringData}");
#endif

							OnMessage?.Invoke(this, stringData);
						}
					}
				}
			}
		}
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _webSocket?.Dispose();
        }
        #endregion
    }
}