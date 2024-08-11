using System;
using System.Drawing;

namespace RD_AAOW
	{
	/// <summary>
	/// Список возможных ошибок программы
	/// </summary>
	public enum ProgramErrorCodes
		{
		/// <summary>
		/// Ошибок нет
		/// </summary>
		EXEC_OK = 0,

		/// <summary>
		/// Некорректные параметры команды (говорит об ошибке в программе)
		/// </summary>
		EXEC_INVALID_PARAMETERS = -1,

		/// <summary>
		/// Файл не найден или недоступен
		/// </summary>
		EXEC_FILE_UNAVAILABLE = -2,

		/// <summary>
		/// Файл повреждён или не поддерживается
		/// </summary>
		EXEC_INVALID_FILE = -3,

		/// <summary>
		/// Ошибка работы с памятью (возможно, требуется повтор операции)
		/// </summary>
		EXEC_MEMORY_ALLOC_FAIL = -4,

		/// <summary>
		/// Файл не содержит палитры
		/// </summary>
		EXEC_NO_PALETTE_AVAILABLE = -11,

		/// <summary>
		/// Файл не содержит палитры
		/// </summary>
		EXEC_UNSUPPORTED_COLORS = -12,

		/// <summary>
		/// Операционная система не поддерживает данный тип изображений
		/// </summary>
		EXEC_UNSUPPORTED_OS = -13,

		/// <summary>
		/// Метод на стадии разработки
		/// </summary>
		EXEC_NOT_IMPLEMENTED = -100
		}

	/// <summary>
	/// Интерфейс описывает требования к кодекам, используемым в программе
	/// </summary>
	public interface ICodec
		{
		/// <summary>
		/// Метод загружает указанное изображение и возвращает его в виде объекта Bitmap
		/// </summary>
		/// <param name="FilePath">Путь к файлу изображения</param>
		/// <param name="LoadedImage">Загруженное изображение</param>
		/// <returns>Возвращает преобразованное изображение или null в случае ошибки</returns>
		ProgramErrorCodes LoadImage (string FilePath, out Bitmap LoadedImage);

		/// <summary>
		/// Метод сохраняет указанное изображение в требуемом формате
		/// </summary>
		/// <param name="Image">Сохраняемое изображение</param>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Дополнительные параметры (если требуются)</param>
		/// <param name="ImageColorFormat">Цветовое представление выходного изображения</param>
		/// <param name="BitmapEdge">Порог яркости для чёрно-белого преобразования</param>
		/// <returns>Возвращает true в случае успеха</returns>
		ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, ASColorMode ImageColorFormat,
			byte BitmapEdge, object Parameters);

		/// <summary>
		/// Метод определяет, может ли быть создан указанный файл с заданными параметрами сохранения
		/// </summary>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Дополнительные параметры (если требуются)</param>
		/// <returns>Возвращает полное имя файла в случае допустимости записи</returns>
		string TestOutputFile (string FilePath, object Parameters);

		/// <summary>
		/// Возвращает список соответствующих формату расширений файлов
		/// </summary>
		string[] FileExtensions
			{
			get;
			}

		/// <summary>
		/// Возвращает true, если кодек может функционировать в текущей конфигруации приложения
		/// </summary>
		bool IsCodecAvailable (bool InternalLibraryUnavailable);

		/// <summary>
		/// Возвращает параметры работы кодека в режиме сохранения:
		/// - элемент [n][0] = название создаваемого формата
		/// - элемент [n][1] = внутренний параметр кодека, соответствующий формату
		/// </summary>
		object[][] OutputModeSettings
			{
			get;
			}
		}

	/// <summary>
	/// Класс описывает методы преобразования цветовых представлений
	/// </summary>
	public static class ColorTransition
		{
		// Метод возвращает оттенок серого для представленного цвета
		private static byte GetGreyscaleValue (Color OldColor)
			{
#if PAL_NTSC
			return (byte)Math.Ceiling((double)OldColor.R * 0.299 + (double)OldColor.G * 0.587 + 
				(double)OldColor.B * 0.114);	// PAL/NTSC
#else
			return (byte)Math.Ceiling ((double)OldColor.R * 0.2126 + (double)OldColor.G * 0.7152 +
				(double)OldColor.B * 0.0722);   // HDTV
#endif
			}

		/// <summary>
		/// Метод преобразует указанный цвет в ближайший цвет из шкалы серых оттенков
		/// </summary>
		/// <param name="OldColor">Цвет для преобразования</param>
		/// <returns>Конечный цвет</returns>
		public static Color ToGreyscale (Color OldColor)
			{
			byte rgbValue = GetGreyscaleValue (OldColor);
			return Color.FromArgb (OldColor.A, rgbValue, rgbValue, rgbValue);
			}

		/// <summary>
		/// Метод преобразует указанный цвет в чёрный или белый в зависимости указанного порога яркости
		/// </summary>
		/// <param name="OldColor">Цвет для преобразования</param>
		/// <param name="Edge">Порог яркости цвета</param>
		/// <returns>Конечный цвет</returns>
		public static Color ToBitmap (Color OldColor, byte Edge)
			{
			byte rgbValue = GetGreyscaleValue (OldColor);
			if (rgbValue < Edge)
				return Color.FromArgb (OldColor.A, 0, 0, 0);

			return Color.FromArgb (OldColor.A, 255, 255, 255);
			}

		/// <summary>
		/// Маркер режима «только чёрный и белый»
		/// </summary>
		public const string OnlyBnWMarker = "[black & white]";

		/// <summary>
		/// Маркер режима «градации серого»
		/// </summary>
		public const string OnlyGreyscaleMarker = "[greyscale]";

		/// <summary>
		/// Маркер режима «все цвета»
		/// </summary>
		public const string RGBMarker = "[rgb]";
		}
	}
