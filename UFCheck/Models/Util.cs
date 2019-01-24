using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess;
using Oracle.ManagedDataAccess.Client;

namespace UFCheck.Models
{
    public static class Util
    {
        private static char[] arrMddConvert;  // mdd格式的字典数组
        static Util()
        {
            arrMddConvert = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c' };
        }

        public static string ReplaceStringWithDateFormat(string str, DateTime dtNow)
        {
            string strTmp = str;   // 返回值

            string yyyymmdd_replacement = dtNow.ToString("yyyyMMdd");
            string yymmdd_replacement = dtNow.ToString("yyMMdd");
            string mmdd_replacement = string.Format("{0}{1}", dtNow.Month.ToString().PadLeft(2, '0'), dtNow.Day.ToString().PadLeft(2, '0'));
            string mdd_replacement = string.Format("{0}{1}", arrMddConvert[dtNow.Month - 1], dtNow.Day.ToString().PadLeft(2, '0'));


            strTmp = Regex.Replace(strTmp, "yyyymmdd", yyyymmdd_replacement, RegexOptions.IgnoreCase);  // 1.替换yyyymmdd
            strTmp = Regex.Replace(strTmp, "yymmdd", yymmdd_replacement, RegexOptions.IgnoreCase);      // 2.替换yymmdd
            strTmp = Regex.Replace(strTmp, "mmdd", mmdd_replacement, RegexOptions.IgnoreCase);          // 3.替换mmdd
            strTmp = Regex.Replace(strTmp, "mdd", mdd_replacement, RegexOptions.IgnoreCase);            // 4.替换mdd
            return strTmp;
        }



        public static DateTime GetDBDate(DBConn dbConn)
        {
            DateTime dtReturn = new DateTime(1, 1, 1);

            string connStr = string.Format(@"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2}))));Persist Security Info=True;User ID={3};Password={4};",
                                                dbConn.IP,
                                                dbConn.Port,
                                                dbConn.Server,
                                                dbConn.User,
                                                dbConn.Password
                                                );

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"select sysdate from dual";
                    object res = cmd.ExecuteScalar();

                    DateTime.TryParse(res.ToString(), out dtReturn);
                }
            }

            return dtReturn;
        }




    }
}
