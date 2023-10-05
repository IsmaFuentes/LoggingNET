using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogIt.Data;
using LogIt.Interfaces;
using LogIt.Models;

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
        if(DataSource.GetType() == typeof(JsonDataSource))
        {
          ((JsonDataSource)DataSource).Dispose();
        }
        if(DataSource.GetType() == typeof(MSQLDataSource))
        {
          ((MSQLDataSource)DataSource).Dispose();
        }

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

      // TODO: Refactor (with GenericDatasource object inheritance...)
      if(this.DataSource.GetType() == typeof(JsonDataSource))
      {
        ((JsonDataSource)DataSource).Append(new Log(message, level, exception));
      }

      if(this.DataSource.GetType() == typeof(MSQLDataSource))
      {
        ((MSQLDataSource)DataSource).Append(new Log(message, level, exception));
      }
    }

    public Task CreateLogAsync(string message, LogLevel level, Exception exception = null)
    {
      if(level == LogLevel.Error && exception == null)
      {
        throw new ArgumentNullException("exception");
      }


      // TODO: Refactor (with GenericDatasource object inheritance...)
      if(this.DataSource.GetType() == typeof(JsonDataSource))
      {
        return ((JsonDataSource)DataSource).AppendAsync(new Log(message, level, exception));
      }

      if(this.DataSource.GetType() == typeof(MSQLDataSource))
      {
        return ((MSQLDataSource)DataSource).AppendAsync(new Log(message, level, exception));
      }

      return null;
    }

    public IEnumerable<Log> GetLogList(int max)
    {
      // TODO: Refactor (with GenericDatasource object inheritance...)

      if(this.DataSource.GetType() == typeof(JsonDataSource))
      {
        return ((JsonDataSource)DataSource).Enumerate(max);
      }

      if(this.DataSource.GetType() == typeof(MSQLDataSource))
      {
        return ((MSQLDataSource)DataSource).Enumerate(max);
      }

      return new List<Log>();
    }

    public IEnumerable<Log> GetLogList()
    {
      // TODO: Refactor (with GenericDatasource object inheritance...)

      if(this.DataSource.GetType() == typeof(JsonDataSource))
      {
        return ((JsonDataSource)DataSource).Enumerate();
      }

      if(this.DataSource.GetType() == typeof(MSQLDataSource))
      {
        return ((MSQLDataSource)DataSource).Enumerate();
      }

      return new List<Log>();
    }
  }
}
