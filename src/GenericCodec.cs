using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для стандартных изображений, поддерживаемых платформой .NET
	/// </summary>
	public class GenericCodec: ICodec
		{
		/// <summary>
		/// Метод загружает указанное изображение и возвращает его в виде объекта Bitmap
		/// </summary>
		/// <param name="FilePath">Путь к файлу изображения</param>
		/// <param name="LoadedImage">Загруженное изображение</param>
		/// <returns>Возвращает преобразованное изображение или null в случае ошибки</returns>
		public ProgramErrorCodes LoadImage (string FilePath, out Bitmap LoadedImage)
			{
			// Открытие изображения
			try
				{
				Bitmap image = (Bitmap)Image.FromFile (FilePath);
				LoadedImage = (Bitmap)image.Clone ();
				image.Dispose ();
				}
			catch
				{
				LoadedImage = null;
				return ProgramErrorCodes.EXEC_INVALID_FILE;
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
		public ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, ASColorMode ImageColorFormat,
			byte BitmapEdge, object Parameters)
			{
			// Контроль
			if ((Image == null) || (Parameters == null))
				return ProgramErrorCodes.EXEC_INVALID_PARAMETERS;
			ImageFormat imageType = (ImageFormat)Parameters;

			// Контроль наличия файла (защита от перезаписи)
			string fullPath = TestOutputFile (FilePath, Parameters);
			if (fullPath == "")
				return ProgramErrorCodes.EXEC_FILE_UNAVAILABLE;

			// Перегонка
			Bitmap img = new Bitmap (Image);

			// Перегонка цветов
			if (ImageColorFormat != ASColorMode.AllColors)
				{
				for (int h = 0; h < Image.Height; h++)
					{
					for (int w = 0; w < Image.Width; w++)
						{
						if (ImageColorFormat == ASColorMode.Greyscale)
							{
							img.SetPixel (w, h, ColorTransition.ToGreyscale (Image.GetPixel (w, h)));
							}
						if (ImageColorFormat == ASColorMode.Bitmap)
							{
							Color cc = Image.GetPixel (w, h);
							if ((imageType == ImageFormat.Png) || (cc.A == 255))
								{
								img.SetPixel (w, h, ColorTransition.ToBitmap (Image.GetPixel (w, h), BitmapEdge));
								}
							else
								{
								img.SetPixel (w, h, Color.FromArgb (255, 255, 255));    // В этом преобразовании нужно уничтожить альфа-канал
								}
							}
						}
					}
				}

			// Преобразование битности
			if (ImageColorFormat == ASColorMode.Bitmap)
				{
				Bitmap img2 = img.Clone (new Rectangle (0, 0, img.Width, img.Height), PixelFormat.Format1bppIndexed);
				img.Dispose ();
				img = (Bitmap)img2.Clone ();
				img2.Dispose ();
				}

			// Сохранение
			try
				{
				img.Save (fullPath, imageType);
				img.Dispose ();
				}
			catch
				{
				return ProgramErrorCodes.EXEC_FILE_UNAVAILABLE;
				}

			return ProgramErrorCodes.EXEC_OK;
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
			ImageFormat imageType = (ImageFormat)Parameters;

			// Подбор расширения
			string fullPath = FilePath + ".bmp";
			if (imageType == ImageFormat.Gif)
				fullPath = FilePath + ".gif";
			if (imageType == ImageFormat.Jpeg)
				fullPath = FilePath + ".jpeg";
			if (imageType == ImageFormat.Tiff)
				fullPath = FilePath + ".tiff";
			if (imageType == ImageFormat.Png)
				fullPath = FilePath + ".png";

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
				return fe;
				}
			}
		private string[] fe = [
			"*.bmp", "*.dib", "*.rle",		// Bitmaps
			"*.gif",
			"*.png",
			"*.jpe*", "*.jpg", "*.jfif",	// JPEGs
			"*.tif*",						// TIFFs
			];

		/// <summary>
		/// Всегда возвращает true (доступен постоянно)
		/// </summary>
		public bool IsCodecAvailable (bool InternalLibraryUnavailable)
			{
			return true;
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
			[ "PNG, Portable network graphics", ImageFormat.Png],
			[ "JPEG, Joint photographic experts group", ImageFormat.Jpeg],
			[ "BMP, Windows bitmap", ImageFormat.Bmp ],
			[ "GIF, Graphics interchange format", ImageFormat.Gif ],
			[ "TIFF, Tagged image file format", ImageFormat.Tiff ],
			];
		}
	}
