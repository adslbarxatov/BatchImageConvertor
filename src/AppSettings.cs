using System;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс реализует хранилище настроек программы
	/// </summary>
	public static class AppSettings
		{
		/// <summary>
		/// Возвращает или задаёт входную директорию
		/// </summary>
		public static string InputPath
			{
			get
				{
				return RDGenerics.GetSettings (inputPathPar,
					Environment.GetFolderPath (Environment.SpecialFolder.Desktop) + "\\");
				}
			set
				{
				RDGenerics.SetSettings (inputPathPar, value);
				}
			}
		private const string inputPathPar = "InputPath";

		/// <summary>
		/// Возвращает или задаёт выходную директорию
		/// </summary>
		public static string OutputPath
			{
			get
				{
				return RDGenerics.GetSettings (outputPathPar,
					Environment.GetFolderPath (Environment.SpecialFolder.Desktop) + "\\");
				}
			set
				{
				RDGenerics.SetSettings (outputPathPar, value);
				}
			}
		private const string outputPathPar = "OutputPath";

		/// <summary>
		/// Возвращает или задаёт флаг включения подпапок в поиск
		/// </summary>
		public static bool IncludeSubdirs
			{
			get
				{
				return RDGenerics.GetSettings (includeSubdirsPar, false);
				}
			set
				{
				RDGenerics.SetSettings (includeSubdirsPar, value);
				}
			}
		private const string includeSubdirsPar = "IncludeSubdirs";

		/// <summary>
		/// Возвращает или задаёт режим изменения размеров изображения
		/// </summary>
		public static ASResizingMode ResizingMode
			{
			get
				{
				return (ASResizingMode)RDGenerics.GetSettings (resizingModePar, (uint)ASResizingMode.RelativeSize);
				}
			set
				{
				RDGenerics.SetSettings (resizingModePar, (uint)value);
				}
			}
		private const string resizingModePar = "ResizingMode";

		/// <summary>
		/// Возвращает или задаёт режим изменения цветности изображения
		/// </summary>
		public static ASColorMode ColorMode
			{
			get
				{
				return (ASColorMode)RDGenerics.GetSettings (colorModePar, (uint)ASColorMode.AllColors);
				}
			set
				{
				RDGenerics.SetSettings (colorModePar, (uint)value);
				}
			}
		private const string colorModePar = "ColorMode";

		/// <summary>
		/// Возвращает или задаёт абсолютную ширину изображения
		/// </summary>
		public static uint AbsoluteWidth
			{
			get
				{
				return RDGenerics.GetSettings (absoluteWidthPar, 0);
				}
			set
				{
				RDGenerics.SetSettings (absoluteWidthPar, value);
				}
			}
		private const string absoluteWidthPar = "AbsoluteWidth";

		/// <summary>
		/// Возвращает или задаёт абсолютную высоту изображения
		/// </summary>
		public static uint AbsoluteHeight
			{
			get
				{
				return RDGenerics.GetSettings (absoluteHeightPar, 0);
				}
			set
				{
				RDGenerics.SetSettings (absoluteHeightPar, value);
				}
			}
		private const string absoluteHeightPar = "AbsoluteHeight";

		/// <summary>
		/// Возвращает или задаёт относительную ширину изображения
		/// </summary>
		public static uint RelativeWidth
			{
			get
				{
				return RDGenerics.GetSettings (relativeWidthPar, 0);
				}
			set
				{
				RDGenerics.SetSettings (relativeWidthPar, value);
				}
			}
		private const string relativeWidthPar = "RelativeWidth";

		/// <summary>
		/// Возвращает или задаёт относительную высоту изображения
		/// </summary>
		public static uint RelativeHeight
			{
			get
				{
				return RDGenerics.GetSettings (relativeHeightPar, 0);
				}
			set
				{
				RDGenerics.SetSettings (relativeHeightPar, value);
				}
			}
		private const string relativeHeightPar = "RelativeHeight";

		/// <summary>
		/// Возвращает или задаёт относительное левое смещение изображения
		/// </summary>
		public static uint RelativeLeft
			{
			get
				{
				return RDGenerics.GetSettings (relativeLeftPar, 0);
				}
			set
				{
				RDGenerics.SetSettings (relativeLeftPar, value);
				}
			}
		private const string relativeLeftPar = "RelativeLeft";

		/// <summary>
		/// Возвращает или задаёт относительное верхнее смещение изображения
		/// </summary>
		public static uint RelativeTop
			{
			get
				{
				return RDGenerics.GetSettings (relativeTopPar, 0);
				}
			set
				{
				RDGenerics.SetSettings (relativeTopPar, value);
				}
			}
		private const string relativeTopPar = "RelativeTop";

		/// <summary>
		/// Возвращает или задаёт границу чёрно-белого преобразования
		/// </summary>
		public static byte BitmapEdge
			{
			get
				{
				uint v = RDGenerics.GetSettings (bitmapEdgePar, 128);
				return (byte)((v > maxBE) ? maxBE : v);
				}
			set
				{
				uint v = (value > maxBE) ? maxBE : value;
				RDGenerics.SetSettings (bitmapEdgePar, v);
				}
			}
		private const string bitmapEdgePar = "BitmapEdge";
		private const uint maxBE = 255;

		/// <summary>
		/// Возвращает или задаёт режим отражения изображения
		/// </summary>
		public static ASFlipType FlipType
			{
			get
				{
				return (ASFlipType)RDGenerics.GetSettings (flipTypePar, (uint)ASFlipType.None);
				}
			set
				{
				RDGenerics.SetSettings (flipTypePar, (uint)value);
				}
			}
		private const string flipTypePar = "FlipType";

		/// <summary>
		/// Возвращает или задаёт режим поворота изображения
		/// </summary>
		public static ASRotationType RotationType
			{
			get
				{
				return (ASRotationType)RDGenerics.GetSettings (rotationTypePar, (uint)ASRotationType.None);
				}
			set
				{
				RDGenerics.SetSettings (rotationTypePar, (uint)value);
				}
			}
		private const string rotationTypePar = "RotationType";

		/// <summary>
		/// Возвращает или задаёт тип выходных изображений (без привязки к конкретным форматам)
		/// </summary>
		public static uint OutputImageType
			{
			get
				{
				return RDGenerics.GetSettings (outputImageTypePar, 0);
				}
			set
				{
				RDGenerics.SetSettings (outputImageTypePar, value);
				}
			}
		private const string outputImageTypePar = "OutputImageType";

		/// <summary>
		/// Возвращает или задаёт размещение водяного знака
		/// </summary>
		public static uint WatermarkPlacement
			{
			get
				{
				uint v = RDGenerics.GetSettings (watermarkPlacementPar, 8); // Нижний правый угол
				return (v > maxWP) ? maxWP : v;
				}
			set
				{
				uint v = (value > maxWP) ? maxWP : value;
				RDGenerics.SetSettings (watermarkPlacementPar, v);
				}
			}
		private const string watermarkPlacementPar = "WatermarkPlacement";
		private const uint maxWP = 8;

		/// <summary>
		/// Возвращает или задаёт путь к файлу водяного знака
		/// </summary>
		public static string WatermarkPath
			{
			get
				{
				return RDGenerics.GetSettings (watermarkPathPar, "");
				}
			set
				{
				RDGenerics.SetSettings (watermarkPathPar, value);
				}
			}
		private const string watermarkPathPar = "WatermarkPath";

		/// <summary>
		/// Возвращает или задаёт уровень непрозрачности водяного знака
		/// </summary>
		public static uint WatermarkOpacity
			{
			get
				{
				uint v = RDGenerics.GetSettings (watermarkOpacityPar, 0);
				return (v > maxWO) ? maxWO : v;
				}
			set
				{
				uint v = (value > maxWO) ? maxWO : value;
				RDGenerics.SetSettings (watermarkOpacityPar, v);
				}
			}
		private const string watermarkOpacityPar = "WatermarkOpacity";
		private const uint maxWO = 100;
		}

	/// <summary>
	/// Доступные режимы изменения размера изображения
	/// </summary>
	public enum ASResizingMode
		{
		/// <summary>
		/// Размер в пикселях
		/// </summary>
		AbsoluteSize,

		/// <summary>
		/// Размер в процентах
		/// </summary>
		RelativeSize,

		/// <summary>
		/// Размер в процентах с обрезкой
		/// </summary>
		RelativeCrop,
		}

	/// <summary>
	/// Доступные режимы изменения цветности изображения
	/// </summary>
	public enum ASColorMode
		{
		/// <summary>
		/// Сохранение цветов
		/// </summary>
		AllColors,

		/// <summary>
		/// Оттенки серого
		/// </summary>
		Greyscale,

		/// <summary>
		/// Только чёрный и белый
		/// </summary>
		Bitmap,
		}

	/// <summary>
	/// Доступные режимы отражения изображения
	/// </summary>
	public enum ASFlipType
		{
		/// <summary>
		/// Без отражения
		/// </summary>
		None = 0,

		/// <summary>
		/// Горизонтальное
		/// </summary>
		Horizontal = 4,

		/// <summary>
		/// Вертикальное
		/// </summary>
		Vertical = 6,

		/// <summary>
		/// Оба
		/// </summary>
		Both = 2,
		}

	/// <summary>
	/// Доступные режимы поворота изображения
	/// </summary>
	public enum ASRotationType
		{
		/// <summary>
		/// 0°
		/// </summary>
		None = 0,

		/// <summary>
		/// 90°
		/// </summary>
		Quarter = 1,

		/// <summary>
		/// 180°
		/// </summary>
		Half = 2,

		/// <summary>
		/// 270°
		/// </summary>
		HalfAndQuarter = 2,
		}
	}
