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
		private uint successes = 0;                         // Счётчики успешных обработок и общего числа изображений
		private double totalImages = 0.0;

		private int selectedFlip, selectedRotation, selectedOutputType; // Транзактные переменные
		private byte bitmapEdge;
		private List<string> messages = new List<string> ();

		private const string PBMcolors = "PBM, Portable bitmap format (RGB)";
		private const string PBMgreyscale = "PBM, Portable bitmap format (greyscale)";
		private const string PBMbitmap = "PBM, Portable bitmap format (B&W)";

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
				LanguageCombo.SelectedIndex = (int)Localization.CurrentLanguage;
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
			//RelativeWidth.Maximum = RelativeHeight.Maximum = 100;	// Определяется далее
			RelativeLeft.Maximum = RelativeTop.Maximum = 99;
			AbsoluteSize_CheckedChanged (null, null);

			// Перечисление основных кодеков
			codecs.Add (new GenericCodec ());
			codecs.Add (new MetafileCodec ());

			AddOutputCodec ("PNG, Portable network graphics", 0, ImageFormat.Png);
			AddOutputCodec ("JPEG, Joint photographic experts group", 0, ImageFormat.Jpeg);
			AddOutputCodec ("BMP, Windows bitmap", 0, ImageFormat.Bmp);
			AddOutputCodec ("GIF, Graphics interchange format", 0, ImageFormat.Gif);
			AddOutputCodec ("TIFF, Tagged image file format", 0, ImageFormat.Tiff);

			// Перечисление дополнительных кодеков
			if (File.Exists (RDGenerics.AppStartupPath + ProgramDescription.AssemblyCodecsLibrary))
				{
				// Контроль совместимости
				if (BatchImageConvertorLibrary.LibraryVersion != ProgramDescription.LibraryVersion)
					{
					RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning_Center, "IncompatibleLibraryVersion");
					}
				else
					{
					codecs.Add (new PBMCodec ());
					codecs.Add (new TGACodec ());
					codecs.Add (new PCXCodec ());
					codecs.Add (new ICOCodec ());
					//codecs.Add (new JP2Codec ());

					//AddOutputCodec ("JP2, JPEG 2000 file format", 5, JP2Codec.ImageTypes.JP2);
					//AddOutputCodec ("J2K, JPEG 2000 file format", 5, JP2Codec.ImageTypes.J2K);
					AddOutputCodec ("TGA, Truevision targa image", 3, null);
					AddOutputCodec ("PCX, PCExchange image format", 4, null);
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
				InputPath.Text = RDGenerics.GetAppSettingsValue (InputPath.Name);
				OutputPath.Text = RDGenerics.GetAppSettingsValue (OutputPath.Name);
				IncludeSubdirs.Checked = RDGenerics.GetAppSettingsValue (IncludeSubdirs.Name) != "0";

				switch (RDGenerics.GetAppSettingsValue (AbsoluteSize.Name))
					{
					default:
					case "0":
						RelativeSize.Checked = true;
						break;

					case "1":
						RelativeCrop.Checked = true;
						break;

					case "2":
						AbsoluteSize.Checked = true;
						break;
					}

				switch (RDGenerics.GetAppSettingsValue (GreyscaleRadio.Name))
					{
					default:
					case "0":
						SaveColorsRadio.Checked = true;
						break;

					case "1":
						BitmapRadio.Checked = true;
						break;

					case "2":
						GreyscaleRadio.Checked = true;
						break;
					}

				// Сбросовые настройки
				AbsoluteWidth.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (AbsoluteWidth.Name));
				AbsoluteHeight.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (AbsoluteHeight.Name));
				RelativeWidth.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (RelativeWidth.Name));
				RelativeHeight.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (RelativeHeight.Name));
				RelativeTop.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (RelativeTop.Name));
				RelativeLeft.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (RelativeLeft.Name));

				BitmapEdgeTrack.Value = int.Parse (RDGenerics.GetAppSettingsValue (BitmapEdgeTrack.Name));
				RotationCombo.SelectedIndex = int.Parse (RDGenerics.GetAppSettingsValue (RotationCombo.Name));
				FlipCombo.SelectedIndex = int.Parse (RDGenerics.GetAppSettingsValue (FlipCombo.Name));
				ImageTypeCombo.SelectedIndex = int.Parse (RDGenerics.GetAppSettingsValue (ImageTypeCombo.Name));
				}
			catch { }
			RDGenerics.LoadWindowDimensions (this);

			// Назначение заголовка окна
			this.Text = ProgramDescription.AssemblyTitle;
			}

		// Сбросы настроек преобразования
		private void DoNothingToSize_Click (object sender, EventArgs e)
			{
			RelativeSize.Checked = true;
			RelativeWidth.Value = RelativeHeight.Value = 100;
			}

		private void DoNothingToColor_Click (object sender, EventArgs e)
			{
			SaveColorsRadio.Checked = true;
			}

		private void DoNothingToRotation_Click (object sender, EventArgs e)
			{
			FlipCombo.SelectedIndex = RotationCombo.SelectedIndex = 0;
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
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning_Center, "InputPathNotSpecified");
				return;
				}

			if (OutputPath.Text == "")
				{
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning_Center, "OutputPathNotSpecified");
				return;
				}

			if (RelativeCrop.Checked && ((RelativeLeft.Value + RelativeWidth.Value > RelativeWidth.Maximum) ||
				(RelativeTop.Value + RelativeHeight.Value > RelativeHeight.Maximum)))
				{
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning_Center, "IncorrectCropValues");
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
			HardWorkExecutor hwe = new HardWorkExecutor (MasterImageProcessor, null, " ", true, true);

			// Завершение
			ResultsList.Items.AddRange (messages.ToArray ());
			ResultsList.Items.Add ("   ");
			ResultsList.Items.Add (string.Format (Localization.GetText ("ResultText"),
				(uint)totalImages, successes));

			// Выбор последней строки списка, если возможно
			if (ResultsList.Items.Count != 0)
				ResultsList.SelectedIndex = ResultsList.Items.Count - 1;

			// Разблокировка
			SetInterfaceState (true);
			}

		// Основной метод обработки изображений
		private void MasterImageProcessor (object sender, DoWorkEventArgs e)
			{
			// Инициализация
			BackgroundWorker bw = ((BackgroundWorker)sender);

			// Сбор списка изображений
			List<List<string>> fileNames = new List<List<string>> ();
			for (int i = 0; i < codecs.Count; i++)
				{
				fileNames.Add (new List<string> ());
				for (int j = 0; j < codecs[i].FileExtensions.Length; j++)
					{
					try
						{
						fileNames[fileNames.Count - 1].AddRange (Directory.GetFiles (InputPath.Text,
							codecs[i].FileExtensions[j], IncludeSubdirs.Checked ? SearchOption.AllDirectories :
							SearchOption.TopDirectoryOnly));
						}
					catch
						{
						RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning_Center, "InputPathUnavailable");

						e.Cancel = true;
						return;
						}

					bw.ReportProgress ((int)HardWorkExecutor.ProgressBarSize,
						Localization.GetText ("ProcessingList"));
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
				totalImages += fileNames[c].Count;

			// Обработка
			Bitmap img = null;

			for (int c = 0; c < fileNames.Count; c++)
				{
				for (int n = 0; n < fileNames[c].Count; n++)
					{
					// Счётчик
					currentImage++;

					bw.ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
						totalImages), string.Format (Localization.GetText ("ProcessingText"),
						Path.GetFileName (fileNames[c][n])));

					// Завершение работы, если получено требование от диалога (в том числе - на этапе сборки списка)
					if (bw.CancellationPending || e.Cancel)
						{
						e.Cancel = true;
						return;
						}

					#region Тест на возможность записи конечного изображения

					string outputPath = OutputPath.Text + "\\" + Path.GetFileNameWithoutExtension (fileNames[c][n]);
					if (codecs[outputCodecsNumbers[selectedOutputType]].TestOutputFile (outputPath,
						outputFormats[selectedOutputType]) == "")
						{
						messages.Add (string.Format (Localization.GetText ("FileGeneric"),
							Path.GetFileName (fileNames[c][n])) + Localization.GetText ("FileOverwrite"));

						bw.ReportProgress ((int)(HardWorkExecutor.ProgressBarSize *
							currentImage / totalImages), messages[messages.Count - 1]);
						continue;
						}

					#endregion

					#region Открытие изображения
					string msg = "";
					switch (codecs[c].LoadImage (fileNames[c][n], out img))
						{
						case ProgramErrorCodes.EXEC_FILE_UNAVAILABLE:
							msg = Localization.GetText ("FileUnavailable");
							break;

						case ProgramErrorCodes.EXEC_INVALID_FILE:
							msg = Localization.GetText ("FileIncorrect");
							break;

						case ProgramErrorCodes.EXEC_MEMORY_ALLOC_FAIL:
							msg = Localization.GetText ("NotEnoughMemory");
							break;

						case ProgramErrorCodes.EXEC_OK:
							break;

						// Других вариантов быть не должно
						default:
							throw new Exception (Localization.GetText ("DebugRequired") + " (1)");
						}
					if (msg != "")
						{
						msg = string.Format (Localization.GetText ("FileGeneric"),
							Path.GetFileName (fileNames[c][n])) + msg;
						messages.Add (msg);
						bw.ReportProgress ((int)(HardWorkExecutor.ProgressBarSize *
							currentImage / totalImages), msg);
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

					// До этого места контроль на совпадение имён уже выполнен.
					// Ошибки записи можно списать на недоступность папки
					if (codecs[outputCodecsNumbers[selectedOutputType]].SaveImage (img, outputPath, imageColorFormat,
						bitmapEdge, outputFormats[selectedOutputType]) != ProgramErrorCodes.EXEC_OK)
						{
						messages.Add (string.Format (Localization.GetText ("FileGeneric"),
							Path.GetFileName (fileNames[c][n])) + Localization.GetText ("OutputPathUnavailable"));

						img.Dispose ();
						bw.ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
							totalImages), messages[messages.Count - 1]);

						e.Cancel = true;
						return;
						}

					#endregion

					// Выполнено
					messages.Add (string.Format (Localization.GetText ("FileProcessed"),
						Path.GetFileName (fileNames[c][n])));
					bw.ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
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
				SaveColorsRadio.Enabled = GreyscaleRadio.Enabled = BitmapRadio.Enabled =
					DoNothingToColor.Enabled = false;
				}
			else if (ImageTypeCombo.Text == PBMbitmap)
				{
				BitmapRadio.Checked = true;
				SaveColorsRadio.Enabled = GreyscaleRadio.Enabled = BitmapRadio.Enabled =
					DoNothingToColor.Enabled = false;
				}
			else
				{
				SaveColorsRadio.Enabled = GreyscaleRadio.Enabled = BitmapRadio.Enabled =
					DoNothingToColor.Enabled = true;
				}
			}

		// Информация о поддерживаемых форматах
		private void BICForm_HelpButtonClicked (object sender, CancelEventArgs e)
			{
			// Отмена общей обработки
			e.Cancel = true;

			// Сборка справки
			string types = Localization.GetText ("SupportedFileTypes") + ":" + Localization.RNRN;
			for (int c = 0; c < codecs.Count; c++)
				{
				types += (" • " + codecs[c].ToString () + ": ");
				for (int t = 0; t < codecs[c].FileExtensions.Length - 1; t++)
					types += (codecs[c].FileExtensions[t].Substring (2).ToUpper () + ", ");

				types += codecs[c].FileExtensions[codecs[c].FileExtensions.Length - 1].Substring (2).ToUpper ();
				if (c < codecs.Count - 1)
					types += Localization.RN;
				}

			// Отображение
			RDGenerics.ShowAbout (false);
			RDGenerics.MessageBox (RDMessageTypes.Success_Left, types);
			}

		// Выбор варианта задания размера
		private void AbsoluteSize_CheckedChanged (object sender, EventArgs e)
			{
			AbsoluteWidth.Enabled = AbsoluteHeight.Enabled = Label07.Enabled = AbsoluteSize.Checked;
			RelativeWidth.Enabled = RelativeHeight.Enabled = Label06.Enabled = (RelativeSize.Checked || RelativeCrop.Checked);
			RelativeLeft.Enabled = RelativeTop.Enabled = Label08.Enabled = CropCenter.Enabled = RelativeCrop.Checked;

			RelativeWidth.Maximum = RelativeHeight.Maximum = RelativeSize.Checked ? 400 : 100;
			}

		// Установка порога яркости для чёрно-белого преобразования
		private void BitmapRadio_CheckedChanged (object sender, EventArgs e)
			{
			BitmapEdgeTrack.Visible = ThresholdLabel.Visible = BitmapRadio.Checked;
			}

		// Запуск менеджера палитр
		private void Palettes_Click (object sender, EventArgs e)
			{
			if (!allowPalettes)
				return;

			PalettesManager pm = new PalettesManager ();
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
			Localization.CurrentLanguage = (SupportedLanguages)LanguageCombo.SelectedIndex;

			// Загрузка и сохранение
			LoadingTab.Text = Localization.GetText (LoadingTab.Name);
			InputFolder.Description = Localization.GetText ("InputFolderDescription");
			IncludeSubdirs.Text = Localization.GetText ("IncludeSubdirsText");
			InputLabel.Text = Localization.GetText ("InputLabel");

			OutputFolder.Description = Localization.GetText ("OutputFolderDescription");
			OutputLabel.Text = Localization.GetText ("OutputLabel");
			OutputFormatLabel.Text = Localization.GetText ("OutputFormatLabel");
			StartButton.Text = Localization.GetText ("BStart");

			// Размеры
			SizeTab.Text = Localization.GetText (SizeTab.Name);
			AbsoluteSize.Text = Localization.GetText ("AbsoluteSizeText");
			RelativeSize.Text = Localization.GetText ("RelativeSizeText");
			RelativeCrop.Text = Localization.GetText ("RelativeCropText");
			CropCenter.Text = Localization.GetText ("CropCenterText");
			DoNothingToSize.Text = Localization.GetText ("DoNothing");

			// Цвета
			ColorTab.Text = Localization.GetText (ColorTab.Name);
			SaveColorsRadio.Text = Localization.GetText ("SaveColorsRadioText");
			GreyscaleRadio.Text = Localization.GetText ("GreyscaleRadioText");
			BitmapRadio.Text = Localization.GetText ("BitmapRadioText");
			ThresholdLabel.Text = Localization.GetText ("ThresholdLabel");
			DoNothingToColor.Text = Localization.GetText ("DoNothing");

			// Поворот и отражение
			RotationTab.Text = Localization.GetText (RotationTab.Name);
			for (int i = 1; i <= 4; i++)
				FlipCombo.Items.Add (Localization.GetText ("FlipComboItems" + i.ToString ()));
			CWLabel.Text = Localization.GetText ("CWLabelText");
			FlipLabel.Text = Localization.GetText ("FlipLabelText");
			DoNothingToRotation.Text = Localization.GetText ("DoNothing");

			// Прочее
			OthersTab.Text = Localization.GetText (OthersTab.Name);
			Palettes.Text = Localization.GetText ("PalettesManager");
			ExitButton.Text = Localization.GetDefaultText (LzDefaultTextValues.Button_Exit);
			LanguageLabel.Text = Localization.GetDefaultText (LzDefaultTextValues.Control_InterfaceLanguage);

			// Завершено
			FlipCombo.SelectedIndex = flipType;
			}

		// Сохранение настроек
		private void BICForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			try
				{
				RDGenerics.SetAppSettingsValue (InputPath.Name, InputPath.Text);
				RDGenerics.SetAppSettingsValue (OutputPath.Name, OutputPath.Text);
				RDGenerics.SetAppSettingsValue (IncludeSubdirs.Name, IncludeSubdirs.Checked ? "ISD" : "0");

				if (RelativeCrop.Checked)
					RDGenerics.SetAppSettingsValue (AbsoluteSize.Name, "1");
				else if (AbsoluteSize.Checked)
					RDGenerics.SetAppSettingsValue (AbsoluteSize.Name, "2");
				else
					RDGenerics.SetAppSettingsValue (AbsoluteSize.Name, "0");

				if (GreyscaleRadio.Checked)
					RDGenerics.SetAppSettingsValue (GreyscaleRadio.Name, "2");
				else if (BitmapRadio.Checked)
					RDGenerics.SetAppSettingsValue (GreyscaleRadio.Name, "1");
				else
					RDGenerics.SetAppSettingsValue (GreyscaleRadio.Name, "0");

				RDGenerics.SetAppSettingsValue (AbsoluteWidth.Name, ((int)AbsoluteWidth.Value).ToString ());
				RDGenerics.SetAppSettingsValue (AbsoluteHeight.Name, ((int)AbsoluteHeight.Value).ToString ());
				RDGenerics.SetAppSettingsValue (RelativeWidth.Name, ((int)RelativeWidth.Value).ToString ());
				RDGenerics.SetAppSettingsValue (RelativeHeight.Name, ((int)RelativeHeight.Value).ToString ());
				RDGenerics.SetAppSettingsValue (RelativeLeft.Name, ((int)RelativeLeft.Value).ToString ());
				RDGenerics.SetAppSettingsValue (RelativeTop.Name, ((int)RelativeTop.Value).ToString ());

				RDGenerics.SetAppSettingsValue (BitmapEdgeTrack.Name, BitmapEdgeTrack.Value.ToString ());
				RDGenerics.SetAppSettingsValue (RotationCombo.Name, RotationCombo.SelectedIndex.ToString ());
				RDGenerics.SetAppSettingsValue (FlipCombo.Name, FlipCombo.SelectedIndex.ToString ());
				RDGenerics.SetAppSettingsValue (ImageTypeCombo.Name, ImageTypeCombo.SelectedIndex.ToString ());
				}
			catch { }
			RDGenerics.SaveWindowDimensions (this);
			}
		}
	}
