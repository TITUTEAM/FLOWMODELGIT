using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SQLite;

using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Globalization;

namespace FLOWMODEL
{
	// Структура с описанием материала
	struct Materials
	{
		public int ID { get { return _id; } set { _id = value; } }
		int _id;

		public string Name { get { return _name; } set { _name = value; } }
		string _name;

		public Materials(int _id, string _name)
		{
			this._id = _id;
			this._name = _name;
		}
	}

	class Database : INotifyPropertyChanged
	{
		private SQLiteConnection dbConnection;

		// Превращение класса в singleton-класс с помощью паттерна
		public static readonly Database _instance = new Database();
		Database() { }
		
		// Список структур материалов
		List<Materials> MaterialsList = new List<Materials>();

		// Строка с описанием материала для Tooltip у Combobox выбора материала, класс Database назначается как DataContext
		// Для этого Combobox, а сам Combobox биндится в XAML на эту строку. Строка меняется в методе GetMaterialSettings
		// и автоматически обновляет Tooltip у Combobox. Это нужно, чтобы вставлять длинный текст описания в Tooltip, у
		// которого вручную через стили назначена максимальная ширина (300)
		private string _matDescr;
		public string MatDescr { get { return _matDescr; } set { _matDescr = value; this.NotifyPropertyChanged("MatDescr"); } }

		private String ParseErrorString = "";

