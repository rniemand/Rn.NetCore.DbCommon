using System;
using System.Collections.Generic;
using System.Reflection;
using Rn.NetCore.Common.Extensions;

namespace Rn.NetCore.DbCommon.Helpers
{
  public static class SqlHelper
  {
    // Public methods
    public static string GenerateExecutedSql(string sql, object param = null)
    {
      // TODO: [REVISE] (SqlHelper.GenerateExecutedSql) Revise this

      if (param == null)
        return sql;

      var formattedSql = sql;

      foreach (var (placeholder, sqlValue) in GetReplacementDictionary())
      {
        formattedSql = formattedSql.Replace(placeholder, sqlValue);
      }

      return formattedSql;
    }

    public static Dictionary<string, string> GetReplacementDictionary(object param = null)
    {
      // TODO: [TESTS] (SqlHelper.GetReplacementDictionary) Add tests
      if (param == null)
        return new Dictionary<string, string>();

      var properties = param.GetType().GetProperties();
      var replacementDictionary = new Dictionary<string, string>();

      // ReSharper disable once LoopCanBeConvertedToQuery
      foreach (var propertyInfo in properties)
      {
        replacementDictionary.Add($"@{propertyInfo.Name}", GetFormattedValue(param, propertyInfo));
      }

      return replacementDictionary;
    }


    // Reflection helpers
    private static string GetFormattedValue(object param, PropertyInfo propertyInfo)
    {
      if (propertyInfo.PropertyType.IsEnum)
        return GetEnumValue(param, propertyInfo);

      var underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
      var safeName = propertyInfo.PropertyType.Name.LowerTrim();
      var isNullable = underlyingType != null;

      if (isNullable)
        safeName = underlyingType.Name.LowerTrim();

      if (safeName == "int32")
        return GetInt32Value(param, propertyInfo, isNullable);

      if (safeName == "int64")
        return GetInt64Value(param, propertyInfo, isNullable);

      if (safeName == "double")
        return GetIntDoubleValue(param, propertyInfo, isNullable);

      if (safeName == "boolean")
        return GetBooleanValue(param, propertyInfo, isNullable);

      if (safeName == "datetime")
        return GetDateTimeValue(param, propertyInfo, isNullable);

      if (safeName == "string")
        return GetStringValue(param, propertyInfo, isNullable);

      // Something is not supported
      var value = propertyInfo.GetValue(param);
      throw new Exception($"Unsupported type ({safeName}): {value}");
    }

    private static string GetEnumValue(object param, PropertyInfo propertyInfo)
    {
      var value = propertyInfo.GetValue(param);
      var changeType = Convert.ChangeType(value, propertyInfo.PropertyType);
      var intValue = (int)changeType;
      return intValue.ToString("D");
    }

    private static string GetInt32Value(object param, PropertyInfo propertyInfo, bool isNullable)
    {
      if (isNullable)
      {
        var nullable = (int?)propertyInfo.GetValue(param);
        return !nullable.HasValue ? "NULL" : nullable.Value.ToString("D");
      }

      var value = (int)propertyInfo.GetValue(param);
      return value.ToString("D");
    }

    private static string GetInt64Value(object param, PropertyInfo propertyInfo, bool isNullable)
    {
      if (isNullable)
      {
        var nullable = (long?)propertyInfo.GetValue(param);
        return !nullable.HasValue ? "NULL" : nullable.Value.ToString("D");
      }

      var value = (long) propertyInfo.GetValue(param);
      return value.ToString("D");
    }

    private static string GetIntDoubleValue(object param, PropertyInfo propertyInfo, bool isNullable)
    {
      double value;

      if (isNullable)
      {
         var nullable = (double?)propertyInfo.GetValue(param);
         if (!nullable.HasValue) return "NULL";
         value = nullable.Value;
      }
      else
      {
        value = (double)propertyInfo.GetValue(param);
      }

      return value
        .ToString("N")
        .Replace(",", "")
        .Replace(".00", "");
    }

    private static string GetBooleanValue(object param, PropertyInfo propertyInfo, bool isNullable)
    {
      bool value;

      if (isNullable)
      {
        var nullable = (bool?) propertyInfo.GetValue(param);
        if (!nullable.HasValue) return "NULL";
        value = nullable.Value;
      }
      else
      {
        value = (bool)propertyInfo.GetValue(param);
      }

      return value ? "1" : "0";
    }

    private static string GetDateTimeValue(object param, PropertyInfo propertyInfo, bool isNullable)
    {
      DateTime value;

      if (isNullable)
      {
        var nullable = (DateTime?) propertyInfo.GetValue(param);
        if (!nullable.HasValue) return "NULL";
        value = nullable.Value;
      }
      else
      {
        value = (DateTime)propertyInfo.GetValue(param);
      }

      return $"'{value:O}'";
    }

    private static string GetStringValue(object param, PropertyInfo propertyInfo, bool isNullable)
    {
      // TODO: [SQL-ESCAPE] (SqlHelper.GetStringValue) SQL Escape this
      var value = (string) propertyInfo.GetValue(param);

      if (string.IsNullOrWhiteSpace(value))
        return isNullable ? "NULL" : "''";

      return $"'{value}'";
    }
  }
}
