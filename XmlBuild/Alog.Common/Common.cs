using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
namespace Alog.Common
{
    /// <summary>
    ///common 的摘要说明
    /// </summary>
    public class common
    {
        #region  替换url的参数
        /// <summary>
        /// 替换url的参数
        /// </summary>
        /// <param name="SQL">SQL</param>
        /// <returns></returns>
        public static string ReplaceParameter(string SQL, Dictionary<string, string> parameters)
        {
            string pattern = @"@[^@]\w+";
            MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(SQL, pattern, RegexOptions.IgnoreCase);
            foreach (Match m in mc)
            {
                string key = m.Value.Trim();              //另外 m.Groups[1]，m.Groups[2]是非固定段字符串 
                string key1 = m.Value.Trim().Replace("'", "'").Substring(1);

                if (parameters.ContainsKey(key1))
                {
                    string keyvalue = parameters[key1].ToString();
                    //判断是否IN语句
                    string tmp = SQL.Replace(" ", "");
                    int index = tmp.IndexOf(key);
                    if (index > -1)
                    {
                        string inSymbol = tmp.Substring(index - 3, 3);
                        if (inSymbol.ToUpper() == "IN(")
                        {
                            SQL = System.Text.RegularExpressions.Regex.Replace(SQL, key, HttpUtility.UrlDecode(keyvalue), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        }
                    }
                    SQL = System.Text.RegularExpressions.Regex.Replace(SQL, "'" + key + "'", "'" + HttpUtility.UrlDecode(keyvalue) + "'", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    SQL = System.Text.RegularExpressions.Regex.Replace(SQL, key, "'" + HttpUtility.UrlDecode(keyvalue) + "'", System.Text.RegularExpressions.RegexOptions.IgnoreCase);                    
                }
            }
            return SQL;
        }

        /// <summary>
        /// 替换url的参数
        /// </summary>
        /// <param name="SQL">SQL</param>
        /// <returns></returns>
        public static string ReplaceParameter(string SQL, Dictionary<string, string> parameters, Dictionary<string, SqlParam> parameterParams)
        {
            string pattern = @"@[^@]\w+";
            MatchCollection mc = Regex.Matches(SQL, pattern, RegexOptions.IgnoreCase);
            foreach (Match m in mc)
            {
                string key = m.Value.Trim();
                if (parameters.ContainsKey(key))
                {
                    SQL = SQL.Replace("'" + key + "'", key);
                    SqlParam newParam = new SqlParam() { Value = parameters[key.Trim()].ToString() };
                    parameterParams.Add(key, newParam);
                }
            }
            return SQL;
        }
        #endregion
    }
}