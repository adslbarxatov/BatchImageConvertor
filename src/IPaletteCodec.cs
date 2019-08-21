using System.Collections.Generic;
using System.Drawing;

namespace BatchImageConvertor
	{
	/// <summary>
	/// Интерфейс описывает требования к кодекам палитр, используемым в программе
	/// </summary>
	public interface IPaletteCodec
		{
		/// <summary>
		/// Метод загружает указанную палитру и возвращает его в виде объекта List of Color
		/// </summary>
		/// <param name="Palette">Загруженная палитра</param>
		/// <param name="FilePath">Путь к файлу палитры</param>
		/// <returns>Возвращает преобразованное изображение или null в случае ошибки</returns>
		ProgramErrorCodes LoadPalette (string FilePath, out List<Color> Palette);

		/// <summary>
		/// Метод сохраняет указанную палитру
		/// </summary>
		/// <param name="Palette">Сохраняемая палитра</param>
		/// <param name="FilePath">Имя и путь к файлу палитры без расширения</param>
		/// <returns>Возвращает true в случае успеха</returns>
		ProgramErrorCodes SavePalette (string FilePath, List<Color> Palette);

		/// <summary>
		/// Возвращает список соответствующих формату расширений файлов
		/// </summary>
		string[] FileExtensions
			{
			get;
			}

		/// <summary>
		/// Возвращает максимально допустимое количество цветов в палитре
		/// </summary>
		uint MaxColors
			{
			get;
			}
		}
	}
