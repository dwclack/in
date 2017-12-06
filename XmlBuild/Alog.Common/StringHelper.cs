using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Alog.Common
{
    public static class StringHelper
    {
        /// <summary>
        /// 获得字符串中开始和结束字符串中间得值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="leftStr">左边文本</param>
        /// <param name="rightStr">右边文本</param>
        /// <returns></returns> 
        public static string GetBetweenStr(string str, string leftStr, string rightStr)
        {
            var list = GetListBetweenStr(str, leftStr, rightStr);
            if (list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获得字符串中开始和结束字符串中间得值到List
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="leftStr">左边文本</param>
        /// <param name="rightStr">右边文本</param>
        /// <returns>List集合</returns>
        public static List<string> GetListBetweenStr(string str, string leftStr, string rightStr)
        {
            List<string> list = new List<string>();
            int leftIndex = str.IndexOf(leftStr);//左文本起始位置
            int leftlength = leftStr.Length;//左文本长度
            int rightIndex = 0;
            string temp = "";
            while (leftIndex != -1)
            {
                rightIndex = str.IndexOf(rightStr, leftIndex + leftlength);
                if (rightIndex == -1)
                {
                    break;
                }
                temp = str.Substring(leftIndex + leftlength, rightIndex - leftIndex - leftlength);
                list.Add(temp);
                leftIndex = str.IndexOf(leftStr, rightIndex + 1);
            }
            return list;
        }

        /// <summary>
        /// 获取异常信息和调用堆栈
        /// </summary>
        /// <param name="ex">异常</param>
        /// <returns>Message:异常信息  StackTrace:调用堆栈</returns>
        public static string GetExceptionMsg(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Message:");
            sb.Append(ex.Message);
            sb.Append("  StackTrace:");
            sb.Append(ex.StackTrace);

            return sb.ToString();
        }
    }
}
