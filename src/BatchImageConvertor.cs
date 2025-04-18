using RD_AAOW;
using System.Reflection;
using System.Resources;

// Управление общими сведениями о сборке
// ВИДИМЫЕ СТРОКИ
[assembly: AssemblyTitle (ProgramDescription.AssemblyDescription)]
[assembly: AssemblyCompany (RDGenerics.AssemblyCompany)]
// НЕВИДИМЫЕ СТРОКИ
[assembly: AssemblyDescription (ProgramDescription.AssemblyDescription)]
[assembly: AssemblyProduct (ProgramDescription.AssemblyTitle)]
[assembly: AssemblyCopyright (RDGenerics.AssemblyCopyright)]
[assembly: AssemblyVersion (ProgramDescription.AssemblyVersion)]

namespace RD_AAOW
	{
	/// <summary>
	/// Класс, содержащий сведения о программе
	/// </summary>
	public class ProgramDescription
		{
		/// <summary>
		/// Основное название сборки
		/// </summary>
		public const string AssemblyMainName = "BatchImageConvertor";

		/// <summary>
		/// Название программы
		/// </summary>
		public const string AssemblyTitle = AssemblyMainName + " v 4.2";

		/// <summary>
		/// Версия программы
		/// </summary>
		public const string AssemblyVersion = "4.2.0.0";

		/// <summary>
		/// Версия библиотеки
		/// </summary>
		public const string LibraryVersion = "4.2.0.0";

		/// <summary>
		/// Последнее обновление
		/// </summary>
		public const string AssemblyLastUpdate = "19.04.2025; 4:06";
		// Активен с 17.02.2018; 1:51

		/// <summary>
		/// Пояснение к программе
		/// </summary>
		public const string AssemblyDescription = "Batch image conversion utility";

		/// <summary>
		/// Минимальный линейный размер изображения в программе
		/// </summary>
		public const uint MinLinearSize = (1 << 4);

		/// <summary>
		/// Максимальный линейный размер изображения в программе
		/// </summary>
		public const uint MaxLinearSize = (1 << 14);

		/// <summary>
		/// Возвращает список менеджеров ресурсов для локализации приложения
		/// </summary>
		public readonly static ResourceManager[] AssemblyResources = [
			BatchImageConvertorResources.ResourceManager,

			BatchImageConvertor_ru_ru.ResourceManager,
			BatchImageConvertor_en_us.ResourceManager,
			];

		/// <summary>
		/// Возвращает набор ссылок на видеоматериалы по языкам
		/// </summary>
		public readonly static string[] AssemblyVideoLinks = [];

		/// <summary>
		/// Возвращает набор поддерживаемых языков
		/// </summary>
		public readonly static RDLanguages[] AssemblyLanguages = [
			RDLanguages.ru_ru,
			RDLanguages.en_us,
			];

		/// <summary>
		/// Возвращает описание сопоставлений файлов для приложения
		/// </summary>
		public readonly static string[][] AssemblyAssociations = [
			[ AppSettings.ProfileExt, RDLocale.GetText ("bipfile"), "BIPFileIcon", "-" ],
			];
		}
	}
