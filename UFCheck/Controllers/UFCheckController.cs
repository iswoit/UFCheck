using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;
using UFCheck.Models;

namespace UFCheck
{
    public class UFCheckController
    {
        private DBConn _dbConn;                                              // 数据库连接串
        DateTime _dtNow;                                                    // 当前时间（需要读数据库时间）
        private List<CheckItem> _listCheckItem = new List<CheckItem>();     // 检查项model列表



        public DateTime DtNow
        {
            get { return _dtNow; }
        }

        public List<CheckItem> CheckItemList
        {
            get { return _listCheckItem; }
        }


        /// <summary>
        /// Controller构造函数
        /// </summary>
        public UFCheckController()
        {
            // 从配置文件读取数据库连接串
            _dbConn = DBConn.LoadDBConn();

            // 读取数据库时间
            try
            {
                _dtNow = Util.GetDBDate(_dbConn);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(@"获取数据库当前日期失败: {0}", ex.Message));
            }


            // 读取配置(要替换成数据库时间)
            _listCheckItem = CheckItem.LoadCheckItemList(_dtNow);
        }


        /// <summary>
        /// 设置开始或结束
        /// </summary>
        /// <param name="checkItem"></param>
        /// <param name="isRunning"></param>
        public void SetItemRunningStatus(CheckItem checkItem, bool isRunning)
        {
            checkItem.IsRunning = isRunning;
        }


        /// <summary>
        /// 检查单个条目
        /// </summary>
        /// <param name="checkItem"></param>
        public void StartCheckItem(CheckItem checkItem)
        {
            /* 1.读数据库日期，判断
             * 2.读状态，判断
             * 3.读额外表
             */



            string connStr = string.Format(@"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2}))));Persist Security Info=True;User ID={3};Password={4};",
                                                _dbConn.IP,
                                                _dbConn.Port,
                                                _dbConn.Server,
                                                _dbConn.User,
                                                _dbConn.Password
                                                );

            // 读日期
            if (checkItem.ParaDate != null)
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = conn.CreateCommand())
                    {
                        string strWhere = string.Empty;
                        if (checkItem.ParaDate.Condition.Trim().Length > 0)
                        {
                            strWhere = " where " + checkItem.ParaDate.Condition;
                        }

                        cmd.CommandText = string.Format(@"select {0} from {1} {2}", checkItem.ParaDate.Column, checkItem.ParaDate.Table, strWhere);
                        object res = cmd.ExecuteScalar();

                        checkItem.ParaDate.ActualValue = res.ToString().Trim();
                    }
                }
            }


            // 读状态
            if (checkItem.ParaStatus != null)
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = conn.CreateCommand())
                    {
                        string strWhere = string.Empty;
                        if (checkItem.ParaDate.Condition.Trim().Length > 0)
                        {
                            strWhere = " where " + checkItem.ParaDate.Condition;
                        }

                        cmd.CommandText = string.Format(@"select {0} from {1} {2}", checkItem.ParaStatus.Column, checkItem.ParaStatus.Table, strWhere);
                        object res = cmd.ExecuteScalar();

                        checkItem.ParaStatus.ActualValue = res.ToString().Trim();
                    }
                }
            }


            // 读额外
            if (checkItem.ParaExtra != null)
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = conn.CreateCommand())
                    {
                        string strWhere = string.Empty;
                        if (checkItem.ParaExtra.Condition.Trim().Length > 0)
                        {
                            strWhere = " where " + checkItem.ParaExtra.Condition;
                        }

                        cmd.CommandText = string.Format(@"select {0} from {1} {2}", checkItem.ParaExtra.Column, checkItem.ParaExtra.Table, strWhere);
                        object res = cmd.ExecuteScalar();

                        checkItem.ParaExtra.ActualValue = res.ToString().Trim();
                    }
                }
            }


            checkItem.IsChecked = true;
            checkItem.UpdateNote();

        }



        public bool IsAllOK()
        {
            foreach (CheckItem checkItem in _listCheckItem)
            {
                if (!checkItem.IsCheckPassed)
                    return false;
            }

            return true;
        }

    }
}
