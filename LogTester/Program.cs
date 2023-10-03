using LogIt;

namespace LogTester
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Creating logger instance...");

            using(var logger = new Logger(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\source.txt", 100))
            {
                int size = 100;
                while (--size > 0)
                {
                    logger.CreateLogAsync($"Asynchronous log", LogLevel.Information, null);
                }

                var logs = logger.GetLogList();

                Console.Write("Total logs: " + logs.Count());
            }

            Console.ReadLine();
        }
    }
}