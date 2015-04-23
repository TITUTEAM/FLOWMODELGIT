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

namespace FLOWMODEL
{
	public partial class MainWindow : Window
	{
        private MathModel DefaultModel;
		public MainWindow()
		{
			InitializeComponent();
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
		}

		// Нажата кнопка РАССЧИТАТЬ
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
			String errorMessage;
			double[] T, Eta;
			Stopwatch AlgoritmTime;
			String Time;
			String Error = "sdfsdf";

			// Инициализация мат. модели
			DefaultModel = new MathModel(H_TBox.Text, W_TBox.Text, Vu_TBox.Text,Mu0_TBox.Text, N_TBox.Text, L_TBox.Text,
				DeltaL_TBox.Text, B_TBox.Text, Tr_TBox.Text, Tu_TBox.Text, Alpha_TBox.Text, Ro_TBox.Text, C_TBox.Text, Tm_TBox.Text);

			// Если исходная строка ошибки изменялась (к ней прибавлялись
			// перечисления ошибочных параметров)
			if (!DefaultModel.GetError().Equals("Некорректные значения: "))
			{
				MessageBox.Show(DefaultModel.GetError(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			// В ином случае (строка ошибки не менялась от исходной) мы запускает алгоритм
			else
			{
				try
				{
					AlgoritmTime = new Stopwatch();
					AlgoritmTime.Start();
					DefaultModel.Algorithm();
					AlgoritmTime.Stop();
					Time = Convert.ToString(AlgoritmTime.ElapsedMilliseconds);

					Tp_TBox.Text = Convert.ToString(DefaultModel.GetTp(), CultureInfo.InvariantCulture);
					EtaP_TBox.Text = Convert.ToString(DefaultModel.GetEtaP(), CultureInfo.InvariantCulture);
					G_TBox.Text = Convert.ToString(DefaultModel.GetG(), CultureInfo.InvariantCulture);
 
					// Получение массивов температур и вязкостей для таблицы
					T = DefaultModel.GetTI();
					Eta = DefaultModel.GetEtaI();

					List<DataGridItem> list = new List<DataGridItem>();

					for (int i = 0; i < T.Length; i++)
					{
						list.Add(new DataGridItem() { id = Convert.ToDouble(i) / 10, T = T[i], Eta = Eta[i] });
					}

					ResultsGataGrid.ItemsSource = list;

					ResultsGataGrid.Columns[0].Header = "Длина, м";
					ResultsGataGrid.Columns[1].Header = "Температура, С";
					ResultsGataGrid.Columns[2].Header = "Вязкость, Па*с";

					MessageBox.Show("Рассчеты успешно произведены. \n Затраченное время: " + Time + " мс", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

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
	}
}
