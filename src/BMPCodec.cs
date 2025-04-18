using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для палитр в составе рисунков BMP
	/// </summary>
	public class BMPCodec: IPaletteCodec
		{
		[DllImport (BatchImageConvertorLibrary.CodecsLibraryFile)]
		private static extern Int16 BMP_LoadPalette (string FileName, out IntPtr Buffer, out UInt16 ColorsCount);
		// RGBA

		[DllImport (BatchImageConvertorLibrary.CodecsLibraryFile)]
		private static extern Int16 BMP_SetPalette (string FileName, byte[] Buffer, UInt16 ColorsCount);
		// RGBA

		/// <summary>
		/// Метод загружает указанную палитру и возвращает его в виде объекта List of Color
		/// </summary>
		/// <param name="Palette">Загруженная палитра</param>
		/// <param name="FilePath">Путь к файлу палитры</param>
		/// <returns>Возвращает преобразованное изображение или null в случае ошибки</returns>
		public ProgramErrorCodes LoadPalette (string FilePath, out List<Color> Palette)
			{
			// Загрузка изображения
			IntPtr buffer;
			UInt16 colorsCount;
			Palette = [];
			ProgramErrorCodes error = (ProgramErrorCodes)BMP_LoadPalette (FilePath, out buffer, out colorsCount);

			if (error != ProgramErrorCodes.EXEC_OK)
				return error;

			// Извлечение массива данных и сборка изображения
			unsafe
				{
				byte* a = (byte*)buffer.ToPointer ();

				for (int c = 0; c < colorsCount; c++)
					Palette.Add (Color.FromArgb (a[4 * c + 3], a[4 * c + 0], a[4 * c + 1], a[4 * c + 2]));
				}

			// Завершено
			return ProgramErrorCodes.EXEC_OK;
			}

		/// <summary>
		/// Метод сохраняет указанную палитру
		/// </summary>
		/// <param name="Palette">Сохраняемая палитра</param>
		/// <param name="FilePath">Имя и путь к файлу палитры без расширения</param>
		/// <returns>Возвращает true в случае успеха</returns>
		public ProgramErrorCodes SavePalette (string FilePath, List<Color> Palette)
			{
			// Контроль (блок параметров не используется)
			if ((Palette == null) || (Palette.Count == 0) || (Palette.Count > MaxColors))
				return ProgramErrorCodes.EXEC_INVALID_PARAMETERS;

			// Подготовка параметров
			byte[] array = new byte[Palette.Count * 4];

			for (int c = 0; c < Palette.Count; c++)
				{
				array[c * 4 + 0] = Palette[c].R;
				array[c * 4 + 1] = Palette[c].G;
				array[c * 4 + 2] = Palette[c].B;
				array[c * 4 + 3] = Palette[c].A;
				}

			// Обращение
			return (ProgramErrorCodes)BMP_SetPalette (FilePath, array, (UInt16)Palette.Count);
			}

		/// <summary>
		/// Возвращает список соответствующих формату расширений файлов
		/// </summary>
		public string[] FileExtensions
			{
			get
				{
				return fe;
				}
			}
		private string[] fe = ["*.bmp"];

		/// <summary>
		/// Возвращает максимально допустимое количество цветов в палитре
		/// </summary>
		public uint MaxColors
			{
			get
				{
				return 256;
				}
			}
		}
	}
