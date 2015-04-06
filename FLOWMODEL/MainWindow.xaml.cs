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

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
			String errorMessage;
			double[] T, Eta;

			try
			{
				// Инициализация мат. модели
				DefaultModel = new MathModel(
					double.Parse(D_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(H_TBox.Text, CultureInfo.InvariantCulture), 
					double.Parse(Vu_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(Mu0_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(N_TBox.Text, CultureInfo.InvariantCulture),
					double.Parse(W_TBox.Text, CultureInfo.InvariantCulture),
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

				if (!errorMessage.Equals("Некорректные значения: "))
				{
					MessageBox.Show(errorMessage.Substring(0, errorMessage.Length - 2), "Error");
					DefaultModel = null;
				}
				else
				{
					try
					{
						DefaultModel.Algorithm();

						Tp_TBox.Text = Convert.ToString(DefaultModel.GetTp());
						EtaP_TBox.Text = Convert.ToString(DefaultModel.GetEtaP());
						G_TBox.Text = Convert.ToString(DefaultModel.GetG());

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

						MessageBox.Show("Рассчеты успешно произведены", "Готово");

						CalcGrid.Visibility = Visibility.Hidden;
						ResultGrid.Visibility = Visibility.Visible;
					}
					catch
					{
						MessageBox.Show("Ошибка при выполнении алгоритма", "Error");
						DefaultModel = null;
					}
				}
			}
				catch
			{
				// Если в заданных значениях присутствуют некореекстные значения: буквы, знаки, кидаем ошибку
				MessageBox.Show("Неправильный формат данных", "Error");
				DefaultModel = null;
			}

			DefaultModel = null;
        }
	}
}
