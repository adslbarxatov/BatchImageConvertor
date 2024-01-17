using System.Drawing;
using System.Drawing.Imaging;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для метафайлов Windows
	/// </summary>
	public class MetafileCodec: ICodec
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
				Image m = Metafile.FromFile (FilePath);
				LoadedImage = new Bitmap (m);
				m.Dispose ();
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
		/// Метод сохраняет указанное изображение в требуемом формате (в данном формате не используется)
		/// </summary>
		/// <param name="Image">Сохраняемое изображение</param>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Неиспользуемый параметр</param>
		/// <param name="ImageColorFormat">Цветовое представление выходного изображения</param>
		/// <param name="BitmapEdge">Порог яркости для чёрно-белого преобразования</param>
		/// <returns>Возвращает true в случае успеха</returns>
		public ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, OutputImageColorFormat ImageColorFormat,
			byte BitmapEdge, object Parameters)
			{
			// Не поддерживается в данной реализации
			return ProgramErrorCodes.EXEC_NOT_IMPLEMENTED;
			}

		/// <summary>
		/// Метод определяет, может ли быть создан указанный файл с заданными параметрами сохранения
		/// </summary>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Дополнительные параметры (если требуются)</param>
		/// <returns>Возвращает полное имя файла в случае допустимости записи</returns>
		public string TestOutputFile (string FilePath, object Parameters)
			{
			// Не поддерживается в данной реализации
			return "";
			}

		/// <summary>
		/// Возвращает список соответствующих формату расширений файлов
		/// </summary>
		public string[] FileExtensions
			{
			get
				{
				return new string[] { "*.wmf", "*.emf" };
				}
			}

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
				return new object[][] { };
				}
			}
		}
	}
