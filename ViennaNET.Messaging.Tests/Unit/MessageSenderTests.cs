using System;
using Moq;
using NUnit.Framework;
using ViennaNET.Messaging.Messages;
using ViennaNET.Messaging.MQSeriesQueue;
using ViennaNET.Messaging.Sending.Impl;
using ViennaNET.Messaging.Tests.Unit.DSL;

namespace ViennaNET.Messaging.Tests.Unit
{
  [TestFixture(Category = "Unit", TestOf = typeof(MessageSender))]
  internal class MessageSenderTests
  {
    [Test]
    public void Dispose_Call_ShouldDisposeAdapter()
    {
      Mock<IMessageAdapter> adapterMock = null;
      var adapter = Given.MessageAdapter
        .Please(m => adapterMock = m);

      var messageSender = new MessageSender(adapter, Given.CallContextFactory, "ReApplication");
      messageSender.Dispose();

      adapterMock.Verify(x => x.Dispose());
    }

    [Test]
    public void Send_WithMessage_ShouldCallSendOnAdapter()
    {
      Mock<IMessageAdapter> adapterMock = null;
      var adapter = Given.MessageAdapter
        .Please(m => adapterMock = m);
      var message = new TextMessage {Body = "message body"};

      var messageSender = new MessageSender(adapter, Given.CallContextFactory, "ReApplication");
      messageSender.SendMessage(message);

      adapterMock.Verify(x => x.Send(message));
    }

    [Test]
    public void Send_WithMessage_MessageShouldHaveProperties()
    {
      Mock<IMessageAdapter> adapterMock = null;
      var adapter = Given.MessageAdapter
        .Please(m => adapterMock = m);
      var message = new TextMessage { Body = "message body" };

      var messageSender = new MessageSender(adapter, Given.CallContextFactory, "ReApplication");
      messageSender.SendMessage(message);

      adapterMock.Verify(x => x.Send(It.Is<BaseMessage>(m => m.Properties.Count > 0)));
    }

    [Test]
    public void Send_TransactedAdapter_ShouldCommit()
    {
      Mock<IMessageAdapterWithTransactions> adapterMock = null;
      var adapter = Given.MessageAdapter
        .Please<IMessageAdapterWithTransactions>(m => adapterMock = m);
      var message = new TextMessage { Body = "message body" };

      var messageSender = new MessageSender(adapter, Given.CallContextFactory, "ReApplication");
      messageSender.SendMessage(message);

      adapterMock.Verify(x => x.CommitIfTransacted(message));
    }

    [Test]
    public void Send_HasConfiguration_ShouldFillMessageParameters()
    {
      var config = new MqSeriesQueueConfiguration {ReplyQueue = "replyQueue", Lifetime = TimeSpan.FromHours(1)};
      Mock<IMessageAdapterWithTransactions> adapterMock = null;
      var adapter = Given.MessageAdapter
        .WithQueueConfiguration(config)
        .Please<IMessageAdapterWithTransactions>(m => adapterMock = m);
      var message = new TextMessage { Body = "message body" };

      var messageSender = new MessageSender(adapter, Given.CallContextFactory, "ReApplication");
      messageSender.SendMessage(message);

      adapterMock.Verify(
        x => x.Send(
          It.Is<BaseMessage>(m => m.LifeTime == config.Lifetime && m.ReplyQueue == config.ReplyQueue)));
    }
  }
}
