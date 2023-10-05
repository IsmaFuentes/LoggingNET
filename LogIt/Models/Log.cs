using System;

namespace LogIt.Models
{
  public class Log
  {
    public Log(string message, LogLevel level, Exception innerException)
    {
      this._id = Guid.NewGuid();
      this._message = message?.Trim();
      this.Date = DateTime.Now;
      this.Level = level;

      if(level == LogLevel.Error && innerException != null)
      {
        this._stackTrace = innerException.StackTrace?.Trim();
        this._errorMessage = innerException.Message?.Trim();
        this._source = innerException.Source?.Trim();
      }
    }

    private Guid _id;
    public Guid Id { get { return _id; } }

    private string _message;
    public string Message
    {
      get { return _message ?? string.Empty; }
      set { _message = value; }
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
      get { return _source ?? string.Empty; }
      set { _source = value; }
    }

    private string _stackTrace;
    public string StackTrace
    {
      get { return _stackTrace ?? string.Empty; }
      set { _stackTrace = value; }
    }

    private string _errorMessage;
    public string ErrorMessage
    {
      get { return _errorMessage ?? string.Empty; }
      set { _errorMessage = value; }
    }
  }
}
