using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using OxyPlot;

using System.Data;
using System.Data.SQLite;

namespace FLOWMODEL
{
	public partial class MainWindow : Window
	{
		public IList<DataPoint> TemperaturePoints { get; set; }
		public IList<DataPoint> ViscosityPoints { get; set; }

        private MathModel DefaultModel;
		bool AlgorithmLaunched = false;
		
		public MainWindow(bool AdminMode = false)
		{
			InitializeComponent();

			// --------------------------------------------------------------------------------------------------
			// CUSTOM COMMANDS BINDING
			// --------------------------------------------------------------------------------------------------
			// FILE MENU ////////////////////////////////////////////////////////////////////////////////////////
			var SaveReportBinding = new CommandBinding(Commands.SaveReportCommand, SaveReport, CanSaveReport);
			CommandManager.RegisterClassCommandBinding(typeof(Window), SaveReportBinding);

			var AppExitBinding = new CommandBinding(Commands.AppExitCommand, AppExit, CanAppExit);
			CommandManager.RegisterClassCommandBinding(typeof(Window), AppExitBinding);
			// TOOLS MENU ///////////////////////////////////////////////////////////////////////////////////////
			var ChangeUserBinding = new CommandBinding(Commands.ChangeUserCommand, ChangeUser, CanChangeUser);
			CommandManager.RegisterClassCommandBinding(typeof(Window), ChangeUserBinding);

			//Database._instance.Connect();
			Database._instance.GetMaterials(MaterialTypeCombox);

			// Вставка изначального текста в низу окна при его открытии
			Footer.Text = "Добро пожаловать. Для расчета выходных параметров модели введите все необходимые начальные значения и параметры.";

			// Скрытие\показ пункта меню администратора в зависимости от режима
			if (AdminMode == true) AdminMenu.Visibility = Visibility.Visible; else AdminMenu.Visibility = Visibility.Hidden;
		}
		// ----------------------------------------------------------------------------------------------------------
		// РАБОТА С ВКЛАДКАМИ
		// ----------------------------------------------------------------------------------------------------------
		// Переключение видимости WPF Grid'ов при выборе вкладки "Расчет"
		// Grid расчетов - показан, Grid результатов - скрыт, Grid графиков - скрыт
		private void CalculationsButton_Click(object sender, RoutedEventArgs e)
		{
			Footer.Text = "Ввод начальных данных для расчета.";
			CalcGrid.Visibility = Visibility.Visible;
			ResultGrid.Visibility = Visibility.Hidden;
			GraphGrid.Visibility = Visibility.Hidden;
		}

		// Переключение видимости WPF Grid'ов при выборе вкладки "Результаты"
		// Grid расчетов - скрыт, Grid результатов - показан, Grid графиков - скрыт
		private void ResultsButton_Click(object sender, RoutedEventArgs e)
		{
			Footer.Text = "Выходные параметры, полученные из данных для расчета.";
			CalcGrid.Visibility = Visibility.Hidden;
			ResultGrid.Visibility = Visibility.Visible;
			GraphGrid.Visibility = Visibility.Hidden;
		}

		// Переключение видимости WPF Grid'ов при выборе вкладки "Графики"
		// Grid расчетов - скрыт, Grid результатов - скрыт, Grid графиков - показан
		private void GraphButton_Click(object sender, RoutedEventArgs e)
		{
			Footer.Text = "Графики выходных параметров.";
			CalcGrid.Visibility = Visibility.Hidden;
			ResultGrid.Visibility = Visibility.Hidden;
			GraphGrid.Visibility = Visibility.Visible;
		}

		// ----------------------------------------------------------------------------------------------------------
		// КНОПКА "РАССЧИТАТЬ"
		// ----------------------------------------------------------------------------------------------------------
		// Нажата кнопка РАССЧИТАТЬ
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
			double[] T, Eta;
			Stopwatch AlgoritmTime;
			String Time;

			// Инициализация мат. модели
			DefaultModel = new MathModel(H_TBox.Text, W_TBox.Text, Vu_TBox.Text,Mu0_TBox.Text, N_TBox.Text, L_TBox.Text,
				DeltaL_TBox.Text, B_TBox.Text, Tr_TBox.Text, Tu_TBox.Text, Alpha_TBox.Text, Ro_TBox.Text, C_TBox.Text, Tm_TBox.Text);

