using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для изображений типа JPEG2000 image
	/// </summary>
	public class JP2Codec: ICodec
		{
		[DllImport (ProgramDescription.AssemblyCodecsLibrary)]
		private static extern Int16 JP2_Load (string FileName, out UInt16 Width, out UInt16 Height, out IntPtr Buffer);

		[DllImport (ProgramDescription.AssemblyCodecsLibrary)]
		private static extern Int16 JP2_Save (string FileName, UInt16 Width, UInt16 Height, byte[] Buffer,
			byte CodecType);

		[DllImport (ProgramDescription.AssemblyCodecsLibrary)]
		private static extern void BIC_ReleaseBuffer (IntPtr Buffer);

		/// <summary>
		/// Возможные типы изображений
		/// </summary>
		public enum ImageTypes
			{
			/// <summary>
			/// JP2
			/// </summary>
			JP2 = 1,

			/// <summary>
			/// J2K
			/// </summary>
			J2K = 2
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
			ProgramErrorCodes error = (ProgramErrorCodes)JP2_Load (FilePath, out width, out height, out buffer);

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
						LoadedImage.SetPixel (w, h, Color.FromArgb (a[4 * (h * width + w) + 3], a[4 * (h * width + w) + 0],
							a[4 * (h * width + w) + 1], a[4 * (h * width + w) + 2]));
						}
					}
				}

			// Завершено
			BIC_ReleaseBuffer (buffer);
			return ProgramErrorCodes.EXEC_OK;
			}

		/// <summary>
		/// Метод сохраняет указанное изображение в требуемом формате
		/// </summary>
		/// <param name="Image">Сохраняемое изображение</param>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Дополнительные параметры (если требуются)</param>
		/// <param name="ImageColorFormat">Цветовое представление выходного изображения</param>
		/// <param name="BitmapEdge">Порог яркости для чёрно-белого преобразования</param>
		/// <returns>Возвращает true в случае успеха</returns>
		public ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, OutputImageColorFormat ImageColorFormat,
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
			byte[] array = new byte[Image.Width * Image.Height * 4];

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

					array[(h * Image.Width + w) * 4 + 0] = c.R;
					array[(h * Image.Width + w) * 4 + 1] = c.G;
					array[(h * Image.Width + w) * 4 + 2] = c.B;
					array[(h * Image.Width + w) * 4 + 3] = c.A;
					}
				}

			// Обращение
			ProgramErrorCodes res = (ProgramErrorCodes)JP2_Save (fullPath, (UInt16)Image.Width,
				(UInt16)Image.Height, array, (byte)imageType);

			// Инициирование очистки памяти
			array = null;
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
			// Контроль
			if (Parameters == null)
				return "";
			ImageTypes imageType = (ImageTypes)Parameters;

			// Подбор расширения
			string fullPath = FilePath + ".jp2";
			if (imageType == ImageTypes.J2K)
				fullPath = FilePath + ".j2k";

			// Контроль наличия файла (защита от перезаписи)
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
				return new string[] {
					"*.jp2",											// JP2 subformat
					"*.j2c", "*.j2k", "*.jpc", "*.jpf", "*.jpx"			// J2K subformat
					};
				}
			}
		}
	}
