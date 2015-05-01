using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;
using System.Data.SQLite;
using System.Globalization;


namespace FLOWMODEL
{
	public partial class MaterialSettings : UserControl
	{
		// Переменная, определяющая вид контрола (показывать Combobox для выбора материала или
		// текстовое поле для ввода нового названия)
		public bool NewMaterial 
		{
			get { return (bool)GetValue(NewMaterialProperty); }
			set { SetValue(NewMaterialProperty, value); }
		}
		// Используем DependencyProperty как внешний параметр в NewMaterial, чтобы его можно было редактировать в XAML
		public static readonly DependencyProperty NewMaterialProperty =
			DependencyProperty.Register("NewMaterial", typeof(bool), typeof(MaterialSettings), new UIPropertyMetadata(false));

		public MaterialSettings()
		{
			InitializeComponent();
		}

		// На событии загрузки контрола (в дизайнере или в самой программе) настраиваем внешний вид контрола
		private void MaterialSettingsControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (NewMaterial == true)
			{
				MatNameHeader.Visibility = Visibility.Visible;
				MatTypeHeader.Visibility = Visibility.Collapsed;
				MaterialTypeCombox.Visibility = Visibility.Collapsed;
				MatName.Visibility = Visibility.Visible;

				MatDescrHeader.Visibility = Visibility.Visible;
				MatDescr.Visibility = Visibility.Visible;
			}
			else
			{
				MatNameHeader.Visibility = Visibility.Collapsed;
				MatTypeHeader.Visibility = Visibility.Visible;
				MaterialTypeCombox.Visibility = Visibility.Visible;
				MatName.Visibility = Visibility.Collapsed;

				MatDescrHeader.Visibility = Visibility.Collapsed;
				MatDescr.Visibility = Visibility.Collapsed;
			}
		}
		// -------------------------------------------------------------------------------------------------------------

		// -------------------------------------------------------------------------------------------------------------
		// ПОЛЕ ВЫБОРА МАТЕРИАЛА
		// -------------------------------------------------------------------------------------------------------------
		// Запрос информации о выбранном материале из базы данных
		private void MaterialTypeCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{			
			if (MaterialTypeCombox.Visibility == Visibility.Visible && MaterialTypeCombox.SelectedItem != null)
			{
				// Заносим в контрол параметры материала
				var MaterialReader = Database._instance.GetMaterialSettings(Convert.ToInt32(MaterialTypeCombox.SelectedValue));
				MaterialReader.Read();
				// Описание (заносим в Tooltip у Combobox через биндинг, см. больше в классе Database)
				Database._instance.MatDescr = MaterialReader["Descr"].ToString();
				// Плотность
				Ro_TBox.Text = double.Parse(MaterialReader["Density"].ToString()).ToString(CultureInfo.InvariantCulture);
				// Теплоемкость
				C_TBox.Text = double.Parse(MaterialReader["HeatCapacity"].ToString()).ToString(CultureInfo.InvariantCulture);
				// Температура плавления
				Tm_TBox.Text = double.Parse(MaterialReader["TemperatureMelting"].ToString()).ToString(CultureInfo.InvariantCulture);

				// Заносим в контрол параметры математической модели
				var MathmodelReader = Database._instance.GetMaterialMathmodelSettings(Convert.ToInt32(MaterialTypeCombox.SelectedValue));
				MathmodelReader.Read();
				// Коэффициент консистенции
				Mu0_TBox.Text = double.Parse(MathmodelReader["ConsistenceCoef"].ToString()).ToString(CultureInfo.InvariantCulture);
				// Температурный коэффициент вязкости
				B_TBox.Text = double.Parse(MathmodelReader["TemperatureCoef"].ToString()).ToString(CultureInfo.InvariantCulture);
				// Температура приведения
				Tr_TBox.Text = double.Parse(MathmodelReader["TemperatureReduction"].ToString()).ToString(CultureInfo.InvariantCulture);
				// Индекс течения материала
				N_TBox.Text = double.Parse(MathmodelReader["FluidIndex"].ToString()).ToString(CultureInfo.InvariantCulture);
				// Коэффициент теплоотдачи от крышки к материалу
				Alpha_TBox.Text = double.Parse(MathmodelReader["HeatIrradianceCoef"].ToString()).ToString(CultureInfo.InvariantCulture);
			}

		}
		// -------------------------------------------------------------------------------------------------------------
	}
}
