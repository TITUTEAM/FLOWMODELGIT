using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace FLOWMODEL
{
	public static class Commands
	{
		// -----------------------------------------------------------------------------------------------
		// XAML CUSTOM COMMANDS DECLARATION
		// -----------------------------------------------------------------------------------------------
		// FILE MENU /////////////////////////////////////////////////////////////////////////////////////
		// Save a report from application
		public static readonly RoutedUICommand SaveReportReadonly = new RoutedUICommand
			(
				"Сохранить в отчет",
				"SaveReport",
				typeof(Commands),
				new InputGestureCollection()
				{
					new KeyGesture(Key.S, ModifierKeys.Control)
				}
			);
		public static RoutedUICommand SaveReportCommand { get { return SaveReportReadonly; } }

		// Exit from the application
		public static readonly RoutedUICommand AppExitReadonly = new RoutedUICommand
			(
				"Выход",
				"AppExit",
				typeof(Commands),
				new InputGestureCollection()
				{
					new KeyGesture(Key.Q, ModifierKeys.Control)
				}
			);
		public static RoutedUICommand AppExitCommand { get { return AppExitReadonly; } }

		// TOOLS MENU ////////////////////////////////////////////////////////////////////////////////////
		// Change user
		public static readonly RoutedUICommand ChangeUserReadonly = new RoutedUICommand
			(
				"Смена пользователя",
				"ChangeUser",
				typeof(Commands),
				null
			);
		public static RoutedUICommand ChangeUserCommand { get { return ChangeUserReadonly; } }
	}
}
