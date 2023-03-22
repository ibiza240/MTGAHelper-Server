﻿using System;
using System.Data;

namespace MTGAHelper.Entity
{
    [Serializable]
    public class InfoByDate<T>
    {
        public DateTime DateTime { get; set; }
        public T Info { get; set; }

        public InfoByDate()
        {
        }

        public InfoByDate(DateTime datetime, T info)
        {
            DateTime = datetime;
            Info = info;
        }
    }

}
