﻿using Lykke.Service.History.PostgresRepositories.Entities;
using PostgreSQLCopyHelper;

namespace Lykke.Service.History.PostgresRepositories.Mappings
{
    internal class OrderEntityBulkMapping
    {
        public static PostgreSQLCopyHelper<OrderEntity> Generate()
        {
            return new PostgreSQLCopyHelper<OrderEntity>(Constants.TempOrdersTableName)
                .MapUUID("id", x => x.Id)
                .MapUUID("wallet_id", x => x.WalletId)
                .MapUUID("matching_id", x => x.MatchingId)
                .MapInteger("type", x => (int)x.Type)
                .MapInteger("side", x => (int)x.Side)
                .MapInteger("status", x => (int)x.Status)
                .MapText("assetpair_id", x => x.AssetPairId)
                .MapNumeric("volume", x => x.Volume)
                .MapNullable("price", x => x.Price, NpgsqlTypes.NpgsqlDbType.Numeric)
                .MapTimeStamp("create_dt", x => x.CreateDt)
                .MapTimeStamp("register_dt", x => x.RegisterDt)
                .MapTimeStamp("status_dt", x => x.StatusDt)
                .MapNullable("match_dt", x => x.MatchDt, NpgsqlTypes.NpgsqlDbType.TimestampTz)
                .MapNumeric("remaining_volume", x => x.RemainingVolume)
                .MapText("reject_reason", x => x.RejectReason)
                .MapNullable("lower_limit_price", x => x.LowerLimitPrice, NpgsqlTypes.NpgsqlDbType.Numeric)
                .MapNullable("lower_price", x => x.LowerPrice, NpgsqlTypes.NpgsqlDbType.Numeric)
                .MapNullable("upper_limit_price", x => x.UpperLimitPrice, NpgsqlTypes.NpgsqlDbType.Numeric)
                .MapNullable("upper_price", x => x.UpperPrice, NpgsqlTypes.NpgsqlDbType.Numeric)
                .MapBoolean("straight", x => x.Straight)
                .MapBigInt("sequence_number", x => x.SequenceNumber);
        }
    }
}
