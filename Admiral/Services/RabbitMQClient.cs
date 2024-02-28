using TheKrystalShip.Admiral.Domain;
using RabbitMQ.Client.Events;
using TheKrystalShip.Logging;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client.Exceptions;

namespace TheKrystalShip.Admiral.Services
{
    public class RabbitMQClient
    {
        private ConnectionFactory? _factory;
        private IConnection? _connection;
        private readonly ILogger<RabbitMQClient> _logger;
        private readonly DiscordNotifier _discordNotifier;

        private const int MAX_RETRY_ATTEMPTS = 10;

        public RabbitMQClient(DiscordNotifier discordNotifier)
        {
            _discordNotifier = discordNotifier;
            _logger = new Logger<RabbitMQClient>();
        }

        public Task<bool> LoginAsync(string uri)
        {
            return LoginAsync(uri, TimeSpan.FromSeconds(0), 1);
        }

        public async Task StartAsync(string routingKey)
        {
            IModel? channel = _connection?.CreateModel();
            channel?.QueueDeclarePassive(routingKey);

            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnReceived;

            channel.BasicConsume(routingKey, autoAck: true, consumer: consumer);

            _logger.LogInformation("RabbitMQClient started, waiting for messages");

            await Task.Delay(Timeout.Infinite);
        }

        private Task<bool> LoginAsync(string url, TimeSpan sleep, int attempt)
        {
            if (attempt >= MAX_RETRY_ATTEMPTS)
            {
                _logger.LogError("Reached max login attempts, exiting");
                return Task.FromResult(false);
            }

            Thread.Sleep(sleep);

            _factory = new ConnectionFactory { Uri = new Uri(url) };

            try {
                _connection = _factory.CreateConnection();
            } catch (BrokerUnreachableException) {
                _logger.LogError($"Attempt {attempt}/{MAX_RETRY_ATTEMPTS}: RabbitMQ Broker unreachable, retrying");
                return LoginAsync(url, TimeSpan.FromSeconds(60), ++attempt);
            }

            return Task.FromResult(true);
        }

        private async void OnReceived(object? sender, BasicDeliverEventArgs args)
        {
            byte[] body = args.Body.ToArray();

            string message = Encoding.UTF8.GetString(body);

            ServiceStatusMessage? statusMessage = JsonConvert.DeserializeObject<ServiceStatusMessage>(message);

            if (statusMessage is null)
            {
                _logger.LogError("Failed to deserialize message");
                return;
            }

            await _discordNotifier.OnRunningStatusUpdated(new(new(statusMessage.service, "", ""), statusMessage.RunningStatus));
        }
    }
}
