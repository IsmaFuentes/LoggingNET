using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using LogIt.Models;
using System.Linq;

namespace LogIt.Data
{
    public class JsonDataSource : IDisposable
    {
        private List<Log> source { get; set; }
        private int size { get; set; }
        private string sourcePath { get; set; }

        /// <summary>
        /// Ms docs: https://learn.microsoft.com/es-es/dotnet/api/system.threading.semaphore?view=net-7.0
        /// </summary>
        private Semaphore queue { get; set; }
        private List<Task> queuedTasks { get; set; }

        public JsonDataSource(string jsonPath, int size)
        {
            if (!File.Exists(jsonPath))
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
                this.source = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Log>>(fileContent);
            }

            this.queue = new Semaphore(1, 1);
            this.queuedTasks = new List<Task>();
        }

        public void Dispose() 
        {
            if(source != null) 
            {
                source.Clear();
                source = null;
            }

            if (queuedTasks != null)
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

            File.WriteAllText(sourcePath, Newtonsoft.Json.JsonConvert.SerializeObject(source));
        }

        public Task AppendAsync(Log item)
        {
            queue.WaitOne();

#if DEBUG
            Console.WriteLine("Queueing...");
#endif
            var task = Task.Run(() =>
            {
                Append(item);

#if DEBUG
                Console.WriteLine("Realeasing...");
#endif
                queue.Release();
            });

            queuedTasks.Add(task);

            return task;
        }

        public IEnumerable<Log> Enumerate()
        {
            if(queuedTasks.Count > 0)
            {
#if DEBUG
                Console.WriteLine("waiting for the queue to finish");
#endif
                while (!queuedTasks.All(task => task.IsCompleted)) 
                {
                    Thread.Sleep(10);
                }

                queuedTasks.Clear();
            }

            foreach(var item in source)
            {
                yield return item;
            }
        }
    }
}
