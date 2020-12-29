using System;

namespace Rn.NetCore.DbCommon
{
  public static class SqlFormatter
  {
    public static string MergeParams(string sql, object param = null)
    {
      // TODO: [TESTS] (SqlFormatter.MergeParams) Add tests

      if (param == null)
        return sql;

      var processedSql = sql;
      foreach (var property in param.GetType().GetProperties())
      {
        var name = $"@{property.Name}";
        var replacement = GetSqlReplacement(property.GetValue(param));
        processedSql = processedSql.Replace(name, replacement);
      }

      return processedSql;
    }

    private static string GetSqlReplacement(object value)
    {
      // TODO: [TESTS] (SqlFormatter.GetSqlReplacement) Add tests

      if (value == null)
        return "";

      var valueType = value.GetType().Name.ToLower().Trim();

      if (valueType == "string")
        return $"'{value}'";

      if (valueType.StartsWith("int") || valueType == "single")
        return value.ToString();

      if (valueType == "double")
        return ((double)value).ToString("D");

      if (valueType == "boolean")
        return ((bool)value) ? "1" : "0";

      if (valueType == "datetime")
        return $"'{value}'";

      if (value is Enum)
        return Enum.Format(value.GetType(), value, "D");

      throw new Exception($"Unsupported type: {valueType} ({value})");
    }
  }
}
