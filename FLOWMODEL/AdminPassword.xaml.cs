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

namespace FLOWMODEL
{
	/// <summary>
	/// Interaction logic for AdminPassword.xaml
	/// </summary>
	public partial class AdminPassword : Window
	{
		public AdminPassword()
		{
			InitializeComponent();
		}

		private void AdminPasswordCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void AdminPasswordSave_Click(object sender, RoutedEventArgs e)
		{
			// Если метод возвращает true - окно закрывается
			if (Database._instance.SetAdminPassword(AdminLoginPassword.Password)) this.Close();
		}
	}
}
