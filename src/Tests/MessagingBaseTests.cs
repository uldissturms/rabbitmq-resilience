using System;
using System.Threading;
using NUnit.Framework;
using ServiceStack.Text;
using SevenDigital.Messaging;
using StructureMap;

namespace Tests
{
	public class MessagingBaseTests
	{
		private const string _queueName = "messaging-base";

		[Test]
		public void Should_not_ack_message_when_exception_is_thrown_while_processing_message_but_acks()
		{
			RabbitMqHelper.DeleteQueue(_queueName);

			ConfigureMessaging();

			Thread.Sleep(TimeSpan.FromSeconds(1));

			SendSampleMessage();

			Thread.Sleep(TimeSpan.FromSeconds(1));

			var messageCount = RabbitMqHelper.GetMessageCountInAQueue(_queueName);

			Assert.That(messageCount, Is.EqualTo(0));
		}

		private static void SendSampleMessage()
		{
			var senderNode = ObjectFactory.GetInstance<ISenderNode>();

			var sampleMessage = DynamicProxy.GetInstanceFor<ISampleMessage>();
			sampleMessage.Text = "sample message";

			senderNode.SendMessage(sampleMessage);
		}

		private static void ConfigureMessaging()
		{
			MessagingSystem.Configure.WithDefaults();
			MessagingSystem.Sender();

			MessagingSystem.Receiver().TakeFrom(_queueName, _ => _.Handle<ISampleMessage>().With<SampleMessageHandler>());
		}
	}

	public class SampleMessageHandler : IHandle<ISampleMessage>
	{
		public void Handle(ISampleMessage message)
		{
			throw new Exception("Message should not be ack-ed in case exception is thrown");
		}
	}

	public interface ISampleMessage : IMessage
	{
		string Text { get; set; }
	}

	public class AssertEventHook : IEventHook
	{
		public void MessageSent(IMessage message) { }
		public void MessageReceived(IMessage message) { }
		public void HandlerFailed(IMessage message, Type handler, Exception ex) {}
	}
}
