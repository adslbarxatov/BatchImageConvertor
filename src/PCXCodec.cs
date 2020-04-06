using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для изображений типа ZSoft Paintbrush image
	/// </summary>
	public class PCXCodec:ICodec, IPaletteCodec
		{
		[DllImport (ProgramDescription.AssemblyCodecsLibrary)]
		private static extern Int16 PCX_Load (string FileName, out UInt16 Width, out UInt16 Height, out IntPtr Buffer);

		[DllImport (ProgramDescription.AssemblyCodecsLibrary)]
		private static extern Int16 PCX_Save (string FileName, UInt16 Width, UInt16 Height, byte[] Buffer);

		[DllImport (ProgramDescription.AssemblyCodecsLibrary)]
		private static extern Int16 PCX_LoadPalette (string FileName, out IntPtr Buffer, out UInt16 ColorsCount);	// RGB

		[DllImport (ProgramDescription.AssemblyCodecsLibrary)]
		private static extern Int16 PCX_SavePalette (string FileName, byte[] Buffer, UInt16 ColorsCount);			// RGB

		[DllImport (ProgramDescription.AssemblyCodecsLibrary)]
		private static extern void BIC_ReleaseBuffer (IntPtr Buffer);

		/// <summary>
		/// Метод загружает указанное изображение и возвращает его в виде объекта Bitmap
		/// </summary>
		/// <param name="FilePath">Путь к файлу изображения</param>
		/// <param name="LoadedImage">Загруженное изображение</param>
		/// <returns>Возвращает преобразованное изображение или null в случае ошибки</returns>
		public ProgramErrorCodes LoadImage (string FilePath, out Bitmap LoadedImage)
			{
			// Загрузка изображения
			UInt16 width, height;
			IntPtr buffer = IntPtr.Zero;
			ProgramErrorCodes error = (ProgramErrorCodes)PCX_Load (FilePath, out width, out height, out buffer);

			if (error != ProgramErrorCodes.EXEC_OK)
				{
				LoadedImage = null;
				return error;
				}

			// Извлечение массива данных и сборка изображения
			LoadedImage = new Bitmap (width, height, PixelFormat.Format32bppArgb);

			unsafe
				{
				byte* a = (byte*)buffer.ToPointer ();

				for (int h = 0; h < height; h++)
					{
					for (int w = 0; w < width; w++)
						{
						LoadedImage.SetPixel (w, h, Color.FromArgb (a[3 * (h * width + w) + 0],
							a[3 * (h * width + w) + 1], a[3 * (h * width + w) + 2]));
						}
					}
				}

			// Завершено
			BIC_ReleaseBuffer (buffer);			// Давно пора!
			return ProgramErrorCodes.EXEC_OK;
			}

		/// <summary>
		/// Метод сохраняет указанное изображение в требуемом формате
		/// </summary>
		/// <param name="Image">Сохраняемое изображение</param>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Неиспользуемый параметр</param>
		/// <param name="ImageColorFormat">Цветовое представление выходного изображения</param>
		/// <param name="BitmapEdge">Порог яркости для чёрно-белого преобразования</param>
		/// <returns>Возвращает true в случае успеха</returns>
		public ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, OutputImageColorFormat ImageColorFormat, byte BitmapEdge,
			object Parameters)
			{
			// Контроль (блок параметров не используется)
			if (Image == null)
				{
				return ProgramErrorCodes.EXEC_INVALID_PARAMETERS;
				}

			// Контроль наличия файла (защита от перезаписи)
			string fullPath = TestOutputFile (FilePath, Parameters);
			if (fullPath == "")
				{
				return ProgramErrorCodes.EXEC_FILE_UNAVAILABLE;
				}

			// Подготовка параметров
			byte[] array = new byte[Image.Width * Image.Height * 3];

			for (int h = 0; h < Image.Height; h++)
				{
				for (int w = 0; w < Image.Width; w++)
					{
					Color c;
					switch (ImageColorFormat)
						{
						case OutputImageColorFormat.Bitmap:
							c = ColorTransition.ToBitmap (Image.GetPixel (w, h), BitmapEdge);
							break;

						case OutputImageColorFormat.Greyscale:
							c = ColorTransition.ToGreyscale (Image.GetPixel (w, h));
							break;

						case OutputImageColorFormat.Color:
						default:
							c = Image.GetPixel (w, h);
							break;
						}

					array[(h * Image.Width + w) * 3 + 0] = c.R;
					array[(h * Image.Width + w) * 3 + 1] = c.G;
					array[(h * Image.Width + w) * 3 + 2] = c.B;
					}
				}

			// Обращение
			ProgramErrorCodes res = (ProgramErrorCodes)PCX_Save (fullPath, (UInt16)Image.Width, (UInt16)Image.Height, array);

			// Инициирование очистки памяти
			array = null;
			//GC.Collect ();
			return res;
			}

		/// <summary>
		/// Метод определяет, может ли быть создан указанный файл с заданными параметрами сохранения
		/// </summary>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Дополнительные параметры (если требуются)</param>
		/// <returns>Возвращает полное имя файла в случае допустимости записи</returns>
		public string TestOutputFile (string FilePath, object Parameters)
			{
			string fullPath = FilePath + FileExtensions[0].Substring (1);
			if (File.Exists (fullPath))
				{
				return "";
				}
			return fullPath;
			}

		/// <summary>
		/// Возвращает список соответствующих формату расширений файлов
		/// </summary>
		public string[] FileExtensions
			{
			get
				{
				return new string[] { "*.pcx", "*.pcc" };
				}
			}

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
			Palette = new List<Color> ();
			ProgramErrorCodes error = (ProgramErrorCodes)PCX_LoadPalette (FilePath, out buffer, out colorsCount);

			if (error != ProgramErrorCodes.EXEC_OK)
				{
				return error;
				}

			// Извлечение массива данных и сборка изображения
			unsafe
				{
				byte* a = (byte*)buffer.ToPointer ();

				for (int c = 0; c < colorsCount; c++)
					{
					Palette.Add (Color.FromArgb (a[3 * c + 0], a[3 * c + 1], a[3 * c + 2]));
					}
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
				{
				return ProgramErrorCodes.EXEC_INVALID_PARAMETERS;
				}

			// Подготовка параметров
			byte[] array = new byte[Palette.Count * 3];

			for (int c = 0; c < Palette.Count; c++)
				{
				array[c * 3 + 0] = Palette[c].R;
				array[c * 3 + 1] = Palette[c].G;
				array[c * 3 + 2] = Palette[c].B;
				}

			// Обращение
			return (ProgramErrorCodes)PCX_SavePalette (FilePath, array, (UInt16)Palette.Count);
			}

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
