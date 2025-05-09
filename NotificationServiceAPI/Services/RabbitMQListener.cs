using Microsoft.AspNetCore.Connections;
using NotificationServiceAPI.Models;
using Org.BouncyCastle.Crypto;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace NotificationServiceAPI.Services
{
    public class RabbitMQListener : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQListener(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory() { HostName = "rabbitmq" ,Port=5672 ,UserName="guest",Password="guest",DispatchConsumersAsync=true}; 
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "email_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "product_created_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var productEvent = System.Text.Json.JsonSerializer.Deserialize<ProductCreatedEvent>(message);
                var emailData = System.Text.Json.JsonSerializer.Deserialize<EmailMessage>(message);
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                if (emailData != null)
                { 
                    await emailService.SendEmailAsync(emailData);
                }
                else if (productEvent != null)
                {
                    EmailMessage emailProductData= new EmailMessage();
                    emailProductData.To = productEvent.ToEmail;
                    emailProductData.Subject = productEvent.Subject;
                    emailProductData.Body = productEvent.Body ;
                    await emailService.SendEmailAsync(emailData);
                }
            };

            _channel.BasicConsume(queue: "email_queue", autoAck: true, consumer: consumer);
            _channel.BasicConsume(queue: "product_created_queue", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

    }
}
