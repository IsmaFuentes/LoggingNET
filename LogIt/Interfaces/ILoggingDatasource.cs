using System.Collections.Generic;
using System.Threading.Tasks;
using LogIt.Models;

namespace LogIt.Interfaces
{
  public interface ILoggingDatasource
  {
    void Append(Log item);
    Task AppendAsync(Log item);
    IEnumerable<Log> Enumerate(int max);
  }
}
