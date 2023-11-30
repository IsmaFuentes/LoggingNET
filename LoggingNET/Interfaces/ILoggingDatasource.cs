using System.Collections.Generic;
using System.Threading.Tasks;
using LoggingNET.Models;

namespace LoggingNET.Interfaces
{
  public interface ILoggingDatasource
  {
    void Dispose();
    void Append(Log item);
    Task AppendAsync(Log item);
    IEnumerable<Log> Enumerate(int max);
  }
}
