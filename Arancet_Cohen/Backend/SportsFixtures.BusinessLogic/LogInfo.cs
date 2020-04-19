using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.BusinessLogic
{
    public class LogInfo
    {
        private int id;
        private string message;
        private string logType;
        private DateTime date;
        private string username;

        public int Id { get { return id; } set { id = value; } }
        public string Message { get { return message; } set { SetMessage(value); } }

        public string LogType { get { return logType; } set { SetLogType(value); } }

        public DateTime Date { get { return date; } set { SetDate(date = value); } }

        public string Username { get { return username; } set { SetUsername(username = value); } }

        public LogInfo() { }

        private void SetMessage(string value)
        {
            message = value;
        }


        private void SetLogType(string value)
        {
            logType = value;
        }

        private void SetDate(DateTime dateTime)
        {
            date = dateTime;
        }

        private void SetUsername(string value)
        {
            username = value;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            LogInfo log = (LogInfo)obj;
            return id == log.id;
        }

    }
}
