using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using LoggingNET.Data;
using LoggingNET.Models;
using LoggingNET.Interfaces;

namespace LoggingNET
{
  public class Logger : ILogging, IDisposable
  {
    private ILoggingDatasource DataSource { get; set; }

    public Logger(int logLimit = 100, bool useEncryption = false)
    {
      string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Logger");

      if(!Directory.Exists(defaultPath))
      {
        Directory.CreateDirectory(defaultPath);
      }

      this.Configure(Path.Combine(defaultPath, "source.log"), logLimit, useEncryption);
    }

    public Logger(string connectionString)
    {
      this.Configure(connectionString);
    }

    public void Dispose()
    {
      if(DataSource != null)
      {
        DataSource.Dispose();
        DataSource = null;
      }
    }

    public void Configure(string filePath, int logLimit, bool useEncryption)
    {
      DataSource = new JsonDataSource(filePath, logLimit, useEncryption);
    }

    public void Configure(string connectionString)
    {
      DataSource = new MSQLDataSource(connectionString);
    }

    public void CreateLog(string message, LogLevel level, Exception exception = null)
    {
      if(level == LogLevel.Error && exception == null)
      {
        throw new ArgumentNullException("exception");
      }

      DataSource.Append(new Log(message, level, exception));
    }

    public Task CreateLogAsync(string message, LogLevel level, Exception exception = null)
    {
      if(level == LogLevel.Error && exception == null)
      {
        throw new ArgumentNullException("exception");
      }

      return DataSource.AppendAsync(new Log(message, level, exception));
    }

    public IEnumerable<Log> GetLogList(int max)
    {
      return DataSource.Enumerate(max);
    }
  }
}
