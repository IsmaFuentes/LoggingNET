using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogIt.Data;
using LogIt.Models;
using LogIt.Interfaces;

namespace LogIt
{
  public class Logger : ILogging, IDisposable
  {
    private object DataSource { get; set; }

    public Logger(string filePath, int logLimit = 1000)
    {
      this.Configure(filePath, logLimit);
    }

    public Logger(string connectionString)
    {
      this.Configure(connectionString);
    }

    public void Dispose()
    {
      if(DataSource != null)
      {
        ((IDisposable)DataSource).Dispose();
        DataSource = null;
      }
    }

    public void Configure(string filePath, int logLimit = 1000)
    {
      this.DataSource = new JsonDataSource(filePath, logLimit);
    }

    public void Configure(string connectionString)
    {
      this.DataSource = new MSQLDataSource(connectionString);
    }

    public void CreateLog(string message, LogLevel level, Exception exception = null)
    {
      if(level == LogLevel.Error && exception == null)
      {
        throw new ArgumentNullException("exception");
      }

      ((ILoggingDatasource)DataSource).Append(new Log(message, level, exception));
    }

    public Task CreateLogAsync(string message, LogLevel level, Exception exception = null)
    {
      if(level == LogLevel.Error && exception == null)
      {
        throw new ArgumentNullException("exception");
      }

      return ((ILoggingDatasource)DataSource).AppendAsync(new Log(message, level, exception));
    }

    public IEnumerable<Log> GetLogList(int max)
    {
      return ((ILoggingDatasource)DataSource).Enumerate(max);
    }
  }
}
