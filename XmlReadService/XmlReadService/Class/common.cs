using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
namespace Alog_WSKJSD
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
                    SQL = System.Text.RegularExpressions.Regex.Replace(SQL, "'" + key + "'", "'" + HttpUtility.UrlDecode(keyvalue) + "'", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    SQL = System.Text.RegularExpressions.Regex.Replace(SQL, key, "'" + HttpUtility.UrlDecode(keyvalue) + "'", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
            return SQL;
        }
        #endregion
    }
}