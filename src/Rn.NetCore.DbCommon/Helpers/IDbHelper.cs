using System.Data;
using System.Threading.Tasks;

namespace Rn.NetCore.DbCommon.Helpers
{
  public interface IDbHelper
  {
    IDbConnection GetConnection(string connection);

    Task<int> ExecuteAsync(IDbConnection cnn,
      string sql,
      object param = null,
      IDbTransaction transaction = null,
      int? commandTimeout = null,
      CommandType? commandType = null);

    ProcedureHelper GetProcedureHelper(string connection);
  }
}