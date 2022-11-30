using System;
using System.Linq;

namespace ComponentsBind
{
    public class CodeExportBase
    {
        /// <summary>
        /// 首字母小写
        /// </summary>
        public static string FirstCharToLower(string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;

            string str = input.First().ToString().ToLower() + input.Substring(1);
            return str;
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;

            string str = input.First().ToString().ToUpper() + input.Substring(1);
            return str;
        }
    }
}