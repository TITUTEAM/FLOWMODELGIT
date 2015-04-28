using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FLOWMODEL
{
	public partial class App : Application
	{
		public App()
		{
			// Подключаемся к базе при запуске программы
			Database._instance.Connect();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			// Закрываем подключение к базе при выходе из программы
			Database._instance.Close();
		}
	}
}
