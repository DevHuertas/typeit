using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace TypeItWebRole
{
    /// <summary>
    /// Class to wrap the data coming from our Azure table
    /// </summary>
    public class UserStatEntry : TableEntity
    {
        public string TargetWord {get; set;}
        public int Duration { get; set; }
        public bool Panic { get; set; }
        public bool Bored { get; set; }
        public int InitialDelay { get; set; }
        public List<MissedLetter> Missed { get; set; }
    }
}