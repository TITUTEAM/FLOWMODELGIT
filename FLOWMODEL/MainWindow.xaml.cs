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
using Microsoft.Win32;

using OxyPlot;

using System.Data;
using System.Data.SQLite;
using System.IO;
using OxyPlot.Wpf;

namespace FLOWMODEL
{
	public partial class MainWindow : Window
	{
		public IList<DataPoint> TemperaturePoints { get; set; }
		public IList<DataPoint> ViscosityPoints { get; set; }

        private MathModel DefaultModel;

		bool AlgorithmLaunched = false;
		string FooterLastUsedResourceKey, FooterLastAdditionalString;
		
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
			// --------------------------------------------------------------------------------------------------

			// Обновление списка материалов из базы для Combobox
			Database._instance.GetMaterials(MatSettingsControl.MaterialTypeCombox);

			// Скрытие\показ пункта меню администратора в зависимости от режима
			if (AdminMode == true)
			{
				// Показываем меню администратора
				AdminMenu.Visibility = Visibility.Visible;
				// Показываем вкладки администратора
				AdminMenuGrid.Visibility = Visibility.Visible;
				// Сворачиваем все вкладки оператора
				OperatorMenuGrid.Visibility = Visibility.Collapsed;

				// Сворачиваем все режимные и геометрические параметры
				OperatorCalcParametersGrid.Visibility = Visibility.Collapsed;

				// Показываем кнопки администратора (Добавить, Удалить, Сохранить)
				AdminButtons.Visibility = Visibility.Visible;
				// Скрываем кнопки Удалить и Сохранить и показываем кнопку Добавить
				MatDeleteButton.Visibility = Visibility.Hidden;
				MatSaveButton.Visibility = Visibility.Hidden;
				MatAddButton.Visibility = Visibility.Visible;
				// Скрываем кнопку расчета
				CalculateButton.Visibility = Visibility.Collapsed;

				// Показываем контрол добавления материала
				AddMaterialControl.Visibility = Visibility.Visible;
				// Скрываем контрол редактирования материала
				MatSettingsControl.Visibility = Visibility.Collapsed;

				// Вставка изначального текста внизу окна при его открытии
				FooterSetLocalizedText("FooterAdminWelcome");

				AlgorithmLaunched = false;
			}
			else
			{
				// Скрываем меню администратора
				AdminMenu.Visibility = Visibility.Hidden;
				// Сворачиваем все вкладки администратора
				AdminMenuGrid.Visibility = Visibility.Collapsed;
				// Показываем вкладки оператора
				OperatorMenuGrid.Visibility = Visibility.Visible;
				// Скрываем контрол добавления материала
				AddMaterialControl.Visibility = Visibility.Hidden;
				// Скрываем кнопки администратора (Добавить, Удалить, Сохранить)
				AdminButtons.Visibility = Visibility.Hidden;
				// Вставка изначального текста внизу окна при его открытии
				FooterSetLocalizedText("FooterOperatorWelcome");
			}
		}

		// ----------------------------------------------------------------------------------------------------------
		// ВКЛАДКИ ОПЕРАТОРА
		// ----------------------------------------------------------------------------------------------------------
		// Переключение видимости WPF Grid'ов при выборе вкладки "Расчет"
		// Grid расчетов - показан, Grid результатов - скрыт, Grid графиков - скрыт
		private void CalculationsButton_Click(object sender, RoutedEventArgs e)
		{
			FooterSetLocalizedText("FooterCalcTabSelected");
			CalcGrid.Visibility = Visibility.Visible;
			ResultGrid.Visibility = Visibility.Hidden;
			GraphGrid.Visibility = Visibility.Hidden;
		}

		// Переключение видимости WPF Grid'ов при выборе вкладки "Результаты"
		// Grid расчетов - скрыт, Grid результатов - показан, Grid графиков - скрыт
		private void ResultsButton_Click(object sender, RoutedEventArgs e)
		{
			FooterSetLocalizedText("FooterResultsTabSelected");
			CalcGrid.Visibility = Visibility.Hidden;
			ResultGrid.Visibility = Visibility.Visible;
			GraphGrid.Visibility = Visibility.Hidden;
		}

