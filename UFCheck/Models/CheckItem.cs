using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace UFCheck.Models
{
    public class CheckItem
    {
        private int _idx;               // 界面索引
        private string _desc;           // 检查项描述
        private CheckItemParameter _paraDate;       // 市场日期
        private CheckItemParameter _paraStatus;     // 市场状态
        private CheckItemParameter _paraExtra;      // 额外
        private bool _isRunning;         // 是否正在运行
        private bool _isChecked;         // 是否执行过检查
        private string _note;           // 备注


        public int Idx
        {
            get { return _idx; }
        }

        public string Desc
        {
            get { return _desc; }
        }

        public CheckItemParameter ParaDate
        {
            get { return _paraDate; }
            //set { _paraDate = value; }
        }

        public CheckItemParameter ParaStatus
        {
            get { return _paraStatus; }
            //set { _paraStatus = value; }
        }

        public CheckItemParameter ParaExtra
        {
            get { return _paraExtra; }
            //set { _paraStatus = value; }
        }


        public string Note
        {
            get { return _note; }
            set { _note = value; }
        }


        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; }
        }

        // 通过检查
        public bool IsCheckPassed
        {
            get
            {
                if (_isChecked == false)
                    return false;

                if ((ParaDate == null || ParaDate.IsParaOK) && (ParaStatus == null || ParaStatus.IsParaOK))
                    return true;
                else
                {
                    if (ParaExtra != null && ParaExtra.IsParaOK)
                        return true;
                    else
                        return false;
                }
            }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="desc"></param>
        /// <param name="paraDate"></param>
        /// <param name="paraStatus"></param>
        public CheckItem(int idx, string desc, CheckItemParameter paraDate, CheckItemParameter paraStatus, CheckItemParameter paraExtra)
        {
            _idx = idx;
            _desc = desc;
            _paraDate = paraDate;
            _paraStatus = paraStatus;
            _paraExtra = paraExtra;

            _isRunning = false;
            _isChecked = false;
            _note = "未检查";
        }


        /// <summary>
        /// 设置检查完成标志，并写说明
        /// </summary>
        public void SetChecked(string note)
        {
            this._isChecked = true;
            this._note = note;
        }

        /// <summary>
        /// 从配置文件中读取检查项列表
        /// </summary>
        /// <param name="dtNow">数据库当前时间</param>
        /// <returns></returns>
        public static List<CheckItem> LoadCheckItemList(DateTime dtNow)
        {
            int index = 0;
            List<CheckItem> listReturn = new List<CheckItem>();     // 返回值


            // 判断配置文件是否存在，不存在抛出异常
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "cfg.xml")))
                throw new Exception("未能找到配置文件cfg.xml，请重新配置该文件后重启程序!");

            // 读取配置文件
            XmlDocument doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;                                     //忽略文档里面的注释
            using (XmlReader reader = XmlReader.Create(@"cfg.xml", settings))
            {
                doc.Load(reader);

                // 检查根节点
                XmlNode rootNode = doc.SelectSingleNode("Config");   // 根节点
                if (rootNode == null)
                    throw new Exception("无法找到根配置节点<Config>，请检查配置文件格式是否正确!");


                // 获取检查项列表
                XmlNode xnCheckItemList = rootNode.SelectSingleNode("CheckItemList");
                if (xnCheckItemList == null)
                    throw new Exception("无法找到检查项列表节点<Config>-<CheckItemList>，请检查配置文件格式是否正确!");

                // 遍历检查项
                XmlNodeList xnlCheckItem = xnCheckItemList.SelectNodes("CheckItem");    // CheckItem节点
                foreach (XmlNode xnCheckItem in xnlCheckItem)
                {

                    // 临时变量
                    string desc = string.Empty;
                    CheckItemParameter paraDate = null;
                    CheckItemParameter paraStauts = null;
                    CheckItemParameter paraExtra = null;
                    foreach (XmlNode xnCheckItemAttr in xnCheckItem.ChildNodes)    // CheckItem节点的子项
                    {
                        switch (xnCheckItemAttr.Name.Trim())
                        {
                            case "Desc":
                                desc = xnCheckItemAttr.InnerText.Trim();
                                break;
                            case "Date":
                                {
                                    string table = string.Empty;
                                    string column = string.Empty;
                                    string condition = string.Empty;
                                    string expectedValue = string.Empty;

                                    foreach (XmlNode xnCheckItemSub in xnCheckItemAttr.ChildNodes)
                                    {
                                        switch (xnCheckItemSub.Name.Trim())
                                        {
                                            case "Table":
                                                table = xnCheckItemSub.InnerText.Trim();
                                                break;
                                            case "Column":
                                                column = xnCheckItemSub.InnerText.Trim();
                                                break;
                                            case "Condition":
                                                condition = Util.ReplaceStringWithDateFormat(xnCheckItemSub.InnerText.Trim(), dtNow);
                                                break;
                                            case "ExpectedValue":
                                                expectedValue = Util.ReplaceStringWithDateFormat(xnCheckItemSub.InnerText.Trim(), dtNow);
                                                break;
                                        }
                                    }

                                    paraDate = new CheckItemParameter(
                                        table: table,
                                        column: column,
                                        condition: condition,
                                        expectedValue: expectedValue);
                                }
                                break;
                            case "Status":
                                {
                                    string table = string.Empty;
                                    string column = string.Empty;
                                    string condition = string.Empty;
                                    string expectedValue = string.Empty;

                                    foreach (XmlNode xnCheckItemSub in xnCheckItemAttr.ChildNodes)
                                    {
                                        switch (xnCheckItemSub.Name.Trim())
                                        {
                                            case "Table":
                                                table = xnCheckItemSub.InnerText.Trim();
                                                break;
                                            case "Column":
                                                column = xnCheckItemSub.InnerText.Trim();
                                                break;
                                            case "Condition":
                                                condition = Util.ReplaceStringWithDateFormat(xnCheckItemSub.InnerText.Trim(), dtNow);
                                                break;
                                            case "ExpectedValue":
                                                expectedValue = Util.ReplaceStringWithDateFormat(xnCheckItemSub.InnerText.Trim(), dtNow);
                                                break;
                                        }
                                    }

                                    paraStauts = new CheckItemParameter(
                                        table: table,
                                        column: column,
                                        condition: condition,
                                        expectedValue: expectedValue);
                                }
                                break;
                            case "Extra":
                                {
                                    string table = string.Empty;
                                    string column = string.Empty;
                                    string condition = string.Empty;
                                    string expectedValue = string.Empty;

                                    foreach (XmlNode xnCheckItemSub in xnCheckItemAttr.ChildNodes)
                                    {
                                        switch (xnCheckItemSub.Name.Trim())
                                        {
                                            case "Table":
                                                table = xnCheckItemSub.InnerText.Trim();
                                                break;
                                            case "Column":
                                                column = xnCheckItemSub.InnerText.Trim();
                                                break;
                                            case "Condition":
                                                condition = Util.ReplaceStringWithDateFormat(xnCheckItemSub.InnerText.Trim(), dtNow);
                                                break;
                                            case "ExpectedValue":
                                                expectedValue = Util.ReplaceStringWithDateFormat(xnCheckItemSub.InnerText.Trim(), dtNow);
                                                break;
                                        }
                                    }

                                    paraExtra = new CheckItemParameter(
                                        table: table,
                                        column: column,
                                        condition: condition,
                                        expectedValue: expectedValue);
                                }
                                break;
                        }
                    }


                    // 对象插入列表
                    listReturn.Add(new CheckItem(idx: ++index, desc: desc, paraDate: paraDate, paraStatus: paraStauts, paraExtra: paraExtra));

                }//eof foreach
            }//eof using

            return listReturn;
        }



        public void UpdateNote()
        {
            _note = string.Empty;

            if (ParaDate != null && !ParaDate.IsParaOK)
                _note += "交易日期有误;";

            if (ParaStatus != null && !ParaStatus.IsParaOK)
                _note += "市场状态有误;";

            if (ParaExtra != null && ParaExtra.IsParaOK)
                _note = "非交易日";
        }

    }
}
