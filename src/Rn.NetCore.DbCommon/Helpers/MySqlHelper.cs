using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;

namespace Rn.NetCore.DbCommon.Helpers
{
  public class MySqlHelper : IDbHelper
  {
    // TODO: [BREAK-OUT] (MySqlHelper) Break out into own lib

    private readonly ILoggerAdapter<MySqlHelper> _logger;
    private readonly IJsonHelper _jsonHelper;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, string> _connectionStrings;

    // Constructor
    public MySqlHelper(
      ILoggerAdapter<MySqlHelper> logger,
      IJsonHelper jsonHelper,
      IConfiguration configuration)
    {
      // TODO: [TESTS] (MySqlHelper.MySqlHelper) Add tests

      _configuration = configuration;
      _jsonHelper = jsonHelper;
      _logger = logger;
      _connectionStrings = new Dictionary<string, string>();

      LoadConnectionStrings();
    }


    // Interface Methods
    public IDbConnection GetConnection(string connection)
    {
      // TODO: [TESTS] (MySqlHelper.GetConnection) Add tests
      var conString = GetConnectionString(connection);

      // Ensure that we have a connection string to work with
      if (conString == null)
      {
        throw new Exception($"Unable to find connection string: {connection}");
      }

      // Create and open the requested connection
      var sqlCon = new MySqlConnection(conString);

      if (sqlCon.State != ConnectionState.Open)
      {
        sqlCon.Open();
      }

      // Return the connection to the caller
      return sqlCon;
    }

    public async Task<int> ExecuteAsync(
        IDbConnection cnn,
        string sql,
        object param = null,
        IDbTransaction transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
      // TODO: [TESTS] (MySqlHelper.ExecuteAsync) Add tests
      // TODO: [REVISE] (MySqlHelper.ExecuteAsync) Revise this code

      try
      {
        return await cnn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
      }
      catch (Exception ex)
      {
        // Generate the base alert message
        var sb = new StringBuilder();
        sb.Append($"Error running SQL command on '{cnn.Database}'.");
        sb.Append(Environment.NewLine + Environment.NewLine);
        sb.Append($"SQL: {sql}");

        // If there was an error passed into the command - serialize and log it
        if (param != null)
        {
          sb.Append(Environment.NewLine + Environment.NewLine);
          // TODO: [ABSTRACT] (MySqlHelper) Abstract this
          var jsonArgs = _jsonHelper.SerializeObject(param, true);
          sb.Append($"Args: {jsonArgs}");
        }

        // Finally append the exception and log it
        sb.Append(Environment.NewLine + Environment.NewLine);
        sb.Append($"Exception: {ex.GetType().Name}: {ex.Message}");
        _logger.Error(sb.ToString(), ex);

        throw;
      }
    }

    public ProcedureHelper GetProcedureHelper(string connection) 
    {
      // TODO: [TESTS] (MySqlHelper.GetProcedureHelper) Add tests
      return new ProcedureHelper(GetConnection(connection)); 
    } 


    // Internal methods
    private void LoadConnectionStrings()
    {
      // TODO: [TESTS] (MySqlHelper.LoadConnectionStrings) Add tests
      var conStrings = _configuration
        .GetSection("ConnectionStrings")
        ?.GetChildren() ?? new IConfigurationSection[0];

      foreach (var connectionString in conStrings)
      {
        _connectionStrings[connectionString.Key] = connectionString.Value;
      }
    }

    private string GetConnectionString(string name)
    {
      // TODO: [TESTS] (MySqlHelper.GetConnectionString) Add tests
      return !_connectionStrings.ContainsKey(name)
        ? null
        : _connectionStrings[name];
    }
  }
}
