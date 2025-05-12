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
				RDInterface.MessageBox (RDMessageTypes.Question_Center,
					string.Format (RDLocale.GetText ("ComponentMissing"),
					ProgramDescription.CodecsLibrary));
				libUnavailable = true;
				}

			/*else if (BatchImageConvertorLibrary.LibraryVersion != ProgramDescription.LibraryVersion)
				{
				RDInterface.MessageBox (RDMessageTypes.Warning_Center,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.MessageFormat_WrongVersion_Fmt),
					BatchImageConvertorLibrary.CodecsLibraryFile));*/
			else if (!RDGenerics.CheckLibraryVersion (ProgramDescription.AssemblyLibraries[0][0],
				ProgramDescription.AssemblyLibraries[0][1]))
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

	/*/// <summary>
	/// Класс описывает общие методы доступа к библиотеке кодеков
	/// </summary>
	public static class BatchImageConvertorLibrary
		{
		// Внешние функции
		[DllImport (ProgramDescription.CodecsLibrary)]
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

		/// <summary>
		/// Имя библиотеки дополнительных кодеков программы
		/// </summary>
		public const string CodecsLibraryFile = "BatchImageConvertorCodecs.dll";
		}*/
	}
