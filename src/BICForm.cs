using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Начальная форма программы
	/// </summary>
	public partial class BICForm: Form
		{
		// Переменные
		private List<ICodec> codecs = new List<ICodec> ();          // Списки обработчиков изображений
		private List<int> outputCodecsNumbers = new List<int> ();
		private List<object> outputFormats = new List<object> ();
		private uint successes = 0;                                 // Счётчики успешных обработок и общего числа изображений
		private double totalImages = 0.0;

		private int selectedFlip, selectedRotation, selectedOutputType; // Транзактные переменные
		private byte bitmapEdge;
		private List<string> messages = new List<string> ();

		private const string PBMcolors = "PBM (RGB)";
		private const string PBMgreyscale = "PBM (greyscale)";
		private const string PBMbitmap = "PBM (bitmap)";

		private SupportedLanguages al = Localization.CurrentLanguage;   // Язык интерфейса
		private bool allowPalettes = false;

		/// <summary>
		/// Главная форма программы
		/// </summary>
		public BICForm ()
			{
			// Начальная настройка
			InitializeComponent ();
			AcceptButton = StartButton;
			CancelButton = ExitButton;

			LanguageCombo.Items.AddRange (Localization.LanguagesNames);
			try
				{
				LanguageCombo.SelectedIndex = (int)al;
				}
			catch
				{
				LanguageCombo.SelectedIndex = 0;
				}

			// Настройка контролов
			RotationCombo.Items.Add ("0°");
			RotationCombo.Items.Add ("90°");
			RotationCombo.Items.Add ("180°");
			RotationCombo.Items.Add ("270°");

			RotationCombo.SelectedIndex = FlipCombo.SelectedIndex = 0;

			AbsoluteWidth.Minimum = AbsoluteHeight.Minimum = ProgramDescription.MinLinearSize;
			AbsoluteWidth.Maximum = AbsoluteHeight.Maximum = ProgramDescription.MaxLinearSize;
			RelativeWidth.Minimum = RelativeHeight.Minimum = 1;
			RelativeLeft.Minimum = RelativeTop.Minimum = 0;
			RelativeWidth.Maximum = RelativeHeight.Maximum = 100;
			RelativeLeft.Maximum = RelativeTop.Maximum = 99;
			AbsoluteSize_CheckedChanged (null, null);

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
				// Контроль совместимости
				if (BatchImageConvertorLibrary.LibraryVersion != ProgramDescription.LibraryVersion)
					{
					MessageBox.Show (string.Format (Localization.GetText ("IncompatibleLibraryVersion", al),
						ProgramDescription.AssemblyCodecsLibrary), ProgramDescription.AssemblyTitle,
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				else
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

					Palettes.Enabled = allowPalettes = true;
					}
				}

			// Запрос сохранённых настроек
			ImageTypeCombo.SelectedIndex = 0;
			try
				{
				// Безопасные настройки
				InputPath.Text = Registry.GetValue (ProgramDescription.AssemblySettingsKey, InputPath.Name, "").ToString ();
				OutputPath.Text = Registry.GetValue (ProgramDescription.AssemblySettingsKey, OutputPath.Name, "").ToString ();
				IncludeSubdirs.Checked = (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					IncludeSubdirs.Name, "").ToString () != "0");

				if (Registry.GetValue (ProgramDescription.AssemblySettingsKey, AbsoluteSize.Name, "").ToString () != "0")
					AbsoluteSize.Checked = true;
				if (Registry.GetValue (ProgramDescription.AssemblySettingsKey, RelativeCrop.Name, "").ToString () != "0")
					RelativeCrop.Checked = true;
				if (Registry.GetValue (ProgramDescription.AssemblySettingsKey, RelativeSize.Name, "").ToString () != "0")
					RelativeSize.Checked = true;    // Стандартная

				if (Registry.GetValue (ProgramDescription.AssemblySettingsKey, GreyscaleRadio.Name, "").ToString () != "0")
					GreyscaleRadio.Checked = true;
				if (Registry.GetValue (ProgramDescription.AssemblySettingsKey, BitmapRadio.Name, "").ToString () != "0")
					BitmapRadio.Checked = true;
				if (Registry.GetValue (ProgramDescription.AssemblySettingsKey, SaveColorsRadio.Name, "").ToString () != "0")
					SaveColorsRadio.Checked = true; // Стандартная

				// Сбросовые настройки
				AbsoluteWidth.Value = decimal.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					AbsoluteWidth.Name, "").ToString ());
				AbsoluteHeight.Value = decimal.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					AbsoluteHeight.Name, "").ToString ());
				RelativeWidth.Value = decimal.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					RelativeWidth.Name, "").ToString ());
				RelativeHeight.Value = decimal.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					RelativeHeight.Name, "").ToString ());
				RelativeTop.Value = decimal.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					RelativeTop.Name, "").ToString ());
				RelativeLeft.Value = decimal.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					RelativeLeft.Name, "").ToString ());

				BitmapEdgeTrack.Value = int.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					BitmapEdgeTrack.Name, "").ToString ());

				RotationCombo.SelectedIndex = int.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					RotationCombo.Name, "").ToString ());
				FlipCombo.SelectedIndex = int.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					FlipCombo.Name, "").ToString ());

				ImageTypeCombo.SelectedIndex = int.Parse (Registry.GetValue (ProgramDescription.AssemblySettingsKey,
					ImageTypeCombo.Name, "").ToString ());
				}
			catch
				{
				}

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
				InputPath.Text = InputFolder.SelectedPath;
			}

		// Выбор выходной папки
		private void SetOutputPath_Click (object sender, EventArgs e)
			{
			if (OutputFolder.ShowDialog () == DialogResult.OK)
				OutputPath.Text = OutputFolder.SelectedPath;
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
				MessageBox.Show (Localization.GetText ("InputPathNotSpecified", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			if (OutputPath.Text == "")
				{
				MessageBox.Show (Localization.GetText ("OutputPathNotSpecified", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			if (RelativeCrop.Checked && ((RelativeLeft.Value + RelativeWidth.Value > RelativeWidth.Maximum) ||
				(RelativeTop.Value + RelativeHeight.Value > RelativeHeight.Maximum)))
				{
				MessageBox.Show (Localization.GetText ("IncorrectCropValues", al),
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
			ResultsList.Items.Add (string.Format (Localization.GetText ("ResultText", al), (uint)totalImages, successes));

			// Выбор последней строки списка, если возможно
			if (ResultsList.Items.Count != 0)
				ResultsList.SelectedIndex = ResultsList.Items.Count - 1;

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
							IncludeSubdirs.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
						}
					catch
						{
						MessageBox.Show (Localization.GetText ("InputPathUnavailable", al),
							ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

						e.Cancel = true;
						return;
						}

					((BackgroundWorker)sender).ReportProgress ((int)HardWorkExecutor.ProgressBarSize,
						Localization.GetText ("ProcessingList", al));
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
					else    // Default
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
				imageColorFormat = OutputImageColorFormat.Greyscale;
			if (BitmapRadio.Checked)
				imageColorFormat = OutputImageColorFormat.Bitmap;

			// Определение общего числа обрабатываемых изображений
			double currentImage = 0.0;
			totalImages = 0.0;
			for (int c = 0; c < fileNames.Count; c++)
				{
				totalImages += fileNames[c].Count;
				}

			// Обработка
			Bitmap img = null;

			for (int c = 0; c < fileNames.Count; c++)
				{
				for (int n = 0; n < fileNames[c].Count; n++)
					{
					// Счётчик
					currentImage++;

					((BackgroundWorker)sender).ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
						totalImages), string.Format (Localization.GetText ("ProcessingText", al), Path.GetFileName (fileNames[c][n])));

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
						messages.Add (string.Format (Localization.GetText ("FileGeneric", al), Path.GetFileName (fileNames[c][n])) +
							Localization.GetText ("FileOverwrite", al));

						((BackgroundWorker)sender).ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
							totalImages), messages[messages.Count - 1]);
						continue;
						}
					#endregion

					#region Открытие изображения
					string msg = "";
					switch (codecs[c].LoadImage (fileNames[c][n], out img))
						{
						case ProgramErrorCodes.EXEC_FILE_UNAVAILABLE:
							msg = Localization.GetText ("FileUnavailable", al);
							break;

						case ProgramErrorCodes.EXEC_INVALID_FILE:
							msg = Localization.GetText ("FileIncorrect", al);
							break;

						case ProgramErrorCodes.EXEC_MEMORY_ALLOC_FAIL:
							msg = Localization.GetText ("NotEnoughMemory", al);
							break;

						case ProgramErrorCodes.EXEC_OK:
							break;

						// Других вариантов быть не должно
						default:
							throw new Exception (Localization.GetText ("DebugRequired", al) + " (1)");
						}
					if (msg != "")
						{
						msg = string.Format (Localization.GetText ("FileGeneric", al), Path.GetFileName (fileNames[c][n])) + msg;
						messages.Add (msg);
						((BackgroundWorker)sender).ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
							totalImages), msg);
						continue;
						}
					#endregion

					#region Размеры
					if (AbsoluteSize.Checked || (RelativeSize.Checked || RelativeCrop.Checked) &&
						((RelativeWidth.Value != 100) || (RelativeHeight.Value != 100)))
						{
						Bitmap img2;
						if (AbsoluteSize.Checked)
							{
							img2 = new Bitmap (img, new Size ((int)AbsoluteWidth.Value, (int)AbsoluteHeight.Value));
							}
						else
							{
							int w = (int)((double)RelativeWidth.Value / 100.0 * img.Width);
							if (w < ProgramDescription.MinLinearSize)
								{
								w = (int)ProgramDescription.MinLinearSize;
								}

							int h = (int)((double)RelativeHeight.Value / 100.0 * img.Height);
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

								int l = (int)((double)RelativeLeft.Value / 100.0 * img.Width);
								int t = (int)((double)RelativeTop.Value / 100.0 * img.Height);

								g.DrawImage (img, new Point (-l, -t));
								g.Dispose ();
								}
							}
						img.Dispose ();
						img = (Bitmap)img2.Clone ();
						img2.Dispose ();
						}
					#endregion

					#region Поворот/отражение (только если преобразование действительно есть (см. описание RotateFlipType enum))

					if (rfType != 0)
						img.RotateFlip (rfType);

					#endregion

					#region Сохранение
					// До этого места контроль на совпадение имён уже выполнен. Ошибки записи можно списать на недоступность папки
					if (codecs[outputCodecsNumbers[selectedOutputType]].SaveImage (img, outputPath, imageColorFormat,
						bitmapEdge, outputFormats[selectedOutputType]) != ProgramErrorCodes.EXEC_OK)
						{
						messages.Add (string.Format (Localization.GetText ("FileGeneric", al), Path.GetFileName (fileNames[c][n])) +
							Localization.GetText ("OutputPathUnavailable", al));

						img.Dispose ();
						((BackgroundWorker)sender).ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
							totalImages), messages[messages.Count - 1]);

						e.Cancel = true;
						return;
						}
					#endregion

					// Выполнено
					messages.Add (string.Format (Localization.GetText ("FileProcessed", al), Path.GetFileName (fileNames[c][n])));
					((BackgroundWorker)sender).ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
						totalImages), messages[messages.Count - 1]);
					successes++;
					img.Dispose ();
					}
				}
			}

		// Установка блокировки/разблокировки интерфейса
		private void SetInterfaceState (bool State)
			{
			SetInputPath.Enabled = SetOutputPath.Enabled = InputPath.Enabled = OutputPath.Enabled =
				ImageTypeCombo.Enabled = StartButton.Enabled = ExitButton.Enabled =
				RotationCombo.Enabled = FlipCombo.Enabled = CWLabel.Enabled = FlipLabel.Enabled =
				AbsoluteSize.Enabled = RelativeSize.Enabled = RelativeCrop.Enabled =
				LanguageCombo.Enabled = IncludeSubdirs.Enabled = State;

			if (State)
				{
				ImageTypeCombo_SelectedIndexChanged (null, null);
				AbsoluteSize_CheckedChanged (null, null);
				BitmapRadio_CheckedChanged (null, null);
				Palettes.Enabled = allowPalettes;
				}
			else
				{
				SaveColorsRadio.Enabled = GreyscaleRadio.Enabled = BitmapRadio.Enabled =
					AbsoluteWidth.Enabled = AbsoluteHeight.Enabled = RelativeWidth.Enabled = RelativeHeight.Enabled =
					Label06.Enabled = BitmapEdgeTrack.Visible = Palettes.Enabled = State;
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
		private void BICForm_HelpButtonClicked (object sender, CancelEventArgs e)
			{
			// Отмена общей обработки
			e.Cancel = true;

			// Сборка справки
			string types = "\r\n\r\n" + Localization.GetText ("SupportedFileTypes", al) + ":\r\n\r\n";
			for (int c = 0; c < codecs.Count; c++)
				{
				types += (" • " + codecs[c].ToString () + ": ");
				for (int t = 0; t < codecs[c].FileExtensions.Length - 1; t++)
					{
					types += (codecs[c].FileExtensions[t].Substring (2).ToUpper () + ", ");
					}

				types += codecs[c].FileExtensions[codecs[c].FileExtensions.Length - 1].Substring (2).ToUpper ();
				if (c < codecs.Count - 1)
					types += "\r\n\r\n";
				}

			// Отображение
			ProgramDescription.ShowAbout (Localization.GetText ("HelpText", al) + types, false);
			}

		// Выбор варианта задания размера
		private void AbsoluteSize_CheckedChanged (object sender, EventArgs e)
			{
			AbsoluteWidth.Enabled = AbsoluteHeight.Enabled = Label07.Enabled = AbsoluteSize.Checked;
			RelativeWidth.Enabled = RelativeHeight.Enabled = Label06.Enabled = (RelativeSize.Checked || RelativeCrop.Checked);
			RelativeLeft.Enabled = RelativeTop.Enabled = Label08.Enabled = CropCenter.Enabled = RelativeCrop.Checked;
			}

		// Установка порога яркости для чёрно-белого преобразования
		private void BitmapRadio_CheckedChanged (object sender, EventArgs e)
			{
			BitmapEdgeTrack.Visible = BitmapRadio.Checked;
			}

		// Запуск менеджера палитр
		private void Palettes_Click (object sender, EventArgs e)
			{
			if (!allowPalettes)
				return;

			PalettesManager pm = new PalettesManager (al);
			}

		// Центрирование области обрезки
		private void CropCenter_Click (object sender, EventArgs e)
			{
			RelativeLeft.Value = (RelativeWidth.Maximum - RelativeWidth.Value) / 2;
			RelativeTop.Value = (RelativeHeight.Maximum - RelativeHeight.Value) / 2;
			}

		// Выбор языка интерфейса
		private void LanguageCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			// Подготовка
			int flipType = (FlipCombo.SelectedIndex < 0) ? 0 : FlipCombo.SelectedIndex;
			FlipCombo.Items.Clear ();

			// Сохранение языка
			Localization.CurrentLanguage = al = (SupportedLanguages)LanguageCombo.SelectedIndex;

			// Замена
			InputFolder.Description = Localization.GetText ("InputFolderDescription", al);
			OutputFolder.Description = Localization.GetText ("OutputFolderDescription", al);
			IncludeSubdirs.Text = Localization.GetText ("IncludeSubdirsText", al);

			for (int i = 1; i <= 4; i++)
				FlipCombo.Items.Add (Localization.GetText ("FlipComboItems" + i.ToString (), al));

			InputSection.Text = Localization.GetText ("InputSectionText", al);
			ProcessingSection.Text = Localization.GetText ("ProcessingSectionText", al);

			SizeTab.Text = Localization.GetText ("SizesSectionText", al);
			AbsoluteSize.Text = Localization.GetText ("AbsoluteSizeText", al);
			RelativeSize.Text = Localization.GetText ("RelativeSizeText", al);
			RelativeCrop.Text = Localization.GetText ("RelativeCropText", al);
			CropCenter.Text = Localization.GetText ("CropCenterText", al);

			ColorTab.Text = Localization.GetText ("ColorsSectionText", al);
			SaveColorsRadio.Text = Localization.GetText ("SaveColorsRadioText", al);
			GreyscaleRadio.Text = Localization.GetText ("GreyscaleRadioText", al);
			BitmapRadio.Text = Localization.GetText ("BitmapRadioText", al);

			RotationTab.Text = Localization.GetText ("RotationSectionText", al);
			CWLabel.Text = Localization.GetText ("CWLabelText", al);
			FlipLabel.Text = Localization.GetText ("FlipLabelText", al);

			OutputSection.Text = Localization.GetText ("OutputSectionText", al);

			StartButton.Text = Localization.GetText ("BStart", al);
			Palettes.Text = Localization.GetText ("PalettesManager", al);
			ExitButton.Text = Localization.GetText ("BExit", al);

			// Завершено
			FlipCombo.SelectedIndex = flipType;
			}

		// Сохранение настроек
		private void BICForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			try
				{
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, InputPath.Name, InputPath.Text);
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, OutputPath.Name, OutputPath.Text);
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, IncludeSubdirs.Name,
					IncludeSubdirs.Checked ? "ISD" : "0");

				Registry.SetValue (ProgramDescription.AssemblySettingsKey, AbsoluteSize.Name,
					AbsoluteSize.Checked ? "AS" : "0");
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, RelativeSize.Name,
					RelativeSize.Checked ? "RS" : "0");
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, RelativeCrop.Name,
					RelativeCrop.Checked ? "RC" : "0");

				Registry.SetValue (ProgramDescription.AssemblySettingsKey, SaveColorsRadio.Name,
					SaveColorsRadio.Checked ? "SC" : "0");
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, GreyscaleRadio.Name,
					GreyscaleRadio.Checked ? "GS" : "0");
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, BitmapRadio.Name,
					BitmapRadio.Checked ? "BM" : "0");

				Registry.SetValue (ProgramDescription.AssemblySettingsKey, AbsoluteWidth.Name,
					((int)AbsoluteWidth.Value).ToString ());
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, AbsoluteHeight.Name,
					((int)AbsoluteHeight.Value).ToString ());
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, RelativeWidth.Name,
					((int)RelativeWidth.Value).ToString ());
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, RelativeHeight.Name,
					((int)RelativeHeight.Value).ToString ());
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, RelativeLeft.Name,
					((int)RelativeLeft.Value).ToString ());
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, RelativeTop.Name,
					((int)RelativeTop.Value).ToString ());

				Registry.SetValue (ProgramDescription.AssemblySettingsKey, BitmapEdgeTrack.Name,
					BitmapEdgeTrack.Value.ToString ());

				Registry.SetValue (ProgramDescription.AssemblySettingsKey, RotationCombo.Name,
					RotationCombo.SelectedIndex.ToString ());
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, FlipCombo.Name,
					FlipCombo.SelectedIndex.ToString ());

				Registry.SetValue (ProgramDescription.AssemblySettingsKey, ImageTypeCombo.Name,
					ImageTypeCombo.SelectedIndex.ToString ());
				}
			catch
				{
				}
			}
		}
	}
