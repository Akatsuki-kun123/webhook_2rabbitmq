using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using DotEnv.Core;

namespace RabbitMQ{
public class Producer
{
    private readonly ConnectionFactory factory;
    private readonly string exchangeName;
    private readonly string queueName;
    private readonly string routingKey;

    public Producer(){
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
        //exchangeName = "temp_exchange_" + DateTime.Now.Ticks;
        exchangeName = "Test";
        queueName = "Test";
        routingKey = "Test";
    }
    public void Run()
    {
        // Message Declare
        Dictionary<string,string> header = new()
        {
            {"Content-Type","text/plain"}
        };
        Dictionary<string, string> payload = new()
        {
            {"message", "Hello, World!"}
        };
        Message m = new("direct", header, payload, "127.0.0.1");
        
        // Serialize message
        string message = JsonSerializer.Serialize(m);
        byte[] body = Encoding.UTF8.GetBytes(message);

        // Creating a connection to RabbitMQ
        IConnection connection = factory.CreateConnection();
        IModel channel = connection.CreateModel();

        {
            // Declare temp exchange
            channel.ExchangeDeclare(exchange: exchangeName,
                                    type: "direct",
                                    durable: true,
                                    autoDelete: false
                                    );
            IBasicProperties properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            
            channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
            channel.ConfirmSelect();
            // Confirm
             if (!channel.WaitForConfirms(TimeSpan.FromSeconds(5))) // waits for up to 5 seconds
            {
                Console.WriteLine("One or more messages could not be confirmed.");
            }
            else
            {
                Console.WriteLine("All messages confirmed.");
            }

            // Routing Message
            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            
             channel.QueueBind(queue: queueName, 
                               exchange: exchangeName,
                               routingKey: routingKey);

            Console.WriteLine(" [x] Sent {0}", message);
        }
        Thread.Sleep(10000); // Cooldown 10s before clean up
        //Cleanup Process (This worked but currently not using this)
        // Cleanup();
    }

    private void Cleanup()
    {
        IConnection connection = factory.CreateConnection();
        IModel channel = connection.CreateModel();
        {
            // Delete the exchange after use
            channel.ExchangeDelete(exchange: exchangeName);
            Console.WriteLine($"Temporary exchange '{exchangeName}' deleted.");
        }
    }

    public static void Main(string[] args)
    {
        Producer prod = new();
        prod.Run();
    }
}
}