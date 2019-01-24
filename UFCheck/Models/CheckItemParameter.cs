using System;
using System.Collections.Generic;
using System.Text;

namespace UFCheck.Models
{
    public class CheckItemParameter
    {
        private string _table;           // 表
        private string _column;          // 列
        private string _condition;       // 条件
        private string _expectedValue;   // 期望值
        private string _actualValue;     // 实际值


        public string Table
        {
            get { return _table; }
        }

        public string Column
        {
            get { return _column; }
        }

        public string Condition
        {
            get { return _condition; }
        }


        public string ExpectedValue
        {
            get { return _expectedValue; }
        }

        public string ActualValue
        {
            get { return _actualValue; }
            set { _actualValue = value; }
        }

        // 参数是否ok
        public bool IsParaOK
        {
            get {
                if (string.Equals(_expectedValue, _actualValue, StringComparison.CurrentCultureIgnoreCase))
                    return true;
                else
                    return false;
            }
        }

        public CheckItemParameter(string table, string column, string condition, string expectedValue)
        {
            _table = table;
            _column = column;
            _condition = condition;
            _expectedValue = expectedValue;
            _actualValue = string.Empty;
        }
    }
}
