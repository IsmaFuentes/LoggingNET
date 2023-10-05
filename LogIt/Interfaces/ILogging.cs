using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogIt.Models;

namespace LogIt.Interfaces
{
  public interface ILogging
  {
    void Configure(string filePath, int logLimit = 1000);
    void Configure(string connectionString);
    Task CreateLogAsync(string message, LogLevel level, Exception exception);
    void CreateLog(string message, LogLevel level, Exception exception);
    IEnumerable<Log> GetLogList(int records);
  }
}
