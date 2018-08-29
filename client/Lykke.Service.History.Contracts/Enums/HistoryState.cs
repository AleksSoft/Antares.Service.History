﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.History.Contracts.Enums
{
    /// <summary>
    /// History record state
    /// </summary>
    public enum HistoryState
    {
        None,
        InProgress,
        Finished,
        Canceled,
        Failed
    }
}
