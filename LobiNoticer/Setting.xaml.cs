using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LobiNoticer
{
    /// <summary>
    /// Setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
            this.comboBox.SelectedIndex = IndexFromValue(Properties.Settings.Default.interval);
            this.textBox.Text = Properties.Settings.Default.mail;
            this.textBox1.Text = Properties.Settings.Default.password;
            this.Icon = Imaging.CreateBitmapSourceFromHIcon(Properties.Resources.lobi2.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        
        private int IndexFromValue(int interval)
        {
            switch (interval)
            {
                case 1:
                    return 0;
                case 3:
                    return 1;
                case 5:
                    return 2;
                case 10:
                    return 3;
                case 30:
                    return 4;
            }
            return 2;
        }

        private int ValueFromIndex(int index)
        {
            switch (index)
            {
                case -1:
                    return 5;
                case 0:
                    return 1;
                case 1:
                    return 3;
                case 2:
                    return 5;
                case 3:
                    return 10;
                case 4:
                    return 30;
            }
            return 5;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /*e.Cancel = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;*/
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (ValueFromIndex(this.comboBox.SelectedIndex) != Properties.Settings.Default.interval || !this.textBox.Text.Equals(Properties.Settings.Default.mail) || !this.textBox1.Text.Equals(Properties.Settings.Default.password))
            {
                Properties.Settings.Default.mail = this.textBox.Text;
                Properties.Settings.Default.password = this.textBox1.Text;
                Properties.Settings.Default.interval = ValueFromIndex(this.comboBox.SelectedIndex);
                Properties.Settings.Default.Save();
            }
            this.Close();
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
