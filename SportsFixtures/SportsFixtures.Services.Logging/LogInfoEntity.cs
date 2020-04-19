using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.Services.Logging
{
    public class LogInfoEntity
    {
        public int Id { get; set; }
        public string Messagge { get; set; }
        public string LogType { get; set; }
        public DateTime Date { get; set; }
        public string Username { get; set; }
    }
}
