﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для изображений типа Portable bitmap
	/// </summary>
	public class PBMCodec: ICodec
		{
		[DllImport (ProgramDescription.CodecsLibrary)]
		private static extern Int16 PBM_Load (string FileName, out UInt16 Width, out UInt16 Height,
			out IntPtr Buffer);

		[DllImport (ProgramDescription.CodecsLibrary)]
		private static extern Int16 PBM_Save (string FileName, UInt16 Width, UInt16 Height, byte[] Buffer,
			byte ImageType);

		[DllImport (ProgramDescription.CodecsLibrary)]
		private static extern void BIC_ReleaseBuffer (IntPtr Buffer);

		/// <summary>
		/// Возможные типы изображений
		/// </summary>
		public enum ImageTypes
			{
			/// <summary>
			/// Чёрно-белое в текстовом представлении
			/// </summary>
			BitmapAsText = 1,

			/// <summary>
			/// Оттенки серого в текстовом представлении
			/// </summary>
			GreyscaleAsText = 2,

			/// <summary>
			/// Цветное в текстовом представлении
			/// </summary>
			ColorAsText = 3,

			/// <summary>
			/// Чёрно-белое в бинарном представлении
			/// </summary>
			BitmapAsBinary = 4,

			/// <summary>
			/// Оттенки серого в бинарном представлении
			/// </summary>
			GreyscaleAsBinary = 5,

			/// <summary>
			/// Цветное в бинарном представлении
			/// </summary>
			ColorAsBinary = 6
			}

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
			IntPtr buffer;
			ProgramErrorCodes error = (ProgramErrorCodes)PBM_Load (FilePath, out width, out height, out buffer);

			if (error != ProgramErrorCodes.EXEC_OK)
				{
				LoadedImage = null;
				return error;
				}

			// Извлечение массива данных и сборка изображения
			/*LoadedImage = new Bitmap (width, height, PixelFormat.Format32bppArgb);

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
				}*/
			Bitmap tmp = new Bitmap (width, height, 4 * width, PixelFormat.Format32bppArgb, buffer);
			Bitmap tmp2 = tmp.Clone (new Rectangle (Point.Empty, tmp.Size), PixelFormat.Format64bppArgb);   // Протяжка
			LoadedImage = tmp2.Clone (new Rectangle (Point.Empty, tmp.Size), PixelFormat.Format32bppArgb);
			tmp.Dispose ();
			tmp2.Dispose ();
			BIC_ReleaseBuffer (buffer);

			// Завершено
			return ProgramErrorCodes.EXEC_OK;
			}

		/// <summary>
		/// Метод сохраняет указанное изображение в требуемом формате
		/// </summary>
		/// <param name="Image">Сохраняемое изображение</param>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Тип PBM, в котором требуется сохранить изображение</param>
		/// <param name="ImageColorFormat">Цветовое представление выходного изображения</param>
		/// <param name="BitmapEdge">Порог яркости для чёрно-белого преобразования</param>
		/// <returns>Возвращает true в случае успеха</returns>
		public ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, ASColorMode ImageColorFormat,
			byte BitmapEdge, object Parameters)
			{
			// Контроль
			if ((Image == null) || (Parameters == null))
				return ProgramErrorCodes.EXEC_INVALID_PARAMETERS;
			ImageTypes imageType = (ImageTypes)Parameters;

			// Контроль наличия файла (защита от перезаписи)
			string fullPath = TestOutputFile (FilePath, Parameters);
			if (fullPath == "")
				return ProgramErrorCodes.EXEC_FILE_UNAVAILABLE;

			// Подготовка параметров
			BitmapData bmpData = Image.LockBits (new Rectangle (Point.Empty, Image.Size),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			IntPtr ptr = bmpData.Scan0;

			// Копирование данных в массив
			byte[] array = new byte[Image.Width * Image.Height * 3];
			Marshal.Copy (ptr, array, 0, array.Length);

			for (int h = 0; h < Image.Height; h++)
				{
				for (int w = 0; w < Image.Width; w++)
					{
					int idx = 3 * (h * Image.Width + w);
					Color c = Color.FromArgb (array[idx + 0], array[idx + 1], array[idx + 2]);
					switch (ImageColorFormat)
						{
						case ASColorMode.Bitmap:
							c = ColorTransition.ToBitmap (c, BitmapEdge);
							break;

						case ASColorMode.Greyscale:
							c = ColorTransition.ToGreyscale (c);
							break;

						case ASColorMode.AllColors:
						default:
							break;
						}

					array[idx + 0] = c.R;
					array[idx + 1] = c.G;
					array[idx + 2] = c.B;
					}
				}

			/*// Подготовка параметров
			byte[] array = new byte[Image.Width * Image.Height * 3];

			for (int h = 0; h < Image.Height; h++)
				{
				for (int w = 0; w < Image.Width; w++)
					{
					Color c;
					switch (ImageColorFormat)
						{
						case ASColorMode.Bitmap:
							c = ColorTransition.ToBitmap (Image.GetPixel (w, h), BitmapEdge);
							break;

						case ASColorMode.Greyscale:
							c = ColorTransition.ToGreyscale (Image.GetPixel (w, h));
							break;

						case ASColorMode.AllColors:
						default:
							c = Image.GetPixel (w, h);
							break;
						}

					array[(h * Image.Width + w) * 3 + 0] = c.R;
					array[(h * Image.Width + w) * 3 + 1] = c.G;
					array[(h * Image.Width + w) * 3 + 2] = c.B;
					}
				}*/

			// Обращение
			ProgramErrorCodes res = (ProgramErrorCodes)PBM_Save (fullPath, (UInt16)Image.Width,
				(UInt16)Image.Height, array, (byte)imageType);

			// Инициирование очистки памяти
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
			// Контроль параметров
			if (Parameters == null)
				return "";
			ImageTypes imageType = (ImageTypes)Parameters;

			// Подбор расширения
			string fullPath = FilePath + ".pbm";
			if ((imageType == ImageTypes.GreyscaleAsBinary) || (imageType == ImageTypes.GreyscaleAsText))
				fullPath = FilePath + ".pgm";
			if ((imageType == ImageTypes.ColorAsBinary) || (imageType == ImageTypes.ColorAsText))
				fullPath = FilePath + ".ppm";

			// Сборка
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
				return fe;
				}
			}
		private string[] fe = ["*.pnm", "*.pbm", "*.pgm", "*.ppm"];
		// Насчёт .pfm и .pam информация есть. Они будут описаны позднее... наверное...

		/// <summary>
		/// Возвращает true, если кодек может функционировать в текущей конфигруации приложения
		/// </summary>
		public bool IsCodecAvailable (bool InternalLibraryUnavailable)
			{
			return !InternalLibraryUnavailable &&
				File.Exists (RDGenerics.AppStartupPath + ProgramDescription.CodecsLibrary);
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
				return oms;
				}
			}
		private object[][] oms = [
			[ "PBM, Portable bitmap format " + ColorTransition.RGBMarker, ImageTypes.ColorAsBinary ],
			[ "PBM, Portable bitmap format " + ColorTransition.OnlyGreyscaleMarker, ImageTypes.GreyscaleAsBinary ],
			[ "PBM, Portable bitmap format " + ColorTransition.OnlyBnWMarker, ImageTypes.BitmapAsBinary ],
			];
		}
	}
