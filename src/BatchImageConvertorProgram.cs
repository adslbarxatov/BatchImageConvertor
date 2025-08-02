using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает основную программу
	/// </summary>
	public static class BatchImageConvertorProgram
		{
		/// <summary>
		/// Конструктор. Описывает точку входа приложения
		/// </summary>
		[STAThread]
		public static void Main ()
			{
			// Инициализация
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			RDLocale.InitEncodings ();

			// Язык интерфейса и контроль XPUN
			if (!RDLocale.IsXPUNClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsAppInstanceUnique (true))
				return;

			// Проверка наличия компонентов программы
			bool libUnavailable = false;
			if (!RDGenerics.CheckLibrariesExistence (ProgramDescription.AssemblyLibraries, false))
				{
				RDInterface.MessageBox (RDMessageFlags.Question | RDMessageFlags.CenterText,
					string.Format (RDLocale.GetText ("ComponentMissing"),
					ProgramDescription.CodecsLibrary));
				libUnavailable = true;
				}

			else if (!RDGenerics.CheckLibrariesVersions (ProgramDescription.AssemblyLibraries, true))
				{
				libUnavailable = true;
				}

			// Отображение справки и запроса на принятие Политики
			if (!RDInterface.AcceptEULA ())
				return;
			if (!RDInterface.ShowAbout (true))
				RDGenerics.RegisterFileAssociations (true);

			// Запуск
			Application.Run (new BICForm (libUnavailable));
			}
		}
	}
