﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	public partial class Login : Window
	{
		public Login()
		{
			InitializeComponent();
		}

		// Диалоговое окно при при закрытии будет показано только если окно авторизации
		// находится в фокусе (когда пользователь скорее всего сам нажал крестик)
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.IsFocused == true)
			{
				MessageBoxResult ExitResult = MessageBox.Show("Вы действительно хотите выйти?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (ExitResult == MessageBoxResult.Yes)
				{
					Application.Current.Shutdown();
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		// При выборе аккаунта Администратора мы показываем поле ввода пароля и
		// фокусируем ввод на нем
		private void AdminSelectButton_Click(object sender, RoutedEventArgs e)
		{
			this.AdminLoginPanel.Visibility = Visibility.Visible;
			this.AdminLoginPassword.Focus();
		}

		// При выборе аккаунта Исследователя мы создаем окно для исследователя
		// и ставим на нем фокус. Окно авторизации закрываем.
		// (Фокус ставится для того, чтобы окно авторизации закрылось без подтверждения)
		private void ResearcherSelectButton_Click(object sender, RoutedEventArgs e)
		{
			MainWindow ResearcherWindow = new MainWindow();
			ResearcherWindow.Show();
			ResearcherWindow.Focus();
			this.Close();
		}
	}
}
