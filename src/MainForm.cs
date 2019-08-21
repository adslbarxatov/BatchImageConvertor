using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace BatchImageConvertor
	{
	/// <summary>
	/// Начальная форма программы
	/// </summary>
	public partial class MainForm:Form
		{
		// Переменные
		private List<ICodec> codecs = new List<ICodec> ();
		private List<int> outputCodecsNumbers = new List<int> ();
		private List<object> outputFormats = new List<object> ();
		private uint successes = 0;		// Счётчики успешных обработок и общего числа изображений
		private double totalImages = 0.0;

		private int selectedFlip, selectedRotation, selectedOutputType;	// Транзактные переменные
		private byte bitmapEdge;
		private List<string> messages = new List<string> ();

		private const string PBMcolors = "PBM (RGB)";
		private const string PBMgreyscale = "PBM (greyscale)";
		private const string PBMbitmap = "PBM (bitmap)";

		/// <summary>
		/// Главная форма программы
		/// </summary>
		public MainForm ()
			{
			// Начальная настройка
			InitializeComponent ();
			RU_CheckedChanged (null, null);

			// Настройка контролов
			RotationCombo.Items.Add ("0°");
			RotationCombo.Items.Add ("90°");
			RotationCombo.Items.Add ("180°");
			RotationCombo.Items.Add ("270°");

			RotationCombo.SelectedIndex = FlipCombo.SelectedIndex = 0;

			AbsoluteWidth.Minimum = AbsoluteHeight.Minimum = ProgramDescription.MinLinearSize;
			AbsoluteWidth.Maximum = AbsoluteHeight.Maximum = ProgramDescription.MaxLinearSize;
			RelativeWidth.Minimum = RelativeHeight.Minimum = 1;
			RelativeWidth.Maximum = RelativeHeight.Maximum = 100;

			// Перечисление основных кодеков
			codecs.Add (new GenericCodec ());
			codecs.Add (new MetafileCodec ());

			AddOutputCodec ("PNG", 0, ImageFormat.Png);
			AddOutputCodec ("JPEG", 0, ImageFormat.Jpeg);
			AddOutputCodec ("BMP", 0, ImageFormat.Bmp);
			AddOutputCodec ("GIF", 0, ImageFormat.Gif);
			AddOutputCodec ("TIFF", 0, ImageFormat.Tiff);

			// Перечисление дополнительных кодеков
			if (File.Exists (Application.StartupPath + "\\" + ProgramDescription.AssemblyCodecsLibrary))
				{
				codecs.Add (new PBMCodec ());
				codecs.Add (new TGACodec ());
				codecs.Add (new PCXCodec ());
				//codecs.Add (new JP2Codec ());		// Декодеры нестабильны

				//AddOutputCodec ("JP2", 5, JP2Codec.ImageTypes.JP2);	// Кодеры неисправны
				//AddOutputCodec ("J2K", 5, JP2Codec.ImageTypes.J2K);
				AddOutputCodec ("TGA", 3, null);
				AddOutputCodec ("PCX", 4, null);
				AddOutputCodec (PBMcolors, 2, PBMCodec.ImageTypes.ColorAsBinary);
				AddOutputCodec (PBMgreyscale, 2, PBMCodec.ImageTypes.GreyscaleAsBinary);
				AddOutputCodec (PBMbitmap, 2, PBMCodec.ImageTypes.BitmapAsBinary);

				Palettes.Enabled = true;
				}
			ImageTypeCombo.SelectedIndex = 0;

			// Назначение заголовка окна
			this.Text = ProgramDescription.AssemblyTitle;
			}

		// Метод добаляет формат вывода
		private void AddOutputCodec (string OutputFormatName, int OutputCodecNumber, object OutputParameters)
			{
			ImageTypeCombo.Items.Add (OutputFormatName);
			outputCodecsNumbers.Add (OutputCodecNumber);
			outputFormats.Add (OutputParameters);
			}

		// Выбор входной папки
		private void SetInputPath_Click (object sender, EventArgs e)
			{
			if (InputFolder.ShowDialog () == DialogResult.OK)
				{
				InputPath.Text = InputFolder.SelectedPath;
				}
			}

		// Выбор выходной папки
		private void SetOutputPath_Click (object sender, EventArgs e)
			{
			if (OutputFolder.ShowDialog () == DialogResult.OK)
				{
				OutputPath.Text = OutputFolder.SelectedPath;
				}
			}

		// Выход
		private void BExit_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Преобразование
		private void BStart_Click (object sender, EventArgs e)
			{
			// Проверка состояния
			if (InputPath.Text == "")
				{
				MessageBox.Show (RU.Checked ? "Директория с исходными изображениями не задана" :
					"Directory with source images not specified",
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			if (OutputPath.Text == "")
				{
				MessageBox.Show (RU.Checked ? "Директория для преобразованных изображений не задана" :
					"Directory for result images not specified",
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Подготовка транзактных переменных
			messages.Clear ();
			selectedFlip = FlipCombo.SelectedIndex;
			selectedRotation = RotationCombo.SelectedIndex;
			selectedOutputType = ImageTypeCombo.SelectedIndex;
			bitmapEdge = (byte)BitmapEdgeTrack.Value;
			ResultsList.Items.Clear ();
			successes = 0;

			// Блокировка контролов
			SetInterfaceState (false);

			// Запуск
			HardWorkExecutor hwe = new HardWorkExecutor (MasterImageProcessor);

			// Завершение
			ResultsList.Items.AddRange (messages.ToArray ());
			ResultsList.Items.Add ("   ");
			ResultsList.Items.Add ((RU.Checked ? "Обработано – " : "Processed – ") + totalImages.ToString () +
				(RU.Checked ? "; успешно – " : "; successes – ") + successes.ToString ());

			// Выбор последней строки списка, если возможно
			if (ResultsList.Items.Count != 0)
				{
				ResultsList.SelectedIndex = ResultsList.Items.Count - 1;
				}

			// Разблокировка
			SetInterfaceState (true);
			}

		// Основной метод обработки изображений
		private void MasterImageProcessor (object sender, DoWorkEventArgs e)
			{
			// Сбор списка изображений
			List<List<string>> fileNames = new List<List<string>> ();
			for (int i = 0; i < codecs.Count; i++)
				{
				fileNames.Add (new List<string> ());
				for (int j = 0; j < codecs[i].FileExtensions.Length; j++)
					{
					try
						{
						fileNames[fileNames.Count - 1].AddRange (Directory.GetFiles (InputPath.Text, codecs[i].FileExtensions[j],
							SearchOption.TopDirectoryOnly));
						}
					catch
						{
						MessageBox.Show (RU.Checked ? "Указанная директория с исходными изображениями недоступна" :
							"Specified input directory is unavailable",
							ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

						e.Cancel = true;
						return;
						}

					((BackgroundWorker)sender).ReportProgress (0, RU.Checked ? "Формирование списка изображений для обработки..." :
						"Assembling of processing list...");
					}
				}

			#region Определение типа поворота
			RotateFlipType rfType = (RotateFlipType)0;
			switch (selectedRotation)
				{
				// 0°
				default:
				case 0:
					if (selectedFlip == 1)
						rfType = RotateFlipType.RotateNoneFlipX;
					else if (selectedFlip == 2)
						rfType = RotateFlipType.RotateNoneFlipY;
					else if (selectedFlip == 3)
						rfType = RotateFlipType.RotateNoneFlipXY;
					else	// Default
						rfType = RotateFlipType.RotateNoneFlipNone;
					break;

				// 90°
				case 1:
					if (selectedFlip == 1)
						rfType = RotateFlipType.Rotate90FlipX;
					else if (selectedFlip == 2)
						rfType = RotateFlipType.Rotate90FlipY;
					else if (selectedFlip == 3)
						rfType = RotateFlipType.Rotate90FlipXY;
					else
						rfType = RotateFlipType.Rotate90FlipNone;
					break;

				// 180°
				case 2:
					if (selectedFlip == 1)
						rfType = RotateFlipType.Rotate180FlipX;
					else if (selectedFlip == 2)
						rfType = RotateFlipType.Rotate180FlipY;
					else if (selectedFlip == 3)
						rfType = RotateFlipType.Rotate180FlipXY;
					else
						rfType = RotateFlipType.Rotate180FlipNone;
					break;

				// 270°
				case 3:
					if (selectedFlip == 1)
						rfType = RotateFlipType.Rotate270FlipX;
					else if (selectedFlip == 2)
						rfType = RotateFlipType.Rotate270FlipY;
					else if (selectedFlip == 3)
						rfType = RotateFlipType.Rotate270FlipXY;
					else
						rfType = RotateFlipType.Rotate270FlipNone;
					break;
				}
			#endregion

			// Определение типа цветового преобразования
			OutputImageColorFormat imageColorFormat = OutputImageColorFormat.Color;
			if (GreyscaleRadio.Checked)
				{
				imageColorFormat = OutputImageColorFormat.Greyscale;
				}
			if (BitmapRadio.Checked)
				{
				imageColorFormat = OutputImageColorFormat.Bitmap;
				}

			// Определение общего числа обрабатываемых изображений
			double currentImage = 0.0;
			totalImages = 0.0;
			for (int c = 0; c < fileNames.Count; c++)
				{
				totalImages += fileNames[c].Count;
				}

			// Обработка
			Bitmap img = null;
			string msg;

			for (int c = 0; c < fileNames.Count; c++)
				{
				for (int n = 0; n < fileNames[c].Count; n++, currentImage++)
					{
					// Завершение работы, если получено требование от диалога (в том числе - на этапе сборки списка)
					if (((BackgroundWorker)sender).CancellationPending || e.Cancel)
						{
						e.Cancel = true;
						return;
						}

					#region Тест на возможность записи конечного изображения
					string outputPath = OutputPath.Text + "\\" + Path.GetFileNameWithoutExtension (fileNames[c][n]);
					if (codecs[outputCodecsNumbers[selectedOutputType]].TestOutputFile (outputPath,
						outputFormats[selectedOutputType]) == "")
						{
						if (RU.Checked)
							msg = "Файл «" + Path.GetFileName (fileNames[c][n]) + "»: не удалось сохранить " +
								"файл, т.к. конечный файл уже существует";
						else
							msg = "File '" + Path.GetFileName (fileNames[c][n]) + "': failed to save: " +
								"result file already exists";

						messages.Add (msg);
						((BackgroundWorker)sender).ReportProgress ((int)(100.0 * currentImage / totalImages), msg);
						continue;
						}
					#endregion

					#region Открытие изображения
					msg = "";
					switch (codecs[c].LoadImage (fileNames[c][n], out img))
						{
						case ProgramErrorCodes.EXEC_FILE_UNAVAILABLE:
							msg = RU.Checked ? "файл не найден или недоступен" : "file is unavailable";
							break;

						case ProgramErrorCodes.EXEC_INVALID_FILE:
							msg = RU.Checked ? "файл повреждён или не поддерживается" : "file is broken or has unsupported format";
							break;

						case ProgramErrorCodes.EXEC_MEMORY_ALLOC_FAIL:
							msg = RU.Checked ? "недостаточно памяти для обработки" : "not enough memory for processing";
							break;

						case ProgramErrorCodes.EXEC_OK:
							break;

						// Других вариантов быть не должно
						default:
							throw new Exception ("Ошибка порядка вызова функций. Требуется отладка приложения");
						}
					if (msg != "")
						{
						msg = (RU.Checked ? "Файл «" : "File '") + Path.GetFileName (fileNames[c][n]) +
							(RU.Checked ? "»: " : "': ") + msg;
						messages.Add (msg);
						((BackgroundWorker)sender).ReportProgress ((int)(100.0 * currentImage / totalImages), msg);
						continue;
						}
					#endregion

					#region Размеры
					if (AbsoluteSize.Checked ||
						(RelativeSize.Checked || RelativeCrop.Checked) && ((RelativeWidth.Value != 100) || (RelativeHeight.Value != 100)))
						{
						Bitmap img2;
						if (AbsoluteSize.Checked)
							{
							img2 = new Bitmap (img, new Size ((int)AbsoluteWidth.Value, (int)AbsoluteHeight.Value));
							}
						else
							{
							int w = (int)((double)RelativeWidth.Value / 100.0 * (double)img.Width);
							if (w < ProgramDescription.MinLinearSize)
								{
								w = (int)ProgramDescription.MinLinearSize;
								}

							int h = (int)((double)RelativeHeight.Value / 100.0 * (double)img.Height);
							if (h < ProgramDescription.MinLinearSize)
								{
								h = (int)ProgramDescription.MinLinearSize;
								}

							if (RelativeSize.Checked)
								{
								img2 = new Bitmap (img, new Size (w, h));
								}
							else
								{
								img2 = new Bitmap (w, h);
								Graphics g = Graphics.FromImage (img2);
								g.DrawImage (img, new Point ((img.Width - w) / -2, (img.Height - h) / -2));
								g.Dispose ();
								}
							}
						img.Dispose ();
						img = (Bitmap)img2.Clone ();
						img2.Dispose ();
						}
					#endregion

					#region Поворот/отражение (только если преобразование действительно есть (см. описание RotateFlipType enum))
					if ((int)rfType != 0)
						{
						img.RotateFlip (rfType);
						}
					#endregion

					#region Сохранение
					// До этого места контроль на совпадение имён уже выполнен. Ошибки записи можно списать на недоступность папки
					if (codecs[outputCodecsNumbers[selectedOutputType]].SaveImage (img, outputPath, imageColorFormat,
						bitmapEdge, outputFormats[selectedOutputType]) != ProgramErrorCodes.EXEC_OK)
						{
						if (RU.Checked)
							msg = "Файл «" + Path.GetFileName (fileNames[c][n]) + "»: не удалось сохранить " +
								"файл, т.к. конечная директория недоступна для записи";
						else
							msg = "File '" + Path.GetFileName (fileNames[c][n]) + "': cannot save: " +
								"output directory is not writable";

						messages.Add (msg);
						img.Dispose ();
						((BackgroundWorker)sender).ReportProgress ((int)(100.0 * currentImage / totalImages), msg);

						e.Cancel = true;
						return;
						}
					#endregion

					// Выполнено
					if (RU.Checked)
						msg = "Файл «" + Path.GetFileName (fileNames[c][n]) + "» обработан";
					else
						msg = "File '" + Path.GetFileName (fileNames[c][n]) + "' processed";

					messages.Add (msg);
					((BackgroundWorker)sender).ReportProgress ((int)(100.0 * currentImage / totalImages), msg);
					successes++;
					img.Dispose ();
					}
				}
			}

		// Установка блокировки/разблокировки интерфейса
		private void SetInterfaceState (bool State)
			{
			SetInputPath.Enabled = SetOutputPath.Enabled =
				ImageTypeCombo.Enabled = StartButton.Enabled = ExitButton.Enabled = Palettes.Enabled =
				RotationCombo.Enabled = FlipCombo.Enabled = Label04.Enabled = Label05.Enabled =
				AbsoluteSize.Enabled = RelativeSize.Enabled = RelativeCrop.Enabled =
				RU.Enabled = EN.Enabled = State;

			if (State)
				{
				ImageTypeCombo_SelectedIndexChanged (null, null);
				AbsoluteSize_CheckedChanged (null, null);
				BitmapRadio_CheckedChanged (null, null);
				}
			else
				{
				SaveColorsRadio.Enabled = GreyscaleRadio.Enabled = BitmapRadio.Enabled =
					AbsoluteWidth.Enabled = AbsoluteHeight.Enabled = RelativeWidth.Enabled = RelativeHeight.Enabled =
					Label06.Enabled = BitmapEdgeTrack.Visible = State;
				}
			}

		// Подгонка настроек под тип изображения
		private void ImageTypeCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			if (ImageTypeCombo.Text == PBMgreyscale)
				{
				GreyscaleRadio.Checked = true;
				SaveColorsRadio.Enabled = GreyscaleRadio.Enabled = BitmapRadio.Enabled = false;
				}
			else if (ImageTypeCombo.Text == PBMbitmap)
				{
				BitmapRadio.Checked = true;
				SaveColorsRadio.Enabled = GreyscaleRadio.Enabled = BitmapRadio.Enabled = false;
				}
			else
				{
				SaveColorsRadio.Enabled = GreyscaleRadio.Enabled = BitmapRadio.Enabled = true;
				}
			}

		// Информация о поддерживаемых форматах
		private void InputFormats_Click (object sender, EventArgs e)
			{
			string types = RU.Checked ? "Поддерживаемые типы файлов:\n\n" : "Supported file types:\n\n";
			for (int c = 0; c < codecs.Count; c++)
				{
				types += (" • " + codecs[c].ToString () + ": ");
				for (int t = 0; t < codecs[c].FileExtensions.Length - 1; t++)
					{
					types += (codecs[c].FileExtensions[t].Substring (1) + ", ");
					}
				types += (codecs[c].FileExtensions[codecs[c].FileExtensions.Length - 1].Substring (1) + ".\n\n");
				}

			MessageBox.Show (types, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK,
				MessageBoxIcon.Information);
			}

		// Выбор варианта задания размера
		private void AbsoluteSize_CheckedChanged (object sender, EventArgs e)
			{
			AbsoluteWidth.Enabled = AbsoluteHeight.Enabled = AbsoluteSize.Checked;
			RelativeWidth.Enabled = RelativeHeight.Enabled = Label06.Enabled = (RelativeSize.Checked || RelativeCrop.Checked);
			}

		// Установка порога яркости для чёрно-белого преобразования
		private void BitmapRadio_CheckedChanged (object sender, EventArgs e)
			{
			BitmapEdgeTrack.Visible = BitmapRadio.Checked;
			}

		// Запуск менеджера палитр
		private void Palettes_Click (object sender, EventArgs e)
			{
			if (!Palettes.Enabled)
				return;

			PalettesManager pm = new PalettesManager (RU.Checked);
			}

		// Выбор языка интерфейса
		private void RU_CheckedChanged (object sender, EventArgs e)
			{
			// Подготовка
			int flipType = (FlipCombo.SelectedIndex < 0) ? 0 : FlipCombo.SelectedIndex;
			FlipCombo.Items.Clear ();

			// Замена
			if (RU.Checked)
				{
				InputFolder.Description = "Выберите директорию, изображения из которой требуется преобразовать";
				OutputFolder.Description = "Выберите директорию для сохранения преобразованных изображений. " +
					"ПРИ СОВПАДЕНИИ ИМЁН ФАЙЛОВ ПЕРЕЗАПИСЬ НЕ ВЫПОЛНЯЕТСЯ!";

				FlipCombo.Items.Add ("(не отражать)");
				FlipCombo.Items.Add ("по горизонтали");
				FlipCombo.Items.Add ("по вертикали");
				FlipCombo.Items.Add ("по обеим осям");

				Label01.Text = "1. Выберите директорию с исходными изображениями:";
				Label02.Text = "2. Задайте параметры преобразования:";

				GroupBox01.Text = "Размер конечных изображений";
				AbsoluteSize.Text = "Задать размер";
				RelativeSize.Text = "Изменить до";
				RelativeCrop.Text = "Обрезать до";

				GroupBox02.Text = "Цвета конечных изображений";
				SaveColorsRadio.Text = "Сохранить цвета";
				GreyscaleRadio.Text = "Оттенки серого";
				BitmapRadio.Text = "Чёрно-белый с порогом";

				GroupBox03.Text = "Поворот и отражение конечных изображений";
				Label04.Text = "Повернуть по ч. с. на";
				Label05.Text = "Отразить по";

				Label03.Text = "3. Укажите директорию для преобразованных изображений и их тип:";

				StartButton.Text = "4. Начать преобразование";
				Palettes.Text = "Менеджер палитр";
				ExitButton.Text = "Выход";
				}
			else
				{
				InputFolder.Description = "Select directory with source images";
				OutputFolder.Description = "Select directory for result images. " +
					"MATCHING FILES WILL NOT BE OVERWRITTEN!";

				FlipCombo.Items.Add ("(no flip)");
				FlipCombo.Items.Add ("horizontally");
				FlipCombo.Items.Add ("vertically");
				FlipCombo.Items.Add ("both");

				Label01.Text = "1. Specify directory with source images:";
				Label02.Text = "2. Specify conversion parameters:";

				GroupBox01.Text = "Output images' size";
				AbsoluteSize.Text = "Set size to";
				RelativeSize.Text = "Resize to";
				RelativeCrop.Text = "Crop to";

				GroupBox02.Text = "Output images' colors";
				SaveColorsRadio.Text = "Save colors";
				GreyscaleRadio.Text = "Greyscale";
				BitmapRadio.Text = "Bitmap with threshold";

				GroupBox03.Text = "Output images' flip and rotation";
				Label04.Text = "Rotate CW";
				Label05.Text = "Flip";

				Label03.Text = "3. Set output images' directory and type:";

				StartButton.Text = "4. Begin conversion";
				Palettes.Text = "Palettes manager";
				ExitButton.Text = "Quit";
				}

			FlipCombo.SelectedIndex = flipType;
			}
		}
	}