			// Если исходная строка ошибки изменялась (к ней прибавлялись перечисления ошибочных параметров)
			if (!DefaultModel.GetError().Equals("Некорректные значения: "))
			{
				MessageBox.Show(DefaultModel.GetError(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			// В ином случае (строка ошибки не менялась от исходной) мы запускаем алгоритм
			else
			{
				try
				{
					// Инициализация таймера
					AlgoritmTime = new Stopwatch();
					AlgoritmTime.Start();
					DefaultModel.Algorithm();
					AlgoritmTime.Stop();
					Time = Convert.ToString(AlgoritmTime.ElapsedMilliseconds);
 
					// Получение массивов температур и вязкостей для таблицы и графиков
					T = DefaultModel.GetTI();
					Eta = DefaultModel.GetEtaI();

					// Объявление списка данных для заполнения таблицы
					List<DataGridItem> DataGridItemsList = new List<DataGridItem>();
					// Объявление списков точек на графике для показателей температуры и вязкости
					TemperaturePoints = new List<DataPoint>();
					ViscosityPoints = new List<DataPoint>();

					for (int i = 0; i < T.Length; i++)
					{
						// Заполнение таблицы рассчитанными значениями
						DataGridItemsList.Add(new DataGridItem() { id = Convert.ToDouble(i) / 10, T = T[i], Eta = Eta[i] });
						// Добавление точек на графики температуры и вязкости (отображаются через биндинг)
						TemperaturePoints.Add(new DataPoint(Convert.ToDouble(i) / 10, T[i]));
						ViscosityPoints.Add(new DataPoint(Convert.ToDouble(i) / 10, Eta[i]));
					}

					// Вывод рассчитанных значений температуры, вязкости и производительности в текстовые поля
					Tp_TBox.Text = Convert.ToString(DefaultModel.GetTp(), CultureInfo.InvariantCulture);
					EtaP_TBox.Text = Convert.ToString(DefaultModel.GetEtaP(), CultureInfo.InvariantCulture);
					G_TBox.Text = Convert.ToString(DefaultModel.GetG(), CultureInfo.InvariantCulture);

					// Привязка рассчитанных значений к таблице результатов
					ResultsGataGrid.ItemsSource = DataGridItemsList;
					// Название столбцов таблицы
					ResultsGataGrid.Columns[0].Header = "Длина, м";
					ResultsGataGrid.Columns[1].Header = "Температура, С";
					ResultsGataGrid.Columns[2].Header = "Вязкость, Па*с";

					// Вывод значений на графики
					var plotModel1 = new PlotModel();
					TemperatureLine.ItemsSource = TemperaturePoints;
					TemperatureLine.Color = Color.FromArgb(255, 67, 150, 0);
					TemperatureGraph.InvalidatePlot();
					TemperatureGraph.ResetAllAxes();

					ViscosityLine.ItemsSource = ViscosityPoints;
					ViscosityLine.Color = Color.FromArgb(255, 255, 130, 15);
					ViscosityGraph.InvalidatePlot();
					ViscosityGraph.ResetAllAxes();

					MessageBox.Show("Расчеты успешно произведены", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
					Footer.Text = "Готово. Время расчета: " + Time + " мс.";

					// Активация кнопок с результатами и графиками
					ResultsButton.IsEnabled = true;
					GraphButton.IsEnabled = true;

					// Переключение на вкладку с результатами
					CalcGrid.Visibility = Visibility.Hidden;
					ResultGrid.Visibility = Visibility.Visible;

					// Обозначаем, что алгоритм расчета был запущен без ошибок (теперь можно сохранять отчет)
					AlgorithmLaunched = true;
				}
				catch
				{
					MessageBox.Show("Ошибка при выполнении алгоритма", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					DefaultModel = null;
				}
			}
			DefaultModel = null;
        }

		// ----------------------------------------------------------------------------------------------------------
		// ПОЛЕ ВЫБОРА МАТЕРИАЛА
		// ----------------------------------------------------------------------------------------------------------
		// Запрос информации о выбранном материале из базы данных
		private void MaterialTypeCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SQLiteDataReader rd = Database._instance.GetMaterialSettings(Convert.ToInt32(MaterialTypeCombox.SelectedValue));
			rd.Read();
			// Плотность
			Ro_TBox.Text = rd["Density"].ToString();
			// Теплоемкость
			C_TBox.Text = rd["HeatCapacity"].ToString();
			// Температура плавления
			Tm_TBox.Text = rd["TemperatureMelting"].ToString();
		}

		// ----------------------------------------------------------------------------------------------------------
		// 
		// ----------------------------------------------------------------------------------------------------------
		// FILE MENU ////////////////////////////////////////////////////////////////////////////////////////
		// Save report
		private void SaveReport(object sender, ExecutedRoutedEventArgs e)
		{
			// \/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
			// \/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
			// \/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
			MessageBox.Show("Insert report saving routines here");
			// \/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
			// \/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
			// \/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
		}
		private void CanSaveReport(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = AlgorithmLaunched;
		}
		// Exit the application (close current window)
		private void AppExit(object sender, ExecutedRoutedEventArgs e)
		{
			Window SenderWindow = (Window)sender;
			SenderWindow.Close();
		}
		private void CanAppExit(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		// TOOLS MENU ///////////////////////////////////////////////////////////////////////////////////////
		// Close current window and open login window
		private void ChangeUser(object sender, ExecutedRoutedEventArgs e)
		{
			// Открываем окно авторизации и делаем основное окно неактивным (активируем окно авторизации),
			// чтобы оно не спрашивало подтверждения о закрытии
			Login LoginW = new Login();
			LoginW.Show();
			LoginW.Activate();

			// Получаем указатель на главное окно и закрываем его
			Window SenderWindow = (Window)sender;
			SenderWindow.Close();
		}
		private void CanChangeUser(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		// HELP MENU////////////////////////////////////////////////////////////////////////////////////////
		// Открытие окна "О программе"
		private void MenuAbout_Click(object sender, RoutedEventArgs e)
		{
			AboutWindow AbWin = new AboutWindow();
			AbWin.ShowDialog();
		}

		// ADMIN MENU //////////////////////////////////////////////////////////////////////////////////////
		// Открытие окна смены пароля администратора
		private void AdminChangePWMenu_Click(object sender, RoutedEventArgs e)
		{
			AdminPassword AdmPWWindow = new AdminPassword();
			AdmPWWindow.ShowDialog();
		}
		// Открытие окна редактирования базы данных
		private void AdminEditDBMenu_Click(object sender, RoutedEventArgs e)
		{
			AdminDatabase AdmDBWIndow = new AdminDatabase();
			AdmDBWIndow.ShowDialog();
		}

		// Вывод диалогового окна с подтверждением при закрытии программы
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.IsActive == true)
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
	}
}
