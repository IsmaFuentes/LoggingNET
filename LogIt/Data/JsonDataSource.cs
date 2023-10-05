using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using LogIt.Models;
using LogIt.Interfaces;

namespace LogIt.Data
{
  public class JsonDataSource : IDisposable, ILoggingDatasource
  {
    public JsonDataSource(string jsonPath, int size)
    {
      if(Directory.Exists(jsonPath))
      {
        jsonPath = Path.Combine(jsonPath, "source.log");
      }

      if(!File.Exists(jsonPath))
      {
        File.Create(jsonPath).Close();
      }

      this.size = size;
      this.sourcePath = jsonPath;

      string fileContent = File.ReadAllText(jsonPath);

      if(string.IsNullOrEmpty(fileContent) || string.IsNullOrWhiteSpace(fileContent))
      {
        this.source = new List<Log>();
      }
      else
      {
        this.source = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Log>>(fileContent) ?? new List<Log>();
      }

      this.queue = new Semaphore(1, 1);
      this.queuedTasks = new List<Task>();
    }

    /// <summary>
    /// Ms docs: https://learn.microsoft.com/es-es/dotnet/api/system.threading.semaphore?view=net-7.0
    /// </summary>
    private Semaphore queue { get; set; }
    private List<Task> queuedTasks { get; set; }
    private List<Log> source { get; set; }
    private int size { get; set; }
    private string sourcePath { get; set; }

    public void Dispose()
    {
      if(source != null)
      {
        source.Clear();
        source = null;
      }

      if(queuedTasks != null)
      {
        queuedTasks.Clear();
        queuedTasks = null;
      }
    }

    public void Append(Log item)
    {
      if(source.Count == size)
      {
        source.RemoveAt(0);
      }

      source.Add(item);

      File.WriteAllText(sourcePath, Newtonsoft.Json.JsonConvert.SerializeObject(source, Newtonsoft.Json.Formatting.Indented));
    }

    public Task AppendAsync(Log item)
    {
      queue.WaitOne();

#if DEBUG
      Console.WriteLine("Queueing...");
#endif
      var task = Task.Run(() =>
      {
        try
        {
          Append(item);

#if DEBUG
          Console.WriteLine("Releasing...");
#endif
          queue.Release();
        } 
        catch (Exception ex)
        {
#if DEBUG
          Console.WriteLine(ex.Message);
#endif
          queue.Release();
        }
      });

      queuedTasks.Add(task);

      return task;
    }

    public IEnumerable<Log> Enumerate(int max = 100)
    {
      if(queuedTasks.Count > 0)
      {
#if DEBUG
        Console.WriteLine("Waiting for queued tasks to finalize");
#endif
        Task.WaitAll(queuedTasks.ToArray());

        queuedTasks.Clear();
      }

      return source;
    }
  }
}
