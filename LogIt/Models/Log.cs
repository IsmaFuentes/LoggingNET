﻿using System;

namespace LogIt.Models
{
    public class Log
    {
        public Log(string message, LogLevel level, Exception innerException)
        {
            this._id = Guid.NewGuid();
            this._message = message;

            if(level == LogLevel.Error)
            {
                if(innerException == null)
                {
                    throw new ArgumentNullException("innerException", "The exception parameter cannot be null when LogLevel equals 'LogLevel.Error'");
                }

                this._stackTrace = innerException.StackTrace;
                this._errorMessage = innerException.Message;
                this._source = innerException.Source;
            }
        }

        private Guid _id;
        public Guid Id { get { return _id; } }

        private string _message;
        public string Message 
        { 
            get { return _message; } 
            set {  _message = value; } 
        }

        private LogLevel _level;
        public LogLevel Level 
        {
            get { return _level; }
            set { _level = value; }
        }

        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        private string _source;
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        private string _stackTrace;
        public string StackTrace
        {
            get { return _stackTrace; }
            set { _stackTrace = value; }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
    }
}