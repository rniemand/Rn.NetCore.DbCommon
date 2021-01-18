using System.Data;
using System.Threading.Tasks;
using Dapper;
using Rn.NetCore.DbCommon.Models;

namespace Rn.NetCore.DbCommon.Helpers
{
  public class ProcedureHelper
  {
    private readonly IDbConnection _connection;
    private string _spName;
    private object _param;

    public ProcedureHelper(IDbConnection connection)
    {
      // TODO: [TESTS] (ProcedureHelper.ProcedureHelper) Add tests
      _connection = connection;
      _spName = string.Empty;
      _param = null;
    }

    public ProcedureHelper ForProcedure(string procName)
    {
      // TODO: [TESTS] (ProcedureHelper.ForProcedure) Add tests
      _spName = procName;
      return this;
    }

    public ProcedureHelper WithParams(object param = null)
    {
      // TODO: [TESTS] (ProcedureHelper.WithParams) Add tests
      _param = param;
      return this;
    }

    public async Task<DatabaseRow> ExecuteAsRow()
    {
      // TODO: [TESTS] (ProcedureHelper.ExecuteAsRow) Add tests
      return new DatabaseRow(await _connection.QueryAsync<object>(
        _spName,
        _param,
        commandType: CommandType.StoredProcedure
      ));
    }

    public async Task<int> ExecuteAsync()
    {
      // TODO: [TESTS] (ProcedureHelper.ExecuteAsync) Add tests
      return await _connection.ExecuteAsync(
        _spName,
        _param,
        commandType: CommandType.StoredProcedure
      );
    }
  }
}
