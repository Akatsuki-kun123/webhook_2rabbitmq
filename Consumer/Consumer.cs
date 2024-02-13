using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using DotEnv.Core;

namespace RabbitMQ
{
    public class Consumer
    {
        private readonly ConnectionFactory factory;
        private readonly string exchangeName;
        private readonly string queueName;
        private readonly string routingKey;

        public Consumer()
        {
            EnvLoader env = new();
            env.AddEnvFile(".env");
            env.Load();

            EnvReader reader = new();
            factory = new ConnectionFactory
            {
                HostName = reader["HOST"],
                Port = Int32.Parse(reader["PORT"]),
                UserName = reader["RBMQUSER"],
                Password = reader["RBMQPASS"],
                VirtualHost = reader["VHOST"]
            };
            exchangeName = "Test";
            queueName = "Test";
            routingKey = "Test";
        }

        public void Run()
        {
            IConnection connection = factory.CreateConnection();
            IModel channel = connection.CreateModel();
            {
                channel.ExchangeDeclare(exchange: exchangeName, type: "direct", durable: true);
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                Console.WriteLine("Consumer started. Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        public static void Main(string[] args)
        {
            Consumer consumer = new Consumer();
            consumer.Run();
        }
    }
}
