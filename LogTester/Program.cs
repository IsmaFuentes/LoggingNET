using LogIt;

namespace LogTester
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateErrorLogsAsync();

      Console.ReadLine();
    }

    public static void CreateErrorLogsAsync()
    {
      Console.WriteLine("Creating logger instance...");

      using(var logger = new Logger(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}", 100))
      {
        int size = 100;
        while(--size >= 0)
        {
          try
          {
            var a = new int[2];
            var b = a[5];
          }
          catch(Exception e)
          {
            logger.CreateLogAsync($"Asynchronous log", LogLevel.Error, e);
          }
        }

        var logs = logger.GetLogList();

        Console.Write("Total logs: " + logs.Count());
      }
    }

    public static void CreateInformationLogsAsync()
    {
      Console.WriteLine("Creating logger instance...");

      using(var logger = new Logger(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\source.log", 100))
      {
        int size = 100;
        while(--size >= 0)
        {
          logger.CreateLogAsync($"Asynchronous log", LogLevel.Information);
        }

        var logs = logger.GetLogList();

        Console.Write("Total logs: " + logs.Count());
      }
    }

    public static void EnumerateLogs()
    {
      using(var logger = new Logger(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\source.log", 100))
      {

        var logs = logger.GetLogList();

        foreach(var log in logs)
        {
          Console.WriteLine($"{log.Id} - {log.Message}");
        }

        Console.Write("Total logs: " + logs.Count());
      }
    }
  }
}