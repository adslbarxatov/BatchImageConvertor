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

			// Язык интерфейса и контроль XPUN
			if (!Localization.IsXPUNClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsAppInstanceUnique (true))
				return;

			// Проверка наличия компонентов программы
			if (!File.Exists (RDGenerics.AppStartupPath + ProgramDescription.AssemblyCodecsLibrary))
				{
				if (RDGenerics.MessageBox (RDMessageTypes.Question,
					string.Format (Localization.GetText ("ComponentMissing"),
					ProgramDescription.AssemblyCodecsLibrary),
					Localization.GetDefaultText (LzDefaultTextValues.Button_Yes),
					Localization.GetDefaultText (LzDefaultTextValues.Button_No)) ==
					RDMessageButtons.ButtonOne)
					{
					AboutForm af = new AboutForm (null);
					}

				// Не ограничивать работу, если компонент не нужен
				//return;
				}

			// Отображение справки и запроса на принятие Политики
			if (!RDGenerics.AcceptEULA ())
				return;
			RDGenerics.ShowAbout (true);

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
