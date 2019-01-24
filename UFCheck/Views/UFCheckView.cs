using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UFCheck.Models;

namespace UFCheck.Views
{
    public partial class UFCheckView : Form
    {
        private UFCheckController _ufCheckCtl;   // 控制器类
        private Dictionary<int, ListViewItem> dicCheckItem = new Dictionary<int, ListViewItem>();   // 键：checkItem的hash，值：ListViewItem


        public UFCheckView()
        {
            InitializeComponent();
        }

        private void UFCheckView_Load(object sender, EventArgs e)
        {
            // 加载检查项元素
            try
            {
                // 加载控制器类
                _ufCheckCtl = new UFCheckController();

                // 刷新ListView界面
                InitCheckList();

                //debug
                lbNow.Text = _ufCheckCtl.DtNow.ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Exit();
            }
        }


        /// <summary>
        /// 初始化ListView
        /// </summary>
        private void InitCheckList()
        {
            dicCheckItem.Clear();

            lvCheckList.BeginUpdate();
            lvCheckList.Items.Clear();
            foreach (CheckItem ci in _ufCheckCtl.CheckItemList)
            {
                ListViewItem lvi = new ListViewItem(ci.Idx.ToString());
                lvi.SubItems.Add(ci.Desc);
                lvi.SubItems.Add(ci.ParaDate.ActualValue);
                lvi.SubItems.Add(ci.ParaStatus.ActualValue);
                lvi.SubItems.Add(ci.IsCheckPassed ? "√" : "×");
                lvi.SubItems.Add(ci.Note);
                lvi.Tag = ci;

                lvCheckList.Items.Add(lvi);                 // 前台ListView添加
                dicCheckItem.Add(ci.GetHashCode(), lvi);    // 添加hash表
            }
            lvCheckList.EndUpdate();


            lvCheckList.Columns[0].Width = -1;
            lvCheckList.Columns[1].Width = -1;
        }


        /// <summary>
        /// 根据CheckItem的hashcode更新ListView指定行
        /// </summary>
        /// <param name="hashCode"></param>
        private void UpdateCheckList(int hashCode)
        {
            ListViewItem lvi = dicCheckItem[hashCode];
            CheckItem ci = (CheckItem)lvi.Tag;

            lvCheckList.BeginUpdate();
            lvi.SubItems[2].Text = ci.ParaDate.ActualValue;             // 市场日期
            lvi.SubItems[3].Text = ci.ParaStatus.ActualValue;           // 市场状态
            lvi.SubItems[4].Text = ci.IsCheckPassed ? "√" : "×";        // 检查通过
            lvi.SubItems[5].Text = ci.Note;                             // 说明
            if (!ci.IsCheckPassed)
                lvi.BackColor = Color.Pink;
            else
                lvi.BackColor = SystemColors.Window;

            if (ci.IsRunning)
            {
                lvi.BackColor = Color.LightBlue;
                lvi.EnsureVisible();
            }

            lvCheckList.EndUpdate();
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (!bw.IsBusy)
            {
                lbProgramStatus.Text = "正在运行...";
                bw.RunWorkerAsync();
                btnCheck.Text = "检查中...";
            }
            else
            {
                bw.CancelAsync();
                btnCheck.Text = "检查";
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker bgWorker = sender as BackgroundWorker;

                foreach (CheckItem checkItem in _ufCheckCtl.CheckItemList)
                {
                    if (true == bgWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }


                    try
                    {
                        _ufCheckCtl.SetItemRunningStatus(checkItem, true);
                        bgWorker.ReportProgress(1, checkItem.GetHashCode());

                        _ufCheckCtl.StartCheckItem(checkItem);
                        bgWorker.ReportProgress(1, checkItem.GetHashCode());
                        Thread.Sleep(50);
                    }
                    catch (Exception ex)
                    {
                        checkItem.Note = ex.Message;
                    }
                    finally
                    {
                        _ufCheckCtl.SetItemRunningStatus(checkItem, false);
                        bgWorker.ReportProgress(1, checkItem.GetHashCode());
                    }
                }

                e.Result = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                try
                {
                    UpdateCheckList(int.Parse(e.UserState.ToString()));
                }
                catch (Exception)
                { }
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)    // 未处理的异常，需要弹框
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {

            }
            else
            {

            }

            // 刷新状态

            btnCheck.Text = "检查";
            lbProgramStatus.Text = "已执行";
            lbIsCheckPassed.Text = _ufCheckCtl.IsAllOK() ? "√" : "×";

        }
    }
}
