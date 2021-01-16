using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Metrics;
using Rn.NetCore.Common.Metrics.Builders;
using Rn.NetCore.DbCommon.Helpers;

namespace Rn.NetCore.DbCommon.Repos
{
  public abstract class BaseRepo<TRepo>
  {
    public string RepoName { get; }
    public string ConnectionName { get; }
    public ILoggerAdapter<TRepo> Logger { get; }
    public IDbHelper DbHelper { get; }
    public IMetricService MetricService { get; }


    // Constructor
    protected BaseRepo(
      ILoggerAdapter<TRepo> logger,
      IDbHelper dbHelper,
      IMetricService metricService,
      string repoName,
      string connectionName)
    {
      // TODO: [TESTS] (BaseRepo.BaseRepo) Add tests
      Logger = logger;
      DbHelper = dbHelper;
      MetricService = metricService;
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
        .WithCustomTag1(typeof(T).Name);

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
        await MetricService.SubmitPointAsync(builder.Build());
      }
    }

    protected async Task<T> GetSingle<T>(string methodName, string sql, object param = null)
    {
      // TODO: [TESTS] (BaseRepo.GetSingle) Add tests
      var builder = new RepoMetricBuilder(RepoName, methodName, nameof(GetSingle))
        .ForConnection(ConnectionName)
        .WithParameters(param)
        .WithCustomTag1(typeof(T).Name);

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
        await MetricService.SubmitPointAsync(builder.Build());
      }
    }

    protected async Task<int> ExecuteAsync(string methodName, string sql, object param = null)
    {
      // TODO: [TESTS] (BaseRepo.ExecuteAsync) Add tests
      var builder = new RepoMetricBuilder(RepoName, methodName, nameof(ExecuteAsync))
        .ForConnection(ConnectionName)
        .WithParameters(param);

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
        await MetricService.SubmitPointAsync(builder.Build());
      }
    }

    // Internal methods
    public void LogSqlCommand(string methodName, string sql, object param = null)
    {
      // TODO: [TESTS] (BaseRepo.LogSqlCommand) Add tests
      // TODO: [METRICS] (BaseRepo.LogSqlCommand) Add metrics
      // TODO: [CONFIG] (BaseRepo.LogSqlCommand) Make configurable

      Logger.Info(
        "[{repo}.{method}] Running SQL command: {sql}",
        RepoName, methodName,
        SqlFormatter.MergeParams(sql, param)
      );
    }
  }
}
