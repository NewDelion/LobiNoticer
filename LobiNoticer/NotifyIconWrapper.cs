using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LobiAPI;
using System.Windows.Threading;
using System.Windows;

namespace LobiNoticer
{
    public partial class NotifyIconWrapper : Component
    {
        public NotifyIconWrapper()
        {
            InitializeComponent();

            

            toolStripMenuItem1.Click += MenuClick_Status;
            toolStripMenuItem2.Click += MenuClick_Setting;
            toolStripMenuItem3.Click += MenuClick_Exit;

            this.dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
            this.dispatcherTimer.Tick += tick;

            this.showTip = new DispatcherTimer(DispatcherPriority.Normal);
            this.showTip.Tick += tick_show;
            this.showTip.Interval = new TimeSpan(0, 0, 0, 0, 1500);
            this.showTip.Start();
        }

        DispatcherTimer dispatcherTimer = null;
        DispatcherTimer showTip = null;
        BasicAPI api = null;

        private Setting setting = null;
        
        private void ShowSetting()
        {
            setting = new Setting();
            // ウィンドウ表示&最前面に持ってくる
            if (setting.WindowState == System.Windows.WindowState.Minimized)
                setting.WindowState = System.Windows.WindowState.Normal;

            setting.Show();
            setting.Activate();
            // タスクバーでの表示をする
            setting.ShowInTaskbar = true;
        }

        private void MenuClick_Exit(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuClick_Setting(object sender, EventArgs e)
        {
            if (this.toolStripMenuItem2.Enabled)
                ShowSetting();
        }

        private void MenuClick_Status(object sender, EventArgs e)
        {
            if (toolStripMenuItem1.Text == "開始")
            {
                if (setting != null && setting.WindowState == WindowState.Normal)
                {
                    MessageBox.Show("設定ウィンドウを閉じてください。", "メッセージ");
                    return;
                }

                api = new BasicAPI();
                if (!api.Login(Properties.Settings.Default.mail, Properties.Settings.Default.password))
                {
                    MessageBox.Show("ログインに失敗しました。\nメールアドレスとパスワードを設定から確認してください。", "ログイン失敗");
                    return;
                }
                else
                {
                    this.last_id = long.Parse(api.GetNotifications().notifications[0].id);
                }

                toolStripMenuItem1.Text = "停止";
                toolStripMenuItem2.Enabled = false;
                
                dispatcherTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.interval);
                dispatcherTimer.Start();
            }
            else
            {
                toolStripMenuItem1.Text = "開始";
                toolStripMenuItem2.Enabled = true;

                dispatcherTimer.Stop();
            }
            
        }

        private void notifyIcon1_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.toolStripMenuItem2.Enabled)
                ShowSetting();
        }

        private long last_id = 0;
        private object lockObj = new object();
        private Queue<LobiAPI.Json.Notification> notification_queue = new Queue<LobiAPI.Json.Notification>();
        public void tick(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();

            try {
                LobiAPI.Json.Notification[] notificataion = api.GetNotifications().notifications;
                for (int i = notificataion.Length - 1; i >= 0; i--)
                {
                    if (long.Parse(notificataion[i].id) > last_id)
                    {
                        lock (lockObj)
                        {
                            notification_queue.Enqueue(notificataion[i]);
                        }
                        last_id = long.Parse(notificataion[i].id);
                    }
                }
            }
            catch(Exception ex)
            {
                try
                {
                    api.Login(Properties.Settings.Default.mail, Properties.Settings.Default.password);
                }
                catch(Exception exx)
                {
                    MessageBox.Show("致命的なエラー...\nアプリケーションを終了します。");
                    System.Windows.Application.Current.Shutdown();
                }
                
            }
            if (!toolStripMenuItem2.Enabled)
                dispatcherTimer.Start();
        }

        public void tick_show(object sender, EventArgs e)
        {
            lock (lockObj)
            {
                if (notification_queue.Count == 0)
                    return;
                LobiAPI.Json.Notification notification = notification_queue.Dequeue();
                try {//一応tryで囲んどく。たぶん治ったけど
                    string title = notification.title.template;
                    for (int i = 0; i < notification.title.items.Length; i++)
                    {
                        if (notification.title.template.IndexOf("{{p" + (i + 1).ToString() + "}}") != -1)
                        {
                            title = notification.title.template.Replace("{{p" + (i + 1).ToString() + "}}", notification.title.items[i].label);
                        }
                    }
                    notifyIcon1.ShowBalloonTip(3000, title, notification.message == "" ? " " : notification.message, System.Windows.Forms.ToolTipIcon.None);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("エラーが発生しました。開発者に知らせてください。");
                }
            }
        }
    }
}
