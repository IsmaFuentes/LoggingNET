﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using LoggingNET.Models;
using LoggingNET.Interfaces;
using System.Linq;

namespace LoggingNET.Data
{
  public class MSQLDataSource : IDisposable, ILoggingDatasource
  {
    public MSQLDataSource(string connectionString, string tableName = "LoggerTable")
    {
      this.connectionString = connectionString;
      this.tableName = tableName;
      this.queuedTasks = new List<Task>();
      this.queue = new Semaphore(1, 1);

      this.CreateTableIfNotExists(tableName);
    }

    private Semaphore queue { get; set; }
    private List<Task> queuedTasks { get; set; }
    private string connectionString { get; set; }
    private string tableName { get; set; }

    public void Dispose()
    {
      if(queuedTasks != null)
      {
        queuedTasks.Clear();
        queuedTasks = null;
      }
    }

    private void CreateTableIfNotExists(string tableName)
    {
      using(var sqlConnection = new SqlConnection(connectionString))
      {
        sqlConnection.Open();

        string query = $"SELECT CASE WHEN exists((SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}')) THEN 1 ELSE 0 END;";

        using(var command = new SqlCommand(query, sqlConnection))
        {
          if(int.Parse(command.ExecuteScalar().ToString()) == 0)
          {
            string createTableQuery = 
              $"CREATE TABLE {tableName} (" +
               "Id Uniqueidentifier NOT NULL," +
               "Message VARCHAR(200) NOT NULL," +
               "Level INT NOT NULL," +
               "Date Datetime NOT NULL," +
               "UserName VARCHAR(25)," +
               "MachineName VARCHAR(25)," +
               "Source VARCHAR(250)," +
               "StackTrace VARCHAR(250)," +
               "ErrorMessage VARCHAR(250)" +
               ")";

            command.CommandText = createTableQuery;
            command.ExecuteNonQuery();
          }
        }
      }
    }

    public void Append(Log item)
    {
      using(SqlConnection sqlConnection = new SqlConnection(connectionString))
      {
        sqlConnection.Open();

        string query = $"INSERT INTO {tableName} VALUES(@Id, @Message, @Level, @Date, @UserName, @MachineName, @Source, @StackTrace, @ErrorMessage)";

        using(var command = new SqlCommand(query, sqlConnection))
        {
          command.Parameters.AddWithValue("@Id", item.Id);
          command.Parameters.AddWithValue("@Message", item.Message);
          command.Parameters.AddWithValue("@Level", item.Level);
          command.Parameters.AddWithValue("@Date", item.Date);
          command.Parameters.AddWithValue("@UserName", item.UserName);
          command.Parameters.AddWithValue("@MachineName", item.MachineName);
          command.Parameters.AddWithValue("@Source", item.Source);
          command.Parameters.AddWithValue("@StackTrace", item.StackTrace);
          command.Parameters.AddWithValue("@ErrorMessage", item.ErrorMessage);
          command.ExecuteNonQuery();
        }
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
        catch(Exception ex)
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

      using(SqlConnection sqlConnection = new SqlConnection(connectionString))
      {
        sqlConnection.Open();

        string query = $"SELECT TOP ({max}) * FROM {tableName} ORDER BY Date DESC";

        using(var command = new SqlCommand(query, sqlConnection))
        {
          using(var reader = command.ExecuteReader())
          {
            while(reader.Read())
            {
              yield return new Log()
              {
                Message = reader.GetString(1),
                Level = (LogLevel)reader.GetInt32(2),
                Date = reader.GetDateTime(3),
                UserName = reader.GetString(4),
                MachineName = reader.GetString(5),
                Source = reader.GetString(6),
                StackTrace  = reader.GetString(7),
                ErrorMessage = reader.GetString(8),
              };
            }
          }
        }
      }
    }
  }
}
