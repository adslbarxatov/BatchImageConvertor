using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает кодек для иконок Windows
	/// </summary>
	public class ICOCodec: ICodec
		{
		[DllImport (ProgramDescription.CodecsLibrary)]
		private static extern Int16 ICO_Load (string FileName, out UInt16 WidthHeight,
			out IntPtr Buffer, out UInt32 Length);

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
			UInt16 size;
			IntPtr buffer;
			UInt32 length;
			ProgramErrorCodes error = (ProgramErrorCodes)ICO_Load (FilePath, out size, out buffer, out length);

			if (error != ProgramErrorCodes.EXEC_OK)
				{
				LoadedImage = null;
				return error;
				}

			// PNG
			if (length != 0)
				{
				byte[] array = new byte[length];
				Marshal.Copy (buffer, array, 0, (int)length);

				try
					{
					MemoryStream ms = new MemoryStream (array);
					LoadedImage = new Bitmap (ms);
					ms.Close ();
					}
				catch
					{
					LoadedImage = null;
					return ProgramErrorCodes.EXEC_INVALID_FILE;
					}
				}

			// ICO
			else
				{
				Bitmap tmp = new Bitmap (size, size, 4 * size, PixelFormat.Format32bppArgb, buffer);
				Bitmap tmp2 = tmp.Clone (new Rectangle (Point.Empty, tmp.Size), PixelFormat.Format64bppArgb);   // Протяжка
				LoadedImage = tmp2.Clone (new Rectangle (Point.Empty, tmp.Size), PixelFormat.Format32bppArgb);
				tmp.Dispose ();
				tmp2.Dispose ();
				}

			BIC_ReleaseBuffer (buffer);

			// Завершено
			return ProgramErrorCodes.EXEC_OK;
			}

		/// <summary>
		/// Метод сохраняет указанное изображение в требуемом формате (заглушка, не используется)
		/// </summary>
		/// <param name="Image">Сохраняемое изображение</param>
		/// <param name="FilePath">Имя и путь к файлу изображения без расширения</param>
		/// <param name="Parameters">Дополнительные параметры (если требуются)</param>
		/// <param name="ImageColorFormat">Цветовое представление выходного изображения</param>
		/// <param name="BitmapEdge">Порог яркости для чёрно-белого преобразования</param>
		/// <returns>Возвращает true в случае успеха</returns>
		public ProgramErrorCodes SaveImage (Bitmap Image, string FilePath, ASColorMode ImageColorFormat,
			byte BitmapEdge, object Parameters)
			{
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
		private string[] fe = ["*.ico"];

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
		private object[][] oms = [];
		}
	}
