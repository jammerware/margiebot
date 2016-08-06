using System;
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
            byte[] buffer = new byte[1024];
            
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
#if DEBUG
                    Console.WriteLine($"Receive: {_encoding.GetString(buffer)}");
#endif
                    var data = _encoding.GetString(buffer);
                    // TODO: should i be doing this, and if not, what should i do instead?
                    data = TrimStuffIDontKnowWhatItEvenIs(data);
                    OnMessage?.Invoke(this, data);
                }
            }
        }

        private string TrimStuffIDontKnowWhatItEvenIs(string input)
        {
            var openBraceCount = 0;
            var indexOfLastBrace = 0;

            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == '{')
                {
                    openBraceCount++;
                }
                else if (input[i] == '}')
                {
                    openBraceCount--;
                }

                if (openBraceCount == 0)
                {
                    indexOfLastBrace = i;
                    break;
                }
            }

            return input.Substring(0, indexOfLastBrace + 1);
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