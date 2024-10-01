using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TouchOne
{
	/// <summary>
	/// Interaction logic for LoggerWindow.xaml
	/// </summary>
	public partial class LoggerWindow : Window
	{
		public LoggerWindow()
		{
			InitializeComponent();

			IsClosingProcess = false;
			LogText = new StringBuilder();
		}


		public StringBuilder LogText {get; private set;}
		public bool IsClosingProcess { get; set; }

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(this.Visibility == System.Windows.Visibility.Visible && !IsClosingProcess)
			{
				this.Visibility = System.Windows.Visibility.Hidden;
				e.Cancel = true;
			}
		}

		public void ShowLog()
		{
			this.Logger.Text = LogText.ToString();
			LogText = new StringBuilder();
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			ShowLog();
		}

	}
}
