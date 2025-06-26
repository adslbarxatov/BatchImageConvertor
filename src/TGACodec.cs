using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для изображений типа Truevision Targa
	/// </summary>
	public class TGACodec: ICodec
		{
		[DllImport (ProgramDescription.CodecsLibrary)]
		private static extern Int16 TGA_Load (string FileName, out UInt16 Width, out UInt16 Height, out IntPtr Buffer);

		[DllImport (ProgramDescription.CodecsLibrary)]
		private static extern Int16 TGA_Save (string FileName, UInt16 Width, UInt16 Height, byte[] Buffer);

		[DllImport (ProgramDescription.CodecsLibrary)]
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
			IntPtr buffer;
			ProgramErrorCodes error = (ProgramErrorCodes)TGA_Load (FilePath, out width, out height, out buffer);

			if (error != ProgramErrorCodes.EXEC_OK)
				{
				LoadedImage = null;
				return error;
				}

			// Извлечение массива данных и сборка изображения.
			// При одинаковых PixelFormat эта сволота почему-то не выполняет протяжку с первого шага
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
		/// <param name="Parameters">Неиспользуемый параметр</param>
		/// <param name="ImageColorFormat">Цветовое представление выходного изображения</param>
		/// <param name="BitmapEdge">Порог яркости для чёрно-белого преобразования</param>
		/// <returns>Возвращает true в случае успеха</returns>
		public ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, ASColorMode ImageColorFormat,
			byte BitmapEdge, object Parameters)
			{
			// Контроль (блок параметров не используется)
			if (Image == null)
				return ProgramErrorCodes.EXEC_INVALID_PARAMETERS;

			// Контроль наличия файла (защита от перезаписи)
			string fullPath = TestOutputFile (FilePath, Parameters);
			if (fullPath == "")
				return ProgramErrorCodes.EXEC_FILE_UNAVAILABLE;

			// Подготовка параметров
			BitmapData bmpData = Image.LockBits (new Rectangle (Point.Empty, Image.Size),
				ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			IntPtr ptr = bmpData.Scan0;

			// Копирование данных в массив
			byte[] array = new byte[Image.Width * Image.Height * 4];
			Marshal.Copy (ptr, array, 0, array.Length);

			for (int h = 0; h < Image.Height; h++)
				{
				for (int w = 0; w < Image.Width; w++)
					{
					int idx = 4 * (h * Image.Width + w);
					Color c = Color.FromArgb (array[idx + 3], array[idx + 0], array[idx + 1], array[idx + 2]);
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
					array[idx + 3] = c.A;
					}
				}

			// Обращение
			ProgramErrorCodes res = (ProgramErrorCodes)TGA_Save (fullPath, (UInt16)Image.Width,
				(UInt16)Image.Height, array);

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
				return fe;
				}
			}
		private string[] fe = ["*.tga", "*.vda", "*.icb", "*.vst"];

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
			["TGA, Truevision targa image", null],
			];
		}
	}
