using NUnit.Framework;
using Rn.NetCore.DbCommon.Models;

namespace Rn.NetCore.DbCommon.T1.Tests.Models
{
  [TestFixture]
  public class CountResultTests
  {
    [Test]
    public void CountResult_Given_Constructed_ShouldDefault_Id()
    {
      Assert.AreEqual(0, new CountResult().Id);
    }

    [Test]
    public void CountResult_Given_Constructed_ShouldDefault_Count()
    {
      Assert.AreEqual(0, new CountResult().Count);
    }
  }
}