		// Подключение к файлу базы данных
		public void Connect()
		{
			if (dbConnection == null)
			{
				dbConnection = new SQLiteConnection(@"DataSource=..\..\Database\FlowmodelDB.db3;Version=3;foreign keys=true");
				try
				{
					dbConnection.Open();
				}
				catch (SQLiteException ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}
		// -------------------------------------------------------------------------------------------------------------

		// Закрытие подключения к файлу базы данных
		public void Close()
		{
			if(dbConnection != null && dbConnection.State == ConnectionState.Open)
				dbConnection.Close();
		}
		// -------------------------------------------------------------------------------------------------------------

		// Получаем из таблицы материалов информацию о всех ID и названиях материалов для Combobox контрола MaterialSettings
		public void GetMaterials(object TargetCombobox)
		{
			ComboBox cb = (ComboBox)TargetCombobox;
			cb.SelectedIndex = -1;
			cb.ItemsSource = null;
			cb.Items.Clear();
			cb.DataContext = this;
			//(TargetCombobox as ComboBox).Items.Clear();

			MaterialsList.Clear();

			SQLiteCommand command = new SQLiteCommand("SELECT * FROM Materials", dbConnection);
			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				Console.WriteLine(reader["ID"] + " " + reader["Name"]);
				MaterialsList.Add(new Materials(Convert.ToInt32(reader["ID"]), (string)reader["Name"]));
			}

			cb.ItemsSource = MaterialsList;
			cb.SelectedValuePath = "ID";
			cb.DisplayMemberPath = "Name";
			cb.SelectedIndex = 0;
		}
		// -------------------------------------------------------------------------------------------------------------

		// Получаем информацию о свойствах материала из таблицы материалов
		// Выбор материала основывается на SelectedValue у Combobox контрола MaterialSettings
		public SQLiteDataReader GetMaterialSettings(int materialID)
		{
			SQLiteCommand command = new SQLiteCommand("SELECT * FROM [Materials] WHERE [Materials].[ID] = " + materialID, dbConnection);
			return command.ExecuteReader();
		}
		// -------------------------------------------------------------------------------------------------------------

		// Получаем настройки математической модели из таблицы мат. моделей
		// Выбор материала основывается на SelectedValue у Combobox контрола MaterialSettings
		public SQLiteDataReader GetMaterialMathmodelSettings(int materialID)
		{
			SQLiteCommand command = new SQLiteCommand("SELECT * FROM [MathModels] WHERE [MathModels].[MaterialID] = " + materialID, dbConnection);
			return command.ExecuteReader();
		}
		// -------------------------------------------------------------------------------------------------------------

		// Добавляем новый материал в базу данных
		public bool AddNewMaterial(MaterialSettings MaterialSettingsControl)
		{
			int MatAddedRows, MathModelAddedRows;
			int MatRowsWithSimilarName;
			// Обнуляем строку с текстом ошибки
			ParseErrorString = "";

			// Поле названия материала не должно быть пустым
			if (MaterialSettingsControl.MatName.Text != "")
			{
				// Делаем запрос в базу, чтобы проверить, нет ли в базе уже точно такого же материала с таким же названием
				// Если есть - выдаем ошибку
				using (SQLiteCommand command = new SQLiteCommand(dbConnection))
				{
					command.CommandText = "SELECT count(*) FROM Materials WHERE Name LIKE '%" + MaterialSettingsControl.MatName.Text + "%'";
					MatRowsWithSimilarName = Convert.ToInt32(command.ExecuteScalar());
				}
				// Число строк в таблице материалов с таким же названием должно быть равно нулю
				if (MatRowsWithSimilarName == 0)
				{
					try
					{
						using (SQLiteTransaction AddMatTransaction = dbConnection.BeginTransaction())
						{
							using (SQLiteCommand command = new SQLiteCommand(dbConnection))
							{
								// Начинаем все команды внутри одной транзакции
								command.Transaction = AddMatTransaction;

								// Команда вставки свойств материала в таблицу материалов
								command.CommandText = "INSERT INTO Materials(ID, Name, Descr, Density, HeatCapacity, TemperatureMelting) VALUES(@ID, @Name, @Descr, @Density, @HeatCapacity, @TemperatureMelting)";
								command.Prepare();
								command.Parameters.AddWithValue("@ID", null);
								command.Parameters.AddWithValue("@Name", MaterialSettingsControl.MatName.Text);
								command.Parameters.AddWithValue("@Descr", MaterialSettingsControl.MatDescr.Text);
								command.Parameters.AddWithValue("@Density", DoubleFromString(MaterialSettingsControl.Ro_TBox.Text, App.Current.Resources["ModelMaterialDensity"].ToString()));
								command.Parameters.AddWithValue("@HeatCapacity", DoubleFromString(MaterialSettingsControl.C_TBox.Text, App.Current.Resources["ModelMaterialHeatCapacity"].ToString()));
								command.Parameters.AddWithValue("@TemperatureMelting", DoubleFromString(MaterialSettingsControl.Tm_TBox.Text, App.Current.Resources["ModelMaterialTemperatureMelting"].ToString()));
								MatAddedRows = command.ExecuteNonQuery();

								// У добавленного материала находим в таблице материалов присвоенный ему ID (по имени)
								command.CommandText = "SELECT [Materials].[ID] FROM [Materials] WHERE [Materials].[Name] = '" + MaterialSettingsControl.MatName.Text + "'";
								SQLiteDataReader reader = command.ExecuteReader();
								reader.Read();
								int MatAddedID = reader.GetInt32(0);
								reader.Close();

								// Добавляем параметры математической модели в таблицу вместе с найденным ID материала
								command.CommandText = "INSERT INTO MathModels(ID, MaterialID, ConsistenceCoef, TemperatureCoef, TemperatureReduction, FluidIndex, HeatIrradianceCoef) VALUES(@ID, @MaterialID, @ConsistenceCoef, @TemperatureCoef, @TemperatureReduction, @FluidIndex, @HeatIrradianceCoef)";
								command.Prepare();
								command.Parameters.AddWithValue("@ID", null);
								command.Parameters.AddWithValue("@MaterialID", MatAddedID);
								command.Parameters.AddWithValue("@ConsistenceCoef", DoubleFromString(MaterialSettingsControl.Mu0_TBox.Text, App.Current.Resources["ModelConsistenceCoef"].ToString()));
								command.Parameters.AddWithValue("@TemperatureCoef", DoubleFromString(MaterialSettingsControl.B_TBox.Text, App.Current.Resources["ModelTemperatureCoef"].ToString()));
								command.Parameters.AddWithValue("@TemperatureReduction", DoubleFromString(MaterialSettingsControl.Tr_TBox.Text, App.Current.Resources["ModelTemperatureReduction"].ToString()));
								command.Parameters.AddWithValue("@FluidIndex", DoubleFromString(MaterialSettingsControl.N_TBox.Text, App.Current.Resources["ModelFluidIndex"].ToString()));
								command.Parameters.AddWithValue("@HeatIrradianceCoef", DoubleFromString(MaterialSettingsControl.Alpha_TBox.Text, App.Current.Resources["ModelHeatIrradiance"].ToString()));
								MathModelAddedRows = command.ExecuteNonQuery();
							}

							// Если были добавлены обе строки в базу и строка ошибки пустая - успех
							if (MatAddedRows * MathModelAddedRows != 0 && ParseErrorString.Length == 0)
							{
								AddMatTransaction.Commit();
								MessageBox.Show(App.Current.Resources["MessageMaterialAddSuccess"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Information);
								return true;
							}
							else
							{
								AddMatTransaction.Rollback();
								MessageBox.Show(App.Current.Resources["MessageWrongParameters"].ToString() + ParseErrorString, "", MessageBoxButton.OK, MessageBoxImage.Error);
								return false;
							}
						}
					}
					catch (SQLiteException ex)
					{
						// Если транзакция не прошла - выдаем ошибку
						MessageBox.Show("Error: \n " + ex.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
						return false;
					}
				}
				else
				{
					MessageBox.Show(App.Current.Resources["MessageMaterialErrorExists"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
					return false;
				}				
			}
			else
			{
				MessageBox.Show(App.Current.Resources["MessageMaterialErrorName"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}
		// -------------------------------------------------------------------------------------------------------------

		// Удаляем выбранный в Combobox у контрола MaterialSettingsControl материал из базы данных
		public bool DeleteMaterial(MaterialSettings MaterialSettingsControl)
		{
			if (MaterialSettingsControl.MaterialTypeCombox.SelectedItem != null)
			{
				using (SQLiteCommand command = new SQLiteCommand(dbConnection))
				{
					int SelectedMaterialID = Convert.ToInt32(MaterialSettingsControl.MaterialTypeCombox.SelectedValue);
					command.CommandText = "DELETE FROM Materials WHERE ID = " + SelectedMaterialID;
					int DeletedRows = command.ExecuteNonQuery();

					if (DeletedRows > 0)
					{
						MessageBox.Show(App.Current.Resources["MessageMaterialDeleteSuccess"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Information);
						return true;
					}
					else
					{
						MessageBox.Show(App.Current.Resources["MessageMaterialDeleteError"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
						return false;
					}
				}
			}
			return false;
		}
		// -------------------------------------------------------------------------------------------------------------

		// Редактируем выбранный в Combobox у контрола MaterialSettingsControl материал из базы данных
		public void EditMaterial(MaterialSettings MaterialSettingsControl)
		{
			int MatEditedRows, MathmodelEditedRows;
			// Обнуляем строку с текстом ошибки
			ParseErrorString = "";

			if (MaterialSettingsControl.MaterialTypeCombox.SelectedItem != null)
			{
				try
				{
					using (SQLiteTransaction EditMatTransaction = dbConnection.BeginTransaction())
					{
						using (SQLiteCommand command = new SQLiteCommand(dbConnection))
						{
							// Начинаем все команды внутри одной транзакции
							command.Transaction = EditMatTransaction;

							// Обновляем свойства выбранного материала по его ID
							int SelectedMaterialID = Convert.ToInt32(MaterialSettingsControl.MaterialTypeCombox.SelectedValue);
							command.CommandText = "UPDATE [Materials] SET [Density]=@Density, [HeatCapacity]=@HeatCapacity, [TemperatureMelting]=@TemperatureMelting WHERE [ID] = " + SelectedMaterialID;
							command.Prepare();
							command.Parameters.AddWithValue("@Density", DoubleFromString(MaterialSettingsControl.Ro_TBox.Text, App.Current.Resources["ModelMaterialDensity"].ToString()));
							command.Parameters.AddWithValue("@HeatCapacity", DoubleFromString(MaterialSettingsControl.C_TBox.Text, App.Current.Resources["ModelMaterialHeatCapacity"].ToString()));
							command.Parameters.AddWithValue("@TemperatureMelting", DoubleFromString(MaterialSettingsControl.Tm_TBox.Text, App.Current.Resources["ModelMaterialTemperatureMelting"].ToString()));
							MatEditedRows = command.ExecuteNonQuery();

							// Обновляем параметры математической модели для выбранного ID материала
							command.CommandText = "UPDATE [Mathmodels] SET [ConsistenceCoef]=@ConsistenceCoef, [TemperatureCoef]=@TemperatureCoef, [TemperatureReduction]=@TemperatureReduction, [FluidIndex]=@FluidIndex, [HeatIrradianceCoef]=@HeatIrradianceCoef WHERE [MaterialID] = " + SelectedMaterialID;
							command.Prepare();
							command.Parameters.AddWithValue("@ConsistenceCoef", DoubleFromString(MaterialSettingsControl.Mu0_TBox.Text, App.Current.Resources["ModelConsistenceCoef"].ToString()));
							command.Parameters.AddWithValue("@TemperatureCoef", DoubleFromString(MaterialSettingsControl.B_TBox.Text, App.Current.Resources["ModelTemperatureCoef"].ToString()));
							command.Parameters.AddWithValue("@TemperatureReduction", DoubleFromString(MaterialSettingsControl.Tr_TBox.Text, App.Current.Resources["ModelTemperatureReduction"].ToString()));
							command.Parameters.AddWithValue("@FluidIndex", DoubleFromString(MaterialSettingsControl.N_TBox.Text, App.Current.Resources["ModelFluidIndex"].ToString()));
							command.Parameters.AddWithValue("@HeatIrradianceCoef", DoubleFromString(MaterialSettingsControl.Alpha_TBox.Text, App.Current.Resources["ModelHeatIrradiance"].ToString()));
							MathmodelEditedRows = command.ExecuteNonQuery();
						}

						// Если обе строки в базе были изменены и строка ошибки пустая - успех
						if (MatEditedRows * MathmodelEditedRows != 0  && ParseErrorString.Length == 0)
						{
							EditMatTransaction.Commit();
							MessageBox.Show(App.Current.Resources["MessageMaterialSaved"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Information);
						}
						else
						{
							EditMatTransaction.Rollback();
							MessageBox.Show(App.Current.Resources["MessageWrongParameters"].ToString() + ParseErrorString, "", MessageBoxButton.OK, MessageBoxImage.Error);
						}

					}
				}
				catch (SQLiteException ex)
				{
					// Если транзакция не прошла - выдаем ошибку
					MessageBox.Show("Error: \n " + ex.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
		// -------------------------------------------------------------------------------------------------------------

		// Проверка введенного пароля при авторизации на соответствие паролю в базе данных
		public bool CheckPassword(string pw)
		{
			string pw_check;

			using (SQLiteCommand command = new SQLiteCommand(dbConnection))
			{
				command.CommandText = "SELECT [Account].[Stored] FROM [Account] LIMIT 1";

				using (SQLiteDataReader reader = command.ExecuteReader())
				{
					reader.Read();
					pw_check = reader["Stored"].ToString();
				}
			}

			return (pw_check == pw);
		}
		// -------------------------------------------------------------------------------------------------------------

		// Установка нового пароля администратора
		public bool SetAdminPassword(string pword)
		{
			int QueryReturn = 0;

			// Сравниваем пароли, если введенный не совпадает с паролем в базе - обновляем запись о пароле
			if (!CheckPassword(pword))
			{
				using (SQLiteCommand command = new SQLiteCommand(dbConnection))
				{
					command.CommandText = "UPDATE [Account] SET [Stored]=@SetPw";
					command.Prepare();
					command.Parameters.AddWithValue("@SetPw", pword);
					QueryReturn = command.ExecuteNonQuery();
				}
				if (QueryReturn != 0)
				{
					MessageBox.Show(App.Current.Resources["MessagePasswordSuccess"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Information);
					return true;
				}
			}
			else
			{
				MessageBox.Show(App.Current.Resources["MessagePasswordExists"].ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			return false;
		}
		// -------------------------------------------------------------------------------------------------------------

		// Преобразуем параметр из строки, если преобразовать не удается (неверный формат или ошибка ввода)
		// Добавляем название параметра в строку ошибки ParseErrorString
		public double DoubleFromString(string InVar, string ParamName)
		{
			double OutVar = 0.0;
			// Сначала преобразуем из формата компьютера пользователя
			if (!double.TryParse(InVar, NumberStyles.Any, CultureInfo.CurrentCulture, out OutVar))
			{
				// Если не получилось - преобразуем из инвариантного формата
				try
				{
					OutVar = Double.Parse(InVar, CultureInfo.InvariantCulture);
				}
				// Если и тут не получилось - записываем название параметра в строку со списком ошибок
				catch
				{
					ParseErrorString += "\n "+ParamName;
				}
			}
			return OutVar;
		}
		// -------------------------------------------------------------------------------------------------------------

		// Событие, оповещающее об изменении значения одной из публичных переменных
		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string propName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}
		// -------------------------------------------------------------------------------------------------------------
	}
}
