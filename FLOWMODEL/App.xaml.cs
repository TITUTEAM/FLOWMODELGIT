﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

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

		public static void SelectCulture(string culture)
		{
			// List all our resources      
			List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
			foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
			{
				dictionaryList.Add(dictionary);
			}
			// We want our specific culture      
			string requestedCulture = string.Format("Content/Language/StringResources.{0}.xaml", culture);
			ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == requestedCulture);
			if (resourceDictionary == null)
			{
				// If not found, we select our default language              
				requestedCulture = "Content/Language/StringResources.xaml";
				resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == requestedCulture);
			}

			// If we have the requested resource, remove it from the list and place at the end.
			// Then this language will be our string table to use.      
			if (resourceDictionary != null)
			{
				Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
				Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
			}
			// Inform the threads of the new culture      
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
		}

		// Считываем из файла Language.txt настройку языка
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			StreamReader langfile = new StreamReader("Language.txt");
			SelectCulture(langfile.ReadLine());
			langfile.Close();
		}

		public static void SaveSelectedLanguage(string SelectedCulture)
		{
			StreamWriter langfile = new StreamWriter("Language.txt", false);
			langfile.WriteLine(SelectedCulture);
			langfile.Close();
		}
	}
}
