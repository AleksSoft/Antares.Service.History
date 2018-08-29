﻿using Lykke.HttpClientGenerator;

namespace Lykke.Service.History.Client
{
    /// <inheritdoc />
    /// <summary>
    ///     History API aggregating interface.
    /// </summary>
    public class HistoryClient : IHistoryClient
    {
        /// <summary>C-tor</summary>
        public HistoryClient(IHttpClientGenerator httpClientGenerator)
        {
            HistoryApi = httpClientGenerator.Generate<IHistoryApi>();
            OrdersApi = httpClientGenerator.Generate<IOrdersApi>();
        }
        // Note: Add similar Api properties for each new service controller

        /// <inheritdoc />
        public IHistoryApi HistoryApi { get; }

        /// <inheritdoc />
        public IOrdersApi OrdersApi { get; }
    }
}
