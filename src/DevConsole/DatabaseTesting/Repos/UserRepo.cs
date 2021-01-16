using System.Threading.Tasks;
using DevConsole.DatabaseTesting.Entities;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Metrics;
using Rn.NetCore.DbCommon;
using Rn.NetCore.DbCommon.Helpers;
using Rn.NetCore.DbCommon.Repos;

namespace DevConsole.DatabaseTesting.Repos
{
  public interface IUserRepo
  {
    Task<UserEntity> GetUser();
  }

  public class UserRepo : BaseRepo<UserRepo>, IUserRepo
  {
    public UserRepo(
      ILoggerAdapter<UserRepo> logger,
      IDbHelper dbHelper,
      IMetricService metricService)
      : base(logger, dbHelper, metricService, nameof(UserRepo), TargetDatabase.Test)
    {
    }

    public async Task<UserEntity> GetUser()
    {
      return await GetSingle<UserEntity>(
        nameof(GetUser),
        "SELECT * FROM `Users` LIMIT 1"
      );
    }
  }
}
