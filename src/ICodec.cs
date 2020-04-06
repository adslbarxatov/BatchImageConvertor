using System;
using System.Drawing;

namespace RD_AAOW
	{
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
		ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, OutputImageColorFormat ImageColorFormat, byte BitmapEdge,
			object Parameters);

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
		}

	/// <summary>
	/// Возможные варианты цветового представления выходных изображений
	/// </summary>
	public enum OutputImageColorFormat
		{
		/// <summary>
		/// Цветное (без изменений)
		/// </summary>
		Color,

		/// <summary>
		/// Градации серого
		/// </summary>
		Greyscale,

		/// <summary>
		/// Чёрно-белое (бихром)
		/// </summary>
		Bitmap
		}

	/// <summary>
	/// Класс описывает методы преобразования цветовых представлений
	/// </summary>
	public static class ColorTransition
		{
		// Метод возвращает оттенок серого для представленного цвета
		private static byte GetGreyscaleValue (Color OldColor)
			{
			/*return (byte)Math.Ceiling((double)OldColor.R * 0.299 + (double)OldColor.G * 0.587 + 
				(double)OldColor.B * 0.114);	// PAL/NTSC*/
			return (byte)Math.Ceiling ((double)OldColor.R * 0.2126 + (double)OldColor.G * 0.7152 +
				(double)OldColor.B * 0.0722);	// HDTV
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
				{
				return Color.FromArgb (OldColor.A, 0, 0, 0);
				}
			return Color.FromArgb (OldColor.A, 255, 255, 255);
			}
		}
	}
