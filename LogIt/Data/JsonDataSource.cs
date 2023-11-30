using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogIt.Models;
using LogIt.Interfaces;
using LogIt.Cryptography;

namespace LogIt.Data
{
  public class JsonDataSource : IDisposable, ILoggingDatasource
  {
    public JsonDataSource(string jsonPath, int size = 100, bool useEncryption = false)
    {
      if(!File.Exists(jsonPath))
      {
        File.Create(jsonPath).Close();
      }

      this.size = size;
      this.sourcePath = jsonPath;
      this.useEncryption = useEncryption;
      this.queue = new Semaphore(1, 1);
      this.queuedTasks = new List<Task>();

      string fileContent = File.ReadAllText(jsonPath);

      if(string.IsNullOrEmpty(fileContent) || string.IsNullOrWhiteSpace(fileContent))
      {
        this.source = new List<Log>();
      }
      else
      {
        if(useEncryption)
        {
          fileContent = Cipher.Decrypt(jsonPath);
        }

        this.source = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Log>>(fileContent) ?? new List<Log>();
      }
    }

    private Semaphore queue { get; set; }
    private List<Task> queuedTasks { get; set; }
    private List<Log> source { get; set; }
    private int size { get; set; }
    private string sourcePath { get; set; }
    private bool useEncryption { get; set; }

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

      string fileContent = Newtonsoft.Json.JsonConvert.SerializeObject(source, Newtonsoft.Json.Formatting.Indented);

      if(useEncryption)
      {
        Cipher.Encrypt(fileContent, sourcePath);
      }
      else
      {
        File.WriteAllText(sourcePath, fileContent);
      }
    }

    public Task AppendAsync(Log item)
    {
      queue.WaitOne();

      var task = Task.Run(() =>
      {
        try
        {
          Append(item);
        } 
        catch (Exception ex)
        {
          throw ex;
        }
        finally
        {
          queue.Release();
        }
      });

      queuedTasks.Add(task);

      return task;
    }

    public IEnumerable<Log> Enumerate(int max = 100)
    {
      if(queuedTasks.Any())
      {
        try
        {
          Task.WaitAll(queuedTasks.ToArray());
        } 
        catch(Exception ex)
        {
#if DEBUG
          if(ex is AggregateException aggreggatedException)
          {
            foreach(var innerException in aggreggatedException.InnerExceptions)
            {
              System.Diagnostics.Debug.WriteLine(innerException.Message);
            }
          }
#endif
          throw;
        }
        finally
        {
          queuedTasks.Clear();
        }
      }

      return source.Take(max);
    }
  }
}
