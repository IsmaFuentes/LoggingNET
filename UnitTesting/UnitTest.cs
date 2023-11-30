using LogIt;

namespace UnitTesting
{
  public class Tests
  {
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test_JsonDataSource()
    {
      using(var logger = new Logger(useEncryption: true))
      {
        try
        {
          logger.CreateLogAsync("Information log", LogLevel.Information);

          throw new Exception("Unhandled exception");
        } 
        catch(Exception ex)
        {
          logger.CreateLogAsync("Error log", LogLevel.Error, ex);
        }

        var logs = logger.GetLogList(10);
        Assert.True(logs.Count() > 0);
        var l1 = logs.Where(log => log.Level == LogLevel.Information).FirstOrDefault();
        Assert.NotNull(l1);
        var l2 = logs.Where(log => log.Level == LogLevel.Error).FirstOrDefault();
        Assert.NotNull(l2);
      }

      Assert.Pass();
    }

    [Test]
    public void Test_JsonMslqDatasource()
    {
      using(var logger = new Logger("data source=(local);integrated security=SSPI;initial catalog=TestingDB;TrustServerCertificate=True"))
      {
        try
        {
          logger.CreateLogAsync("Information log", LogLevel.Information);

          throw new Exception("Unhandled exception");
        }
        catch(Exception ex)
        {
          logger.CreateLogAsync("Error log", LogLevel.Error, ex);
        }

        var logs = logger.GetLogList(1000);
        Assert.True(logs.Count() > 0);
        var l1 = logs.Where(log => log.Level == LogLevel.Information).FirstOrDefault();
        Assert.NotNull(l1);
        var l2 = logs.Where(log => log.Level == LogLevel.Error).FirstOrDefault();
        Assert.NotNull(l2);
      }

      Assert.Pass();
    }
  }
}