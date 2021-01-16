namespace Rn.NetCore.DbCommon.Models
{
  public class CountResult
  {
    public int Id { get; set; }
    public int Count { get; set; }

    public CountResult()
    {
      // TODO: [TESTS] (CountResult) Add tests
      Id = 0;
      Count = 0;
    }
  }
}
