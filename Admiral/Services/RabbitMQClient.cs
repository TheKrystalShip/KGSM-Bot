using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

using Newtonsoft.Json;

using System.Text;

namespace TheKrystalShip.Admiral.Services;

public class RabbitMQClient
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly Logger<RabbitMQClient> _logger;

    public event Func<RunningStatusUpdatedArgs, Task> StatusChangeReceived;

    public RabbitMQClient(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentNullException(nameof(url));
        }

        _logger = new();
        _factory = new ConnectionFactory { Uri = new Uri(url) };
    }

    public Task<bool> LoginAsync()
    {
        try
        {
            _connection = _factory.CreateConnection();
        }
        catch (BrokerUnreachableException e)
        {
            _logger.LogError($"Error: {e.Message}");
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public async Task StartAsync(string routingKey)
    {
        if (string.IsNullOrEmpty(routingKey))
        {
            throw new ArgumentNullException(nameof(routingKey));
        }

        IModel? channel = _connection?.CreateModel();
        channel?.QueueDeclarePassive(routingKey);

        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
        consumer.Received += OnReceived;

        channel.BasicConsume(routingKey, autoAck: true, consumer: consumer);

        _logger.LogInformation("RabbitMQClient started, waiting for messages");

        await Task.Delay(Timeout.Infinite);
    }

    private void OnReceived(object? sender, BasicDeliverEventArgs args)
    {
        byte[] body = args.Body.ToArray();

        string message = Encoding.UTF8.GetString(body);

        ServiceStatusMessage? statusMessage =
            JsonConvert.DeserializeObject<ServiceStatusMessage>(message);

        if (statusMessage is null)
        {
            _logger.LogError("Failed to deserialize message");
            return;
        }

        StatusChangeReceived(
            new RunningStatusUpdatedArgs(
                new Game(statusMessage.service, "", ""),
                statusMessage.RunningStatus
            )
        );
    }
}
