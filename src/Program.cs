using System;
using System.Windows.Forms;

namespace BatchImageConvertor
	{
	/// <summary>
	/// Класс описывает основную программу
	/// </summary>
	public static class Program
		{
		/// <summary>
		/// Конструктор. Описывает точку входа приложения
		/// </summary>
		[STAThread]
		public static void Main ()
			{
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new MainForm ());
			}
		}
	}
