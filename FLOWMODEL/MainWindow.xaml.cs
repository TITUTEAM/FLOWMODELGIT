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
		// Grid расчетов - показан, Grid результатов - скрыт
		private void CalculationsButton_Click(object sender, RoutedEventArgs e)
		{
			CalcGrid.Visibility = Visibility.Visible;
			ResultGrid.Visibility = Visibility.Hidden;
		}

		// Переключение видимости WPF Grid'ов при выборе вкладки "Результаты"
		// Grid расчетов - скрыт, Grid результатов - показан
		private void ResultsButton_Click(object sender, RoutedEventArgs e)
		{
			CalcGrid.Visibility = Visibility.Hidden;
			ResultGrid.Visibility = Visibility.Visible;
		}

		// Нажата кнопка РАССЧИТАТЬ
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
			String errorMessage;
			double[] T, Eta;
			Stopwatch AlgoritmTime;
			String time;
			try
			{
				// Инициализация мат. модели
				DefaultModel = new MathModel(
					double.Parse(H_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(W_TBox.Text, CultureInfo.InvariantCulture), 
					double.Parse(Vu_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(Mu0_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(N_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(L_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(DeltaL_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(B_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(Tr_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(Tu_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(Alpha_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(Ro_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(C_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(Tm_TBox.Text, CultureInfo.InvariantCulture));

				// Проверка корректных числовых значений на > 0.001
				errorMessage = DefaultModel.Check();
				// Если исходная строка ошибки изменялась (к ней прибавлялись
				// перечисления ошибочных параметров)
				if (!errorMessage.Equals("Некорректные значения: "))
				{
					MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					DefaultModel = null;
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
						time = Convert.ToString(AlgoritmTime.ElapsedMilliseconds);

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

						MessageBox.Show("Рассчеты успешно произведены. \n Затраченное время: " + time + " мс", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

						CalcGrid.Visibility = Visibility.Hidden;
						ResultGrid.Visibility = Visibility.Visible;
					}
					catch
					{
						MessageBox.Show("Ошибка при выполнении алгоритма", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
						DefaultModel = null;
					}
				}
			}
				catch
			{
				// Если в заданных значениях присутствуют некорректные значения: буквы, знаки, кидаем ошибку
				MessageBox.Show("Неправильный формат данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				DefaultModel = null;
			}

			DefaultModel = null;
        }
	}
}
