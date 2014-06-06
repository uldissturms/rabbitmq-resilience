using System.Collections.Generic;
using NUnit.Framework;

namespace Tests
{
	public class NativeClientTests
	{
		private const string _queueName = "native";
		private const string _deadLetterQueueName = "native-dead-letter";

		[SetUp]
		public void Before_each()
		{
			ConfigureMessaging(_queueName);
		}

		[Test]
		public void Should_requeue_message_on_no_ack()
		{
			RabbitMqHelper.SendMessage("sample message", _queueName);

			RabbitMqHelper.GetMessageAndNack(_queueName, false);

			var messageCount = RabbitMqHelper.GetMessageCountInAQueue(_queueName);

			Assert.That(messageCount, Is.EqualTo(1));
		}

		[Test]
		public void Should_dead_letter_at_the_end_of_queue()
		{
			RabbitMqHelper.SendMessage("1", _queueName);
			RabbitMqHelper.SendMessage("2", _queueName);

			var firstMessage = RabbitMqHelper.GetMessageAndNack(_queueName, false);
			var secondMessage = RabbitMqHelper.GetMessageAndNack(_queueName, false);

			Assert.That(firstMessage, Is.EqualTo("1"));
			Assert.That(secondMessage, Is.EqualTo("2"));
		}

		[Test]
		public void Should_requeue_at_the_same_place()
		{
			RabbitMqHelper.SendMessage("1", _queueName);
			RabbitMqHelper.SendMessage("2", _queueName);

			var firstMessage = RabbitMqHelper.GetMessageAndNack(_queueName, true);
			var secondMessage = RabbitMqHelper.GetMessageAndNack(_queueName, true);

			Assert.That(firstMessage, Is.EqualTo("1"));
			Assert.That(secondMessage, Is.EqualTo("1"));
		}

		[Test]
		public void Should_requeue_to_another_dead_letter_queue()
		{
			ConfigureMessaging(_deadLetterQueueName);

			RabbitMqHelper.SendMessage("sample message", _queueName);
			RabbitMqHelper.GetMessageAndNack(_queueName, false);
			var messageCount = RabbitMqHelper.GetMessageCountInAQueue(_deadLetterQueueName);

			Assert.That(messageCount, Is.EqualTo(1));
		}

		private static void ConfigureMessaging(string deadLetterQueueName)
		{
			RabbitMqHelper.DeleteQueue(_queueName);
			RabbitMqHelper.DeleteQueue(_deadLetterQueueName);

			RabbitMqHelper.DeclareQueue(_queueName, new Dictionary<string, object>
				{
					{"x-dead-letter-exchange", deadLetterQueueName}
				});
			RabbitMqHelper.DeclareQueue(_deadLetterQueueName);
		}
	}
}