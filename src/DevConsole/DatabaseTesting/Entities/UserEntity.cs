using System;

namespace DevConsole.DatabaseTesting.Entities
{
  public class UserEntity
  {
    public int UserId { get; set; }
    public bool Deleted { get; set; }
    public DateTime DateCreatedUtc { get; set; }
    public DateTime? DateModified { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string UserEmail { get; set; }

    public UserEntity()
    {
      // TODO: [TESTS] (UserEntity) Add tests
      UserId = 0;
      Deleted = false;
      DateCreatedUtc = DateTime.UtcNow;
      DateModified = null;
      LastLoginDate = null;
      FirstName = string.Empty;
      LastName = string.Empty;
      Username = string.Empty;
      Password = string.Empty;
      UserEmail = string.Empty;
    }
  }
}
