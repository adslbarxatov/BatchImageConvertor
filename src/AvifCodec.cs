using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для изображений формата Avif
	/// </summary>
	public class AvifCodec: ICodec
		{
		// Константы
		private const string codecApp = "davif.exe";

		// Метод получает путь к файлу в формате 8.3
		private static string GetShortName (string LongFileName)
			{
			StringBuilder buffer = new StringBuilder (259);
			int len = GetShortPathName (LongFileName, buffer, buffer.Capacity);

			if (len == 0)
				return LongFileName;
			//throw new System.ComponentModel.Win32Exception ();

			return buffer.ToString ();
			}

		[DllImport ("kernel32")]
		private static extern int GetShortPathName (string longPath, StringBuilder shortPath, int bufSize);

		/// <summary>
		/// Метод загружает указанное изображение и возвращает его в виде объекта Bitmap
		/// </summary>
		/// <param name="FilePath">Путь к файлу изображения</param>
		/// <param name="LoadedImage">Загруженное изображение</param>
		/// <returns>Возвращает преобразованное изображение или null в случае ошибки</returns>
		public ProgramErrorCodes LoadImage (string FilePath, out Bitmap LoadedImage)
			{
			// Преобразование изображения в промежуточный формат
			try
				{
				Process p = new Process ();
				p.StartInfo = new ProcessStartInfo (RDGenerics.AppStartupPath + codecApp,
					"-i " + GetShortName (FilePath) +
					" -o \"" + FilePath + ".png\"");
				p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

				p.Start ();
				p.WaitForExit ();
				}
			catch { }

			// Открытие изображения
			try
				{
				FileStream fs = new FileStream (FilePath + ".png", FileMode.Open);
				LoadedImage = (Bitmap)Image.FromStream (fs);

				fs.Close ();
				File.Delete (FilePath + ".png");
				}
			catch
				{
				LoadedImage = null;
				return ProgramErrorCodes.EXEC_UNSUPPORTED_OS;
				}

			if ((LoadedImage.Width > ProgramDescription.MaxLinearSize) ||
				(LoadedImage.Height > ProgramDescription.MaxLinearSize) ||
				(LoadedImage.Width * LoadedImage.Height == 0))
				{
				return ProgramErrorCodes.EXEC_INVALID_FILE;
				}

			return ProgramErrorCodes.EXEC_OK;
			}

		/// <summary>
		/// Метод сохраняет указанное изображение в требуемом формате
		/// </summary>
		/// <param name="Image">Сохраняемое изображение</param>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Тип, в котором требуется сохранить изображение</param>
		/// <param name="ImageColorFormat">Цветовое представление выходного изображения</param>
		/// <param name="BitmapEdge">Порог яркости для чёрно-белого преобразования</param>
		/// <returns>Возвращает true в случае успеха</returns>
		public ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, OutputImageColorFormat ImageColorFormat,
			byte BitmapEdge, object Parameters)
			{
			return ProgramErrorCodes.EXEC_NOT_IMPLEMENTED;
			}

		/// <summary>
		/// Метод определяет, может ли быть создан указанный файл с заданными параметрами сохранения
		/// </summary>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Дополнительные параметры (если требуются)</param>
		/// <returns>Возвращает полное имя файла в случае допустимости записи</returns>
		public string TestOutputFile (string FilePath, object Parameters)
			{
			// Контроль наличия файла (защита от перезаписи)
			string fullPath = FilePath + FileExtensions[0].Substring (1);
			if (File.Exists (fullPath))
				return "";

			return fullPath;
			}

		/// <summary>
		/// Возвращает список соответствующих формату расширений файлов
		/// </summary>
		public string[] FileExtensions
			{
			get
				{
				return new string[] { "*.avif" };
				}
			}

		/// <summary>
		/// Возвращает true, если кодек может функционировать в текущей конфигруации приложения
		/// </summary>
		public bool IsCodecAvailable (bool InternalLibraryUnavailable)
			{
			return File.Exists (RDGenerics.AppStartupPath + codecApp);
			}

		/// <summary>
		/// Возвращает параметры работы кодека в режиме сохранения:
		/// - элемент [n][0] = название создаваемого формата
		/// - элемент [n][1] = внутренний параметр кодека, соответствующий формату
		/// </summary>
		public object[][] OutputModeSettings
			{
			get
				{
				return new object[][] { };
				}
			}
		}
	}
