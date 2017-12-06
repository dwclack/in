using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Alog.Common
{
    public static class SqlHelper
    {
        public static string GetSqlParamsAndReplaceParams(string sql, string preStr, string startStr, string endStr, string numTypeStartStr, string numTypeEndStr, Dictionary<string, SqlParam> allSqlParams)
        {
            Dictionary<string, SqlParam> selectSqlParams = GetParameters(sql, startStr, endStr);
            sql = ReplaceParameters(sql, startStr, endStr, preStr, selectSqlParams);
            Dictionary<string, SqlParam> selectSqlParams1 = GetParameters(sql, numTypeStartStr, numTypeEndStr);
            sql = ReplaceParameters(sql, numTypeStartStr, numTypeEndStr, preStr, selectSqlParams1);
            foreach (var param in selectSqlParams)
            {
                if (!allSqlParams.ContainsKey(preStr+ param.Key))
                {
                    allSqlParams.Add(preStr + param.Key, param.Value);
                }
            }
            foreach (var param in selectSqlParams1)
            {
                if (!allSqlParams.ContainsKey(preStr + param.Key))
                {
                    allSqlParams.Add(preStr + param.Key, param.Value);
                }
            }

            return sql;
        }

        public static string ReplaceParameters(string sql, string startStr, string endStr, string preStr, Dictionary<string, SqlParam> paramList)
        {
            foreach (var paramName in paramList.Keys)
            {
                sql = sql.Replace(startStr + paramName + endStr, preStr + paramName);
            }
            sql = sql.Replace("N@", "@");

            return sql;
        }

        public static Dictionary<string, SqlParam> GetParameters(string sql, string startStr, string endStr)
        {
            Dictionary<string, SqlParam> parameters = new Dictionary<string, SqlParam>();

            if (!string.IsNullOrEmpty(startStr) && !string.IsNullOrEmpty(endStr))
            {
                var paramNames = StringHelper.GetListBetweenStr(sql, startStr, endStr);
                foreach (var param in paramNames)
                {
                    if (!parameters.ContainsKey(param))
                    {
                        parameters.Add(param, new SqlParam());
                    }
                }
            }

            return parameters;
        }
    }

    public class SqlParam
    {
        public SqlParam()
        {
            this.Type = SqlDbType.VarChar;
            this.Length = 8000;
        }
        public SqlDbType Type { get; set; }
        public int Length { get; set; }
        public string Value { get; set; }
    }
}
