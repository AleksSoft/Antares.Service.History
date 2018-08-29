﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.History.Contracts.History;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.History.Controllers
{
    [Route("api/history")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;

        public HistoryController(IHistoryRecordsRepository historyRecordsRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
        }

        /// <summary>
        /// Get wallet history
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="type"></param>
        /// <param name="assetId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("")]
        [SwaggerOperation("GetHistory")]
        [ProducesResponseType(typeof(IReadOnlyList<BaseHistoryModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<BaseHistoryModel>> GetHistory(
            [FromQuery] Guid walletId,
            [FromQuery(Name = "type")] HistoryType[] type,
            [FromQuery] string assetId = null,
            [FromQuery] string assetPairId = null,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100)
        {
            if (type.Length == 0)
                type = Enum.GetValues(typeof(HistoryType)).Cast<HistoryType>().ToArray();

            var data = await _historyRecordsRepository.GetByWallet(walletId, type, offset, limit, assetPairId, assetId);

            return Mapper.Map<IReadOnlyList<BaseHistoryModel>>(data);
        }
    }
}
