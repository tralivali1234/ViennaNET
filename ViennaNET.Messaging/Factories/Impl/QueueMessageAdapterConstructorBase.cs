﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using ViennaNET.Messaging.Configuration;
using ViennaNET.Messaging.Exceptions;
using ViennaNET.Utils;

namespace ViennaNET.Messaging.Factories.Impl
{
  /// <inheritdoc />
  public abstract class QueueMessageAdapterConstructorBase<TConf, TQueueConf> : IMessageAdapterConstructor
    where TQueueConf : QueueConfigurationBase where TConf : ConfigurationsListBase<TQueueConf>
  {
    private readonly TConf _configuration;

    /// <summary>
    ///   Инициализирует конструктор адаптеров экземпляром класса <see cref="IConfiguration" />
    ///   и именем секции с очередями типа конструктора в конфигурации
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    protected QueueMessageAdapterConstructorBase(IConfiguration configuration, string sectionName)
    {
      _configuration = configuration.ThrowIfNull(nameof(configuration))
                                    .GetSection(sectionName)
                                    .Get<TConf>();
    }

    /// <inheritdoc />
    public IMessageAdapter Create(string queueId)
    {
      var queueConfiguration = _configuration?.GetQueueConfiguration(queueId);

      if (queueConfiguration == null)
      {
        throw new MessagingConfigurationException($"There are no configuration with id '{queueId}' in configuration file");
      }

      return CreateInternal(queueConfiguration);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IMessageAdapter> CreateAll()
    {
      return _configuration == null
        ? new IMessageAdapter[0]
        : _configuration.Queues.Select(x => CreateInternal(x)).ToArray();
    }

    /// <inheritdoc />
    public bool HasQueue(string queueId)
    {
      return _configuration?.GetQueueConfiguration(queueId) != null;
    }

    private IMessageAdapter CreateInternal(TQueueConf queueConfiguration)
    {
      CheckConfigurationParameters(queueConfiguration);

      return CreateAdapter(queueConfiguration);
    }

    /// <summary>
    ///   Создает экземпляр адаптера в соответствии с типом конструктора
    /// </summary>
    /// <param name="queueConfiguration">Конфигурация очереди</param>
    /// <returns></returns>
    protected abstract IMessageAdapter CreateAdapter(TQueueConf queueConfiguration);

    /// <summary>
    ///   Проверяет параметры конфигурации
    /// </summary>
    /// <param name="configuration">Конфигурация</param>
    protected abstract void CheckConfigurationParameters(TQueueConf configuration);
  }
}