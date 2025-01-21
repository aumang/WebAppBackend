using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using ProductPriceTracker.Services;
using System.Net.WebSockets;
using System.Text;
using WebAppBackend.DTO;
using WebAppBackend.Hubs;

namespace WebAppBackend.Services
{
    public class StockMarketService
    {
        private readonly CustomCircuitBreaker _customCircuitBreaker;
        private ClientWebSocket _clientWebSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private IHubContext<StockMarketHub> _hubContext;
        private bool _isListening;

        
        public StockMarketService(CustomCircuitBreaker customCircuitBreaker, IHubContext<StockMarketHub> hubContext)
        {
            _customCircuitBreaker = customCircuitBreaker;
            _clientWebSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
            _isListening = false;
            _hubContext = hubContext;
        }

        public async Task StartTradeValue(string url)
            {
            if (_isListening)
            {
                await SendLogToFrontend("WebSocket is already listening.");
                return;
            }
            _isListening = true;
            StockValueDto tradeValue;
            await SendLogToFrontend($"Starting to listen to WebSocket at {url}...");

            await _customCircuitBreaker.ExecuteAsync(async () =>
            {
                try
                {
                    // Check if WebSocket is already connected, if so close it and start fresh
                    if (_clientWebSocket.State == WebSocketState.Open || _clientWebSocket.State == WebSocketState.Connecting)
                    {
                        await SendLogToFrontend("WebSocket is already connected. Closing previous connection...");
                        await StopTradeValue(); // Gracefully stop the previous connection before starting a new one
                    }

                    await SendLogToFrontend("Attempting to connect to WebSocket...");
                    await _clientWebSocket.ConnectAsync(new Uri(url), _cancellationTokenSource.Token);

                    await SendLogToFrontend($"Successfully connected to {url}");
                    var buffer = new byte[1024 * 4];

                    // Start listening to messages from the WebSocket
                    while (_clientWebSocket.State == WebSocketState.Open)
                    {
                        var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await SendLogToFrontend("WebSocket connection closed by server.");
                            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", _cancellationTokenSource.Token);
                            break;
                        }

                        // Process the message received
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        //var tradeDataDto = JsonConvert.DeserializeObject(message); // Deserialize the message (adjust the model as needed)
                        //tradeValue = new StockValueDto();
                        //tradeValue.CircuitClosed = true;
                        //tradeValue.StockName = "Umang";
                        //tradeValue.StockRate = 1.00;
                        //await SendMarketValueToFrontEnd(tradeValue.ToString()); // Send data to the frontend
                        ////await SendLogToFrontend(tradeDataDto.ToString());
                        var tradeData = JsonConvert.DeserializeObject<dynamic>(message); // Deserialize to a dynamic object

                        // Extract relevant data (adjust based on the Binance WebSocket payload structure)
                        if (tradeData != null)
                        {
                            var tradeValue = new StockValueDto
                            {
                                StockName = "BTC/USDT",
                                StockRate = (double)tradeData.p, // Extracting price from the payload
                                CircuitClosed = false // Example flag
                            };

                            // Send data to the frontend
                            await SendMarketValueToFrontEnd(tradeValue);
                        }

                    }
                    await SendLogToFrontend("WebSocket listening finished.");
                }
                catch (Exception ex)
                {
                    await SendLogToFrontend($"An error occurred while listening to WebSocket: {ex.Message}");
                    throw;
                }
                return Task.CompletedTask;
            });

        }

        //public async Task StopTradeValue()
        //{
        //    if (!_isListening)
        //    {
        //        await SendLogToFrontend("WebSocket is not currently listening.");
        //        return;
        //    }
        //    _isListening = false;
        //    await SendLogToFrontend("Stopping WebSocket listener...");

        //    // Cancel the WebSocket listening task
        //    _cancellationTokenSource.Cancel();

        //    // Ensure that the WebSocket connection is closed gracefully
        //    if (_clientWebSocket.State == WebSocketState.Open)
        //    {
        //        await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stopped by client", _cancellationTokenSource.Token);
        //    }
        //    // Reset the WebSocket instance to allow a fresh connection later
        //    _clientWebSocket = new ClientWebSocket(); // Reinitialize WebSocket
        //    await SendLogToFrontend("WebSocket listener stopped successfully.");
        //}

        public async Task StopTradeValue()
        {
            if (!_isListening)
            {
                await SendLogToFrontend("WebSocket is not currently listening.");
                return;
            }

            _isListening = false;
            await SendLogToFrontend("Stopping WebSocket listener...");

            // Cancel the current task
            _cancellationTokenSource.Cancel();

            // Ensure the WebSocket connection is closed gracefully
            if (_clientWebSocket.State == WebSocketState.Open || _clientWebSocket.State == WebSocketState.Connecting)
            {
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stopped by client", CancellationToken.None);
            }

            // Reinitialize WebSocket and CancellationTokenSource for a fresh start
            _clientWebSocket.Dispose(); // Dispose the old WebSocket to free resources
            _clientWebSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource(); // Reinitialize cancellation token

            await SendLogToFrontend("WebSocket listener stopped successfully.");
        }


        private async Task SendLogToFrontend(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLog", message); // Send log to frontend
        }

        private async Task SendMarketValueToFrontEnd(StockValueDto stockValueDto)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMarketValue", stockValueDto);
        }
    }
}
