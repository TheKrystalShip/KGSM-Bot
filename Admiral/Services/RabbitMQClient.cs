using TheKrystalShip.Admiral.Domain;
using RabbitMQ.Client.Events;
using TheKrystalShip.Logging;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;

namespace TheKrystalShip.Admiral.Services
{
    public class RabbitMQClient
    {
        private ConnectionFactory? _factory;
        private IConnection? _connection;
        private readonly ILogger<RabbitMQClient> _logger;
        private readonly DiscordNotifier _discordNotifier;

        public RabbitMQClient(DiscordNotifier discordNotifier)
        {
            _discordNotifier = discordNotifier;
            _logger = new Logger<RabbitMQClient>();
        }

        public Task LoginAsync(string uri)
        {
            _factory = new ConnectionFactory { Uri = new Uri(uri) };
            _connection = _factory.CreateConnection();

            return Task.CompletedTask;
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

        private async void OnReceived(object? sender, BasicDeliverEventArgs args)
        {
            byte[] body = args.Body.ToArray();

            string message = Encoding.UTF8.GetString(body);

            ServiceStatusMessage? statusMessage = JsonSerializer.Deserialize<ServiceStatusMessage>(message);

            if (statusMessage is null)
            {
                _logger.LogError("Failed to deserialize message");
                return;
            }

            await _discordNotifier.OnRunningStatusUpdated(new(new(statusMessage.service, "", ""), statusMessage.RunningStatus));
        }
    }
}
