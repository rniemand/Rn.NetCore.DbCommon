using System.Collections.Generic;
using System.Linq;
using Rn.NetCore.Common.Extensions;

namespace Rn.NetCore.DbCommon.Models
{
  public class DatabaseRow
  {
    public bool ValidRow { get; private set; }
    private readonly IDictionary<string, object> _row;
    private readonly List<string> _columns;

    public DatabaseRow(object dapperRow = null)
    {
      // TODO: [TESTS] (DatabaseRow) Add tests
      ValidRow = IsDapperRow(dapperRow);
      _columns = new List<string>();

      _row = ValidRow
        ? (IDictionary<string, object>)dapperRow
        : new Dictionary<string, object>();
      
      if (_row?.Keys == null) return;
      foreach (var key in _row?.Keys)
        _columns.Add(key);
    }

    public DatabaseRow(IEnumerable<object> rows)
      : this(rows.FirstOrDefault())
    {
      // TODO: [TESTS] (DatabaseRow) Add tests
    }

    private object GetColumn(int index)
    {
      // TODO: [TESTS] (DatabaseRow.GetColumn) Add tests
      if (!ValidRow)
        return null;

      return _columns.Count < index + 1
        ? null
        : _row[_columns[index]];
    }

    public string GetStringColumn(int index, string fallback = null)
    {
      // TODO: [TESTS] (DatabaseRow.GetStringColumn) Add tests
      return GetColumn(index) switch
      {
        null => fallback,
        string str => str,
        _ => fallback
      };
    }

    public string GetStringColumn(string columnName, string fallback = null)
    {
      // TODO: [TESTS] (DatabaseRow.GetStringColumn) Add tests
      columnName = ResolveColumnName(columnName);
      if (string.IsNullOrWhiteSpace(columnName))
        return fallback;

      return (string) _row[columnName];
    }

    public int GetIntColumn(int index, int fallback = 0)
    {
      // TODO: [TESTS] (DatabaseRow.GetIntColumn) Add tests
      return GetColumn(index) switch
      {
        null => fallback,
        int i => i,
        _ => int.TryParse(GetColumn(index).ToString(), out var parsed) ? parsed : fallback
      };
    }


    // Internal methods
    private static bool IsDapperRow(object potentialRow = null)
    {
      // TODO: [TESTS] (DatabaseRow.IsDapperRow) Add tests
      if (potentialRow == null)
        return false;

      var safeName = potentialRow.GetType().Name.LowerTrim();
      return safeName == "dapperrow";
    }

    private string ResolveColumnName(string columnName)
    {
      // TODO: [TESTS] (DatabaseRow.ResolveColumnName) Add tests
      return _columns.Contains(columnName)
        ? columnName
        : _columns.FirstOrDefault(x => x.IgnoreCaseEquals(columnName));
    }
  }
}
