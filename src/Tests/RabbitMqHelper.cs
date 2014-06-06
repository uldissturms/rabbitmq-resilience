using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

static internal class RabbitMqHelper
{
	public static void DeleteQueue(string queueName)
	{
		using (var connection = GetConnection())
		using (var channel = connection.CreateModel())
		{
			try
			{
				channel.QueueDelete(queueName);
			}
			catch { }
		}
	}

	public static uint GetMessageCountInAQueue(string queueName)
	{
		using (var connection = GetConnection())
		using (var channel = connection.CreateModel())
		{
			var queueDeclareOk = channel.QueueDeclare(queueName, true, false, false, null);
			return queueDeclareOk.MessageCount;
		}
	}

	public static void DeclareQueue(string queueName, IDictionary<string, object> arguments = null)
	{
		using (var connection = GetConnection())
		using (var channel = connection.CreateModel())
		{
			channel.ExchangeDeclare(queueName, ExchangeType.Direct);
			channel.QueueDeclare(queueName, true, false, false, arguments);
			channel.QueueBind(queueName, queueName, string.Empty);
		}
	}

	public static void SendMessage(string message, string queueName)
	{
		using (var connection = GetConnection())
		using (var channel = connection.CreateModel())
		{
			var body = Encoding.UTF8.GetBytes(message);
			channel.BasicPublish(queueName, string.Empty, null, body);
		}
	}

	private static IConnection GetConnection()
	{
		var factory = new ConnectionFactory { HostName = "localhost" };
		return factory.CreateConnection();
	}

	public static string GetMessageAndNack(string queueName, bool requeue)
	{
		using (var connection = GetConnection())
		{
			using (var channel = connection.CreateModel())
			{
				channel.QueueDeclare(queueName, true, false, false, null);

				var consumer = new QueueingBasicConsumer(channel);
				channel.BasicConsume(queueName, false, consumer);

				var message = consumer.Queue.Dequeue();
				var body = message.Body;

				channel.BasicNack(message.DeliveryTag, false, requeue);
				return Encoding.UTF8.GetString(body);
			}
		}
	}
}