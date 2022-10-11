using System;
using System.IO;
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

			// Язык интерфейса и контроль XPR
			SupportedLanguages al = Localization.CurrentLanguage;
			if (!Localization.IsXPRClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsThisInstanceUnique (al == SupportedLanguages.ru_ru))
				return;

			// Проверка наличия компонентов программы
			if (!File.Exists (RDGenerics.AppStartupPath + ProgramDescription.AssemblyCodecsLibrary))
				{
				if (MessageBox.Show (string.Format (Localization.GetText ("ComponentMissing",
					Localization.CurrentLanguage), ProgramDescription.AssemblyCodecsLibrary),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
					DialogResult.Yes)
					{
					AboutForm af = new AboutForm (null);
					}

				// Не ограничивать работу, если компонент не нужен
				//return;
				}

			// Отображение справки и запроса на принятие Политики
			if (!ProgramDescription.AcceptEULA ())
				return;
			ProgramDescription.ShowAbout (Localization.GetText ("HelpText", Localization.CurrentLanguage), true);

			// Запуск
			Application.Run (new BICForm ());
			}
		}

	/// <summary>
	/// Класс описывает общие методы доступа к библиотеке кодеков
	/// </summary>
	public static class BatchImageConvertorLibrary
		{
		// Внешние функции
		[DllImport (ProgramDescription.AssemblyCodecsLibrary)]
		private static extern IntPtr BIC_GetLibVersion ();

		/// <summary>
		/// Возвращает версию библиотеки кодеков
		/// </summary>
		public static string LibraryVersion
			{
			get
				{
				return Marshal.PtrToStringAnsi (BIC_GetLibVersion ());
				}
			}
		}
	}
