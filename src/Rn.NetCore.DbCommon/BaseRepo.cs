using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Metrics;

namespace Rn.NetCore.DbCommon
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
      // TODO: [METRICS] (BaseRepo.GetList) Add metrics

      LogSqlCommand(methodName, sql, param);

      try
      {
        using var connection = DbHelper.GetConnection(ConnectionName);
        var results = await connection.QueryAsync<T>(sql, param);
        var resultList = results.ToList();
        return resultList;
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error running SQL query: {sql}", sql);
        return new List<T>();
      }
    }

    protected async Task<T> GetSingle<T>(string methodName, string sql, object param = null)
    {
      // TODO: [TESTS] (BaseRepo.GetSingle) Add tests
      // TODO: [METRICS] (BaseRepo.GetSingle) Add metrics
      LogSqlCommand(methodName, sql, param);

      try
      {
        using var connection = DbHelper.GetConnection(ConnectionName);
        var results = await connection.QueryAsync<T>(sql, param);
        var actualResult = results.FirstOrDefault();
        return actualResult;
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error running SQL query: {sql}", sql);
        return default(T);
      }
    }

    protected async Task<int> ExecuteAsync(string methodName, string sql, object param = null)
    {
      // TODO: [TESTS] (BaseRepo.ExecuteAsync) Add tests
      // TODO: [METRICS] (BaseRepo.ExecuteAsync) Add metrics

      LogSqlCommand(methodName, sql, param);

      try
      {
        using var connection = DbHelper.GetConnection(ConnectionName);
        var numRows = await connection.ExecuteAsync(sql, param);
        return numRows;
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error running SQL query: {sql}", sql);
        return 0;
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
