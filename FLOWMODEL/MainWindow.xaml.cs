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

		public MainWindow()
		{
			InitializeComponent();

			Database._instance.Connect();
			Database._instance.GetMaterials(MaterialTypeCombox);
		}

		// Вывод диалогового окна с подтверждением при закрытии программы
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

		// ----------------------------------------------------------------------------------------------------------
		// РАБОТА С ВКЛАДКАМИ
		// ----------------------------------------------------------------------------------------------------------
		// Переключение видимости WPF Grid'ов при выборе вкладки "Расчет"
		// Grid расчетов - показан, Grid результатов - скрыт, Grid графиков - скрыт
		private void CalculationsButton_Click(object sender, RoutedEventArgs e)
		{
			CalcGrid.Visibility = Visibility.Visible;
			ResultGrid.Visibility = Visibility.Hidden;
			GraphGrid.Visibility = Visibility.Hidden;
		}

		// Переключение видимости WPF Grid'ов при выборе вкладки "Результаты"
		// Grid расчетов - скрыт, Grid результатов - показан, Grid графиков - скрыт
		private void ResultsButton_Click(object sender, RoutedEventArgs e)
		{
			CalcGrid.Visibility = Visibility.Hidden;
			ResultGrid.Visibility = Visibility.Visible;
			GraphGrid.Visibility = Visibility.Hidden;
		}

		// Переключение видимости WPF Grid'ов при выборе вкладки "Графики"
		// Grid расчетов - скрыт, Grid результатов - скрыт, Grid графиков - показан
		private void GraphButton_Click(object sender, RoutedEventArgs e)
		{
			CalcGrid.Visibility = Visibility.Hidden;
			ResultGrid.Visibility = Visibility.Hidden;
			GraphGrid.Visibility = Visibility.Visible;

			var plotModel1 = new PlotModel();
			//ViscosityLine.ItemsSource = ViscosityPoints;
			//TemperatureLine.ItemsSource = TemperaturePoints;
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

					MessageBox.Show("Расчеты успешно произведены. \n Затраченное время: " + Time + " мс", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

					// Переключение на вкладку с результатами
					CalcGrid.Visibility = Visibility.Hidden;
					ResultGrid.Visibility = Visibility.Visible;
				}
				catch
				{
					MessageBox.Show("Ошибка при выполнении алгоритма", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					DefaultModel = null;
				}
			}
			DefaultModel = null;
        }

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
	}
}
