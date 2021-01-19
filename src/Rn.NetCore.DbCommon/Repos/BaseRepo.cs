using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Metrics;
using Rn.NetCore.Common.Metrics.Builders;
using Rn.NetCore.DbCommon.Helpers;
using Rn.NetCore.DbCommon.Models;

namespace Rn.NetCore.DbCommon.Repos
{
  public abstract class BaseRepo<TRepo>
  {
    public string RepoName { get; }
    public string ConnectionName { get; }
    public ILoggerAdapter<TRepo> Logger { get; }
    public IDbHelper DbHelper { get; }
    public IMetricService Metrics { get; }


    // Constructor
    protected BaseRepo(
      ILoggerAdapter<TRepo> logger,
      IDbHelper dbHelper,
      IMetricService metrics,
      string repoName,
      string connectionName)
    {
      // TODO: [TESTS] (BaseRepo.BaseRepo) Add tests
      Logger = logger;
      DbHelper = dbHelper;
      Metrics = metrics;
      RepoName = repoName;
      ConnectionName = connectionName;
    }


    // Public methods
    protected async Task<List<T>> GetList<T>(string methodName, string sql, object param = null)
    {
      // TODO: [TESTS] (BaseRepo.GetList) Add tests
      var builder = new RepoMetricBuilder(RepoName, methodName, nameof(GetList))
        .ForConnection(ConnectionName)
        .WithParameters(param)
        .WithCustomTag1(typeof(T).Name, true);

      try
      {
        LogSqlCommand(methodName, sql, param);

        using (builder.WithTiming())
        {
          using var connection = DbHelper.GetConnection(ConnectionName);

          IEnumerable<T> results;
          using (builder.WithCustomTiming1())
            results = await connection.QueryAsync<T>(sql, param);

          List<T> resultList;
          using (builder.WithCustomTiming2())
            resultList = results.ToList();

          builder.WithResultCount(resultList.Count);
          return resultList;
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error running SQL query: {sql}", sql);
        builder.WithException(ex);
        return new List<T>();
      }
      finally
      {
        await Metrics.SubmitPointAsync(builder.Build());
      }
    }

    protected async Task<T> GetSingle<T>(string methodName, string sql, object param = null)
    {
      // TODO: [TESTS] (BaseRepo.GetSingle) Add tests
      var builder = new RepoMetricBuilder(RepoName, methodName, nameof(GetSingle))
        .ForConnection(ConnectionName)
        .WithParameters(param)
        .WithCustomTag1(typeof(T).Name, true);

      try
      {
        LogSqlCommand(methodName, sql, param);

        using (builder.WithTiming())
        {
          using var connection = DbHelper.GetConnection(ConnectionName);

          IEnumerable<T> results;
          using (builder.WithCustomTiming1())
            results = await connection.QueryAsync<T>(sql, param);

          var actualResult = results.FirstOrDefault();
          builder.CountResult(actualResult);

          return actualResult;
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error running SQL query: {sql}", sql);
        builder.WithException(ex);
        return default;
      }
      finally
      {
        await Metrics.SubmitPointAsync(builder.Build());
      }
    }

    protected async Task<int> ExecuteAsync(string methodName, string sql, object param = null)
    {
      // TODO: [TESTS] (BaseRepo.ExecuteAsync) Add tests
      var builder = new RepoMetricBuilder(RepoName, methodName, nameof(ExecuteAsync))
        .ForConnection(ConnectionName)
        .WithParameters(param)
        .WithCustomTag1(nameof(Int32), true);

      if (param != null)
        builder.WithCustomTag1(param.GetType().Name);

      try
      {
        LogSqlCommand(methodName, sql, param);

        using (builder.WithTiming())
        {
          using var connection = DbHelper.GetConnection(ConnectionName);

          int numRows;
          using (builder.WithCustomTiming1())
            numRows = await connection.ExecuteAsync(sql, param);

          builder.WithResultCount(numRows);

          return numRows;
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error running SQL query: {sql}", sql);
        builder.WithException(ex);
        return 0;
      }
      finally
      {
        await Metrics.SubmitPointAsync(builder.Build());
      }
    }

    protected async Task<DatabaseRow> SingleRowProcedure(string methodName, string procName, object param = null, string connection = null)
    {
      // TODO: [TESTS] (BaseRepo.SingleRowProcedure) Add tests
      if (string.IsNullOrWhiteSpace(connection))
        connection = ConnectionName;

      var builder = new RepoMetricBuilder(RepoName, methodName, nameof(SingleRowProcedure))
        .ForConnection(connection)
        .WithParameters(param)
        .WithCustomTag1(nameof(DatabaseRow), true)
        .WithCustomTag2(procName);

      try
      {
        // TODO: [LOGGING] (BaseRepo.SingleRowProcedure) Log command

        using (builder.WithTiming())
        {
          var row = await DbHelper
            .GetProcedureHelper(connection)
            .ForProcedure(procName)
            .WithParams(param)
            .ExecuteAsRow();

          builder.WithResultCount(row.ValidRow ? 1 : 0);
          return row;
        }
      }
      catch (Exception ex)
      {
        Logger.LogUnexpectedException(ex);
        builder.WithException(ex);
      }
      finally
      {
        await Metrics.SubmitPointAsync(builder.Build());
      }

      return new DatabaseRow();
    }

    protected async Task<int> ExecuteProcedureAsync(string methodName, string procName, object param = null, string connection = null)
    {
      // TODO: [TESTS] (BaseRepo.ExecuteProcedureAsync) Add tests
      if (string.IsNullOrWhiteSpace(connection))
        connection = ConnectionName;

      var builder = new RepoMetricBuilder(RepoName, methodName, nameof(ExecuteProcedureAsync))
        .ForConnection(connection)
        .WithParameters(param)
        .WithCustomTag1(nameof(Int32), true)
        .WithCustomTag2(procName);

      try
      {
        // TODO: [LOGGING] (BaseRepo.ExecuteProcedureAsync) Log command

        using (builder.WithTiming())
        {
          var rowCount = await DbHelper
            .GetProcedureHelper(connection)
            .ForProcedure(procName)
            .WithParams(param)
            .ExecuteAsync();

          builder.WithResultCount(rowCount);
          return rowCount;
        }
      }
      catch (Exception ex)
      {
        Logger.LogUnexpectedException(ex);
        builder.WithException(ex);
      }
      finally
      {
        await Metrics.SubmitPointAsync(builder.Build());
      }

      return 0;
    }

    protected async Task<DatabaseRow> SingleRowQuery(string methodName, string sql, object param = null, string connection = null)
    {
      // TODO: [TESTS] (BaseRepo.SingleRowQuery) Add tests
      if (string.IsNullOrWhiteSpace(connection))
        connection = ConnectionName;

      var builder = new RepoMetricBuilder(RepoName, methodName, nameof(SingleRowQuery))
        .ForConnection(connection)
        .WithParameters(param)
        .WithCustomTag1(nameof(DatabaseRow), true)
        .WithCustomTag2("RawSQL", true);

      try
      {
        // TODO: [LOGGING] (BaseRepo.SingleRowQuery) Log command

        using (builder.WithTiming())
        {
          var queryResult = (await DbHelper
              .GetConnection(connection)
              .QueryAsync<object>(sql, param)
            ).AsList();

          builder.WithResultCount(queryResult.Count);
          return new DatabaseRow(queryResult.First());
        }
      }
      catch (Exception ex)
      {
        Logger.LogUnexpectedException(ex);
        builder.WithException(ex);
      }
      finally
      {
        await Metrics.SubmitPointAsync(builder.Build());
      }

      return new DatabaseRow();
    }

    // Internal methods
    public void LogSqlCommand(string methodName, string sql, object param = null)
    {
      // TODO: [TESTS] (BaseRepo.LogSqlCommand) Add tests
      // TODO: [METRICS] (BaseRepo.LogSqlCommand) Add metrics
      // TODO: [CONFIG] (BaseRepo.LogSqlCommand) Make configurable

      return;

      try
      {
        Logger.Info(
          "[{repo}.{method}] Running SQL command: {sql}",
          RepoName, methodName,
          SqlFormatter.MergeParams(sql, param)
        );
      }
      catch (Exception)
      {
        // Swallow
      }
    }
  }
}
