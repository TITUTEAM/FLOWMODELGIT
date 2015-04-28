using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SQLite;

using System.Windows;
using System.Windows.Controls;

namespace FLOWMODEL
{
	// Структура с описанием материала
	struct Materials
	{
		public int ID { get { return _id; } set { _id = value; } }
		int _id;

		public string Name { get { return _name; } set { _name = value; } }
		string _name;

		public string Desc { get { return _desc; } set { _desc = value; } }
		string _desc;

		public Materials(int _id, string _name, string _desc)
		{
			this._id = _id;
			this._name = _name;
			this._desc = _desc;
		}
	}

	class Database
	{
		private SQLiteConnection dbConnection;

		// Превращение класса в singleton-класс с помощью паттерна
		public static readonly Database _instance = new Database();
		Database() { }

		// Список структур материалов
		List<Materials> MaterialsList = new List<Materials>();

		public void Connect()
		{
			dbConnection = new SQLiteConnection(@"DataSource=..\..\Database\FlowmodelDB.db3;Version=3;");
			try
			{
				dbConnection.Open();
			}
			catch (SQLiteException ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public void Close()
		{
			dbConnection.Close();
		}

		public void GetMaterials(object TargetCombobox)
		{
			ComboBox cb = (ComboBox)TargetCombobox;
			cb.Items.Clear();
			MaterialsList.Clear();

			SQLiteCommand command = new SQLiteCommand("SELECT * FROM Materials", dbConnection);
			SQLiteDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				Console.WriteLine(reader["ID"] + " " + reader["Name"] + " " + reader["Description"]);
				MaterialsList.Add(new Materials(Convert.ToInt32(reader["ID"]), (string)reader["Name"], (string)reader["Description"]));
			}

			cb.ItemsSource = MaterialsList;
			cb.DisplayMemberPath = "Name";
			cb.SelectedValuePath = "ID";
			cb.SelectedIndex = 0;
		}

		public SQLiteDataReader GetMaterialSettings(int materialID)
		{
			SQLiteCommand command = new SQLiteCommand("SELECT * FROM [Parameters] WHERE [Parameters].[MaterialID] = " + materialID, dbConnection);
			return command.ExecuteReader();
		}

		public bool CheckPassword(string pw)
		{
			this.Connect();
			SQLiteCommand command = new SQLiteCommand("SELECT [Account].[Stored] FROM [Account] LIMIT 1", dbConnection);
			SQLiteDataReader reader = command.ExecuteReader();
			reader.Read();
			string pw_check = reader["Stored"].ToString();
			this.Close();

			return (pw_check == pw);
		}
	}
}