		// Переключение видимости WPF Grid'ов при выборе вкладки "Графики"
		// Grid расчетов - скрыт, Grid результатов - скрыт, Grid графиков - показан
		private void GraphButton_Click(object sender, RoutedEventArgs e)
		{
			FooterSetLocalizedText("FooterGraphTabSelected");
			CalcGrid.Visibility = Visibility.Hidden;
			ResultGrid.Visibility = Visibility.Hidden;
			GraphGrid.Visibility = Visibility.Visible;
		}

		// ----------------------------------------------------------------------------------------------------------
		// ВКЛАДКИ АДМИНИСТРАТОРА
		// ----------------------------------------------------------------------------------------------------------
		// Переключение видимости CustomControl'ов при выборе вкладки "Добавить материал"
		// CustomControl нового материала - показан, CustomControl редактирования - скрыт
		private void NewMaterialButton_Click(object sender, RoutedEventArgs e)
		{
			FooterSetLocalizedText("FooterNewMatTabSelected");
			// Показываем контрол добавления материала
			AddMaterialControl.Visibility = Visibility.Visible;
			// Скрываем контрол редактирования материала
			MatSettingsControl.Visibility = Visibility.Collapsed;
			// Скрываем кнопки Удалить и Сохранить и показываем кнопку Добавить
			MatDeleteButton.Visibility = Visibility.Hidden;
			MatSaveButton.Visibility = Visibility.Hidden;
			MatAddButton.Visibility = Visibility.Visible;
		}
		// Переключение видимости CustomControl'ов при выборе вкладки "Редактировать материалы"
		// CustomControl нового материала - скрыт, CustomControl редактирования - показан
		private void EditMaterialButton_Click(object sender, RoutedEventArgs e)
		{
			FooterSetLocalizedText("FooterEditMatTabSelected");
			// Показываем контрол добавления материала
			AddMaterialControl.Visibility = Visibility.Collapsed;
			// Скрываем контрол редактирования материала
			MatSettingsControl.Visibility = Visibility.Visible;
			// Показываем кнопки Удалить и Сохранить и скрываем кнопку Добавить
			MatDeleteButton.Visibility = Visibility.Visible;
			MatSaveButton.Visibility = Visibility.Visible;
			MatAddButton.Visibility = Visibility.Hidden;
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
			DefaultModel = new MathModel
				(
				H_TBox.Text,
				W_TBox.Text,
				Vu_TBox.Text,
				MatSettingsControl.Mu0_TBox.Text,
				MatSettingsControl.N_TBox.Text,
				L_TBox.Text,
				DeltaL_TBox.Text,
				MatSettingsControl.B_TBox.Text,
				MatSettingsControl.Tr_TBox.Text,
				Tu_TBox.Text,
				MatSettingsControl.Alpha_TBox.Text,
				MatSettingsControl.Ro_TBox.Text,
				MatSettingsControl.C_TBox.Text,
				MatSettingsControl.Tm_TBox.Text
				);

			// Если строка ошибки не пустая (ее длина > 0)
			if (DefaultModel.GetError().Length > 0)
			{
				MessageBox.Show(App.Current.Resources["MessageWrongParameters"].ToString() + DefaultModel.GetError(), "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			// В ином случае (длина строки с текстом ошибки = 0) мы запускаем алгоритм
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

					// Вывод значений на графики
					TemperatureLine.ItemsSource = TemperaturePoints;
					TemperatureLine.Color = Color.FromArgb(255, 67, 150, 0);
					TemperatureGraph.InvalidatePlot();
					TemperatureGraph.ResetAllAxes();

					ViscosityLine.ItemsSource = ViscosityPoints;
					ViscosityLine.Color = Color.FromArgb(255, 255, 130, 15);
					ViscosityGraph.InvalidatePlot();
					ViscosityGraph.ResetAllAxes();

					MessageBox.Show(App.Current.Resources["MessageCalculationSuccess"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Information);
					FooterSetLocalizedText("FooterCalcSuccess", " "+Time);

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
					MessageBox.Show(App.Current.Resources["MessageCalculationError"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
        }

		// ----------------------------------------------------------------------------------------------------------
		// КНОПКА "Добавить"
		// ----------------------------------------------------------------------------------------------------------
		private void MatAddButton_Click(object sender, RoutedEventArgs e)
		{
			// Добавляем новый материал в базу
			if (Database._instance.AddNewMaterial(AddMaterialControl))
			{
				// Если материал успешно добавлен:
				// Обновляем список материалов из базы для Combobox
				Database._instance.GetMaterials(MatSettingsControl.MaterialTypeCombox);
				MatSettingsControl.MaterialTypeCombox.SelectedIndex = 0;
			}
		}

		// ----------------------------------------------------------------------------------------------------------
		// КНОПКА "Удалить"
		// ----------------------------------------------------------------------------------------------------------
		private void MatDeleteButton_Click(object sender, RoutedEventArgs e)
		{
			// Удаляем выбранный в Combobox материал из базы
			if (Database._instance.DeleteMaterial(MatSettingsControl))
			{
				// Если материал успешно удален:
				// Обновляем список материалов из базы для Combobox
				Database._instance.GetMaterials(MatSettingsControl.MaterialTypeCombox);
				MatSettingsControl.MaterialTypeCombox.SelectedIndex = 0;
			}
		}

		// ----------------------------------------------------------------------------------------------------------
		// КНОПКА "Сохранить"
		// ----------------------------------------------------------------------------------------------------------
		private void MatSaveButton_Click(object sender, RoutedEventArgs e)
		{
			// Сохраняем измененные параметры для выбранного в Combobox материала
			Database._instance.EditMaterial(MatSettingsControl);
		}

		// ----------------------------------------------------------------------------------------------------------
		// ОСТАЛЬНЫЕ КОНТРОЛЫ
		// ----------------------------------------------------------------------------------------------------------
		// FILE MENU ////////////////////////////////////////////////////////////////////////////////////////
		// Сохранение отчета
		private void SaveReport(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
			var saveProgressWin = new SavingProgressWindow();

				SaveFileDialog sfd = new SaveFileDialog();
				sfd.FileName = App.Current.Resources["DefaultReportName"].ToString();
				sfd.DefaultExt = ".docx";
				sfd.Filter = "Word 2010 (*.docx)|*.docx|All files (*.*)|*.*";

				saveProgressWin.Show();
				saveProgressWin.Focus();

				if (sfd.ShowDialog() == true)
				{
					TemperatureGraph.SaveBitmap(sfd.FileName + "T.png", 600, 400, OxyColors.White);
					ViscosityGraph.SaveBitmap(sfd.FileName + "V.png", 600, 400, OxyColors.White);

					Report ReportGeneration = new Report(sfd.FileName, DefaultModel, MatSettingsControl.MaterialTypeCombox.Text);

					File.Delete(sfd.FileName + "T.png");
					File.Delete(sfd.FileName + "V.png");

					saveProgressWin.Close();
					this.Focus();
					MessageBox.Show(App.Current.Resources["MessageReportSaved"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					saveProgressWin.Close();
					this.Focus();
				}


			}
			catch
			{
				MessageBox.Show(App.Current.Resources["MessageReportError"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
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

			AlgorithmLaunched = false;

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

		// Вывод диалогового окна с подтверждением при закрытии программы
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.IsActive == true)
			{
				MessageBoxResult ExitResult = MessageBox.Show(App.Current.Resources["MessageExitConfirm"].ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Question);
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

		// Изменить язык интерфейса на русский
		private void SwitchToRussianMenuButton_Click(object sender, RoutedEventArgs e)
		{
			App.SelectCulture("ru-RU");
			UpdateFooterText();
			App.SaveSelectedLanguage("ru-RU");
		}

		// Изменить язык интерфейса на английский
		private void SwitchToEnglishMenuButton_Click(object sender, RoutedEventArgs e)
		{
			App.SelectCulture("en-US");
			UpdateFooterText();
			App.SaveSelectedLanguage("en-US");
		}

		// Применяем локализованный текст к подсказкам внизу окна
		private void FooterSetLocalizedText(string ResKey, string Additional = "")
		{
			FooterLastUsedResourceKey = ResKey;
			FooterLastAdditionalString = Additional;
			Footer.Text = App.Current.Resources[ResKey].ToString() + Additional;
		}
		// Обновляем текст подсказки по указанному ключу ресурса при смене языка
		private void UpdateFooterText()
		{
			Footer.Text = App.Current.Resources[FooterLastUsedResourceKey].ToString() + FooterLastAdditionalString;
		}

		private void MenuHelpShow_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"..\..\Help\Documentation.chm");
		}
	}
}
