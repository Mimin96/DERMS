﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Model
{
    public class SignalsSummaryFilter
    {
        public SignalsSummaryFilter()
        {
            FilterByTime = false;
            SelectedFilterType = "";
            SelectedFilterAlarm = "";
            Name = "Name";
            Address = "Address";
            Value = "Value";
            RawValue = "Raw Value";
            GID = "GID";
            From = DateTime.Today.AddDays(-1);
            To = DateTime.Today.AddDays(1);
        }

        public bool FilterByTime
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public string Address
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }
        public string RawValue
        {
            get;
            set;
        }
        public string SelectedFilterType
        {
            get;
            set;
        }
        public string SelectedFilterAlarm
        {
            get;
            set;
        }
        public DateTime From
        {
            get;
            set;
        }
        public DateTime To
        {
            get;
            set;
        }
        public string GID
        {
            get;
            set;
        }
    }
}