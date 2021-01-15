﻿using System.Collections.Generic;
using Antares.Job.History.RabbitSubscribers.Events;
using Antares.Service.History.Core.Domain.Orders;
using AutoMapper;

namespace Antares.Job.History.AutoMapper
{
    public class ExecutionConverter : ITypeConverter<ExecutionProcessedEvent, IEnumerable<Order>>
    {
        public IEnumerable<Order> Convert(ExecutionProcessedEvent source, IEnumerable<Order> destination,
            ResolutionContext context)
        {
            foreach (var item in source.Orders)
            {
                var order = Mapper.Map<Order>(item);

                order.SequenceNumber = source.SequenceNumber;

                foreach (var trade in order.Trades)
                    trade.OrderId = order.Id;

                yield return order;
            }
        }
    }
}
