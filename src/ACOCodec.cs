using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для палитр типа Adobe Color Swatches
	/// </summary>
	public class ACOCodec: IPaletteCodec
		{
		[DllImport (BatchImageConvertorLibrary.CodecsLibraryFile)]
		private static extern Int16 ACO_LoadPalette (string FileName, out IntPtr Buffer, out UInt16 ColorsCount);
		// RGB

		[DllImport (BatchImageConvertorLibrary.CodecsLibraryFile)]
		private static extern Int16 ACO_SavePalette (string FileName, byte[] Buffer, UInt16 ColorsCount);
		// RGB

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
			ProgramErrorCodes error = (ProgramErrorCodes)ACO_LoadPalette (FilePath, out buffer, out colorsCount);

			if ((error != ProgramErrorCodes.EXEC_OK) && (error != ProgramErrorCodes.EXEC_UNSUPPORTED_COLORS))
				return error;

			// Извлечение массива данных и сборка изображения
			unsafe
				{
				byte* a = (byte*)buffer.ToPointer ();

				for (int c = 0; c < colorsCount; c++)
					Palette.Add (Color.FromArgb (a[3 * c + 0], a[3 * c + 1], a[3 * c + 2]));
				}

			// Завершено
			return error;
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
			byte[] array = new byte[Palette.Count * 3];

			for (int c = 0; c < Palette.Count; c++)
				{
				array[c * 3 + 0] = Palette[c].R;
				array[c * 3 + 1] = Palette[c].G;
				array[c * 3 + 2] = Palette[c].B;
				}

			// Обращение
			return (ProgramErrorCodes)ACO_SavePalette (FilePath, array, (UInt16)Palette.Count);
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
		private string[] fe = ["*.aco"];

		/// <summary>
		/// Возвращает максимально допустимое количество цветов в палитре
		/// </summary>
		public uint MaxColors
			{
			get
				{
				return 1000;
				}
			}
		}
	}
