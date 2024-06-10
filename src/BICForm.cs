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
		// Списки обработчиков изображений
		private List<ICodec> codecs = new List<ICodec> ();
		private List<int> outputCodecsNumbers = new List<int> ();
		private List<object> outputFormats = new List<object> ();
		private ICodec[] availableCodecs = new ICodec[] {
			new GenericCodec (),
			new MetafileCodec (),
			new JP2Codec (),
			new AvifCodec (),
			new WebpCodec (),
			new TGACodec (),
			new PBMCodec (),
			new PCXCodec (),
			new ICOCodec (),
			};

		// Счётчики успешных обработок и общего числа изображений
		private uint successes = 0;
		private double totalImages = 0.0;

		// Транзактные переменные
		/*private int selectedFlip;
		private int selectedRotation;
		private int selectedOutputType;
		private byte bitmapEdge;*/
		private List<string> messages = new List<string> ();
		private bool allowPalettes = false;
		private RadioButton[] placements;

		/*private string watermarkPath;
		private uint watermarkOpacity;
		private uint watermarkPlacement;*/

		/// <summary>
		/// Главная форма программы
		/// </summary>
		/// <param name="LibraryUnavailable">Результат проверки библиотеки на доступность</param>
		public BICForm (bool LibraryUnavailable)
			{
			// Начальная настройка
			InitializeComponent ();
			AcceptButton = StartButton;
			CancelButton = ExitButton;

			LanguageCombo.Items.AddRange (RDLocale.LanguagesNames);
			try
				{
				LanguageCombo.SelectedIndex = (int)RDLocale.CurrentLanguage;
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

			placements = new RadioButton[] {
				WatermarkLT,
				WatermarkCT,
				WatermarkRT,
				WatermarkLM,
				WatermarkCM,
				WatermarkRM,
				WatermarkLB,
				WatermarkCB,
				WatermarkRB,
				};

			AbsoluteSize_CheckedChanged (null, null);

			// Перечисление кодеков
			for (int i = 0; i < availableCodecs.Length; i++)
				{
				if (!availableCodecs[i].IsCodecAvailable (LibraryUnavailable))
					continue;

				codecs.Add (availableCodecs[i]);
				for (int j = 0; j < availableCodecs[i].OutputModeSettings.Length; j++)
					AddOutputCodec (availableCodecs[i].OutputModeSettings[j][0].ToString (),
						codecs.Count - 1, availableCodecs[i].OutputModeSettings[j][1]);
				}

			// Защита от входа без библиотеки
			Palettes.Enabled = allowPalettes = !LibraryUnavailable;

			// Запрос сохранённых настроек
			ImageTypeCombo.SelectedIndex = 0;
			try
				{
				// Безопасные настройки
				/*InputPath.Text = RDGenerics.GetAppSettingsValue (InputPath.Name);
				OutputPath.Text = RDGenerics.GetAppSettingsValue (OutputPath.Name);
				IncludeSubdirs.Checked = RDGenerics.GetAppSettingsValue (IncludeSubdirs.Name) != "0";*/
				InputPath.Text = AppSettings.InputPath;
				OutputPath.Text = AppSettings.OutputPath;
				IncludeSubdirs.Checked = AppSettings.IncludeSubdirs;

				/*switch (RDGenerics.GetAppSettingsValue (AbsoluteSize.Name))
				*/
				switch (AppSettings.ResizingMode)
					{
					default:
					case ASResizingMode.RelativeSize:
						RelativeSize.Checked = true;
						break;

					case ASResizingMode.RelativeCrop:
						RelativeCrop.Checked = true;
						break;

					case ASResizingMode.AbsoluteSize:
						AbsoluteSize.Checked = true;
						break;
					}

				/*switch (RDGenerics.GetAppSettingsValue (GreyscaleRadio.Name))
				*/
				switch (AppSettings.ColorMode)
					{
					default:
					case ASColorMode.AllColors:
						SaveColorsRadio.Checked = true;
						break;

					case ASColorMode.Bitmap:
						BitmapRadio.Checked = true;
						break;

					case ASColorMode.Greyscale:
						GreyscaleRadio.Checked = true;
						break;
					}

				/*BitmapEdgeTrack.Value = int.Parse (RDGenerics.GetAppSettingsValue (BitmapEdgeTrack.Name));
				*/
				BitmapEdgeTrack.Value = AppSettings.BitmapEdge;

				/*RotationCombo.SelectedIndex = int.Parse (RDGenerics.GetAppSettingsValue (RotationCombo.Name));
				*/
				RotationCombo.SelectedIndex = (int)AppSettings.RotationType;

				/*FlipCombo.SelectedIndex = int.Parse (RDGenerics.GetAppSettingsValue (FlipCombo.Name));
				*/
				FlipCombo.SelectedIndex = ((int)AppSettings.FlipType) / 2;

				// Новые
				/*uint b = uint.Parse (RDGenerics.GetAppSettingsValue (WatermarkCM.Name));
				if (b < placements.Length)
					placements[b].Checked = true;*/
				placements[(int)AppSettings.WatermarkPlacement].Checked = true;

				/*WatermarkPath.Text = RDGenerics.GetAppSettingsValue (WatermarkPath.Name);
				WaterOpacityField.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (WaterOpacityField.Name));*/
				WatermarkPath.Text = AppSettings.WatermarkPath;
				WaterOpacityField.Value = AppSettings.WatermarkOpacity;

				// Настройки, требующие приведения к нижней границе
				/*AbsoluteWidth.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (AbsoluteWidth.Name));
				AbsoluteHeight.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (AbsoluteHeight.Name));
				RelativeWidth.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (RelativeWidth.Name));
				RelativeHeight.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (RelativeHeight.Name));
				RelativeTop.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (RelativeTop.Name));
				RelativeLeft.Value = decimal.Parse (RDGenerics.GetAppSettingsValue (RelativeLeft.Name));*/
				AbsoluteWidth.Value = AppSettings.AbsoluteWidth;
				AbsoluteHeight.Value = AppSettings.AbsoluteHeight;
				RelativeWidth.Value = AppSettings.RelativeWidth;
				RelativeHeight.Value = AppSettings.RelativeHeight;
				RelativeLeft.Value = AppSettings.RelativeLeft;
				RelativeTop.Value = AppSettings.RelativeTop;

				// Настройки, которые могут зависеть от режима запуска и приводить к исключениям
				/*ImageTypeCombo.SelectedIndex = int.Parse (RDGenerics.GetAppSettingsValue (ImageTypeCombo.Name));
				*/
				ImageTypeCombo.SelectedIndex = (int)AppSettings.OutputImageType;
				}
			catch { }
			RDGenerics.LoadWindowDimensions (this);

			// Назначение заголовка окна
			this.Text = ProgramDescription.AssemblyTitle;
			if (!RDGenerics.AppHasAccessRights (false, true))
				this.Text += RDLocale.GetDefaultText (RDLDefaultTexts.Message_LimitedFunctionality);
			}

		// Выбор языка интерфейса
		private void LanguageCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			// Подготовка
			int flipType = (FlipCombo.SelectedIndex < 0) ? 0 : FlipCombo.SelectedIndex;
			/*int flipType = (FlipCombo.SelectedIndex < 0) ? 0 : FlipCombo.SelectedIndex;
			*/
			FlipCombo.Items.Clear ();

			// Сохранение языка
			RDLocale.CurrentLanguage = (RDLanguages)LanguageCombo.SelectedIndex;

			// Загрузка и сохранение
			LoadingTab.Text = RDLocale.GetText (LoadingTab.Name);
			InputFolder.Description = RDLocale.GetText ("InputFolderDescription");
			IncludeSubdirs.Text = RDLocale.GetText ("IncludeSubdirsText");
			InputLabel.Text = RDLocale.GetText ("InputLabel");

			OutputFolder.Description = RDLocale.GetText ("OutputFolderDescription");
			OutputLabel.Text = RDLocale.GetText ("OutputLabel");
			OutputFormatLabel.Text = RDLocale.GetText ("OutputFormatLabel");
			StartButton.Text = RDLocale.GetText ("BStart");

			// Размеры
			SizeTab.Text = RDLocale.GetText (SizeTab.Name);
			AbsoluteSize.Text = RDLocale.GetText ("AbsoluteSizeText");
			RelativeSize.Text = RDLocale.GetText ("RelativeSizeText");
			RelativeCrop.Text = RDLocale.GetText ("RelativeCropText");
			CropCenter.Text = RDLocale.GetText ("CropCenterText");
			DoNothingToSize.Text = RDLocale.GetText ("DoNothing");

			// Цвета
			ColorTab.Text = RDLocale.GetText (ColorTab.Name);
			SaveColorsRadio.Text = RDLocale.GetText ("SaveColorsRadioText");
			GreyscaleRadio.Text = RDLocale.GetText ("GreyscaleRadioText");
			BitmapRadio.Text = RDLocale.GetText ("BitmapRadioText");
			ThresholdLabel.Text = RDLocale.GetText ("ThresholdLabel");
			DoNothingToColor.Text = RDLocale.GetText ("DoNothing");

			// Поворот и отражение
			RotationTab.Text = RDLocale.GetText (RotationTab.Name);
			for (int i = 1; i <= 4; i++)
				FlipCombo.Items.Add (RDLocale.GetText ("FlipComboItems" + i.ToString ()));

			CWLabel.Text = RDLocale.GetText ("CWLabelText");
			FlipLabel.Text = RDLocale.GetText ("FlipLabelText");
			DoNothingToRotation.Text = RDLocale.GetText ("DoNothing");

			// Прочее
			OthersTab.Text = RDLocale.GetText (OthersTab.Name);
			Palettes.Text = RDLocale.GetText ("PalettesManager");
			ExitButton.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit);
			LanguageLabel.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceLanguage);
			AboutTheApp.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout);
			SupportedExtButton.Text = RDLocale.GetText ("SupportedExtButton");

			// Водяной знак
			WaterTab.Text = RDLocale.GetText (WaterTab.Text);
			WatermarkLabel.Text = RDLocale.GetText ("WatermarkLabelText");
			WaterPlaceLabel.Text = RDLocale.GetText ("WaterPlaceLabelText");
			WaterOpacityLabel.Text = RDLocale.GetText ("WaterOpacityLabelText");
			DoNothingToWatermark.Text = RDLocale.GetText ("DoNothing");
			OFDialog.Filter = "Portable network graphics (PNG)|*.png";

			// Завершено
			FlipCombo.SelectedIndex = flipType;
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

		private void DoNothingToWatermark_Click (object sender, EventArgs e)
			{
			WaterOpacityField.Value = WaterOpacityField.Minimum;
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
			if (string.IsNullOrWhiteSpace (InputPath.Text))
				{
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning_Center, "InputPathNotSpecified");
				return;
				}

			if (string.IsNullOrWhiteSpace (OutputPath.Text))
				{
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning_Center, "OutputPathNotSpecified");
				return;
				}

			if ((WaterOpacityField.Value > WaterOpacityField.Minimum) && string.IsNullOrWhiteSpace (WatermarkPath.Text))
				{
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning_Center, "WatermarkPathNotSpecified");
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
			successes = 0;
			SaveSettings ();

			/*selectedFlip = FlipCombo.SelectedIndex;
			*/
			/*selectedRotation = RotationCombo.SelectedIndex;
			*/
			/*selectedOutputType = ImageTypeCombo.SelectedIndex;
			bitmapEdge = (byte)BitmapEdgeTrack.Value;*/
			/*watermarkPath = "";
			*/
			/*watermarkPath = WatermarkPath.Text;
				watermarkOpacity = (uint)WaterOpacityField.Value;*/
			/*watermarkPlacement = (uint)i;
						*/

			// Блокировка контролов
			ResultsList.Items.Clear ();
			SetInterfaceState (false);

			// Запуск
			RDGenerics.RunWork (MasterImageProcessor, null, null,
				RDRunWorkFlags.CaptionInTheMiddle | RDRunWorkFlags.AllowOperationAbort);

			// Завершение
			ResultsList.Items.AddRange (messages.ToArray ());
			ResultsList.Items.Add ("   ");
			ResultsList.Items.Add (string.Format (RDLocale.GetText ("ResultText"),
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

			// Контроль водяного знака
			ColorMatrix colorMatrix = new ColorMatrix ();
			/*colorMatrix.Matrix33 = watermarkOpacity / 100.0f;
			*/
			colorMatrix.Matrix33 = AppSettings.WatermarkOpacity / 100.0f;

			ImageAttributes sgAttributes = new ImageAttributes ();
			sgAttributes.SetColorMatrix (colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			Bitmap watermark = null;
			/*if (!string.IsNullOrWhiteSpace (watermarkPath))
			*/
			if ((AppSettings.WatermarkOpacity > 0) && !string.IsNullOrWhiteSpace (AppSettings.WatermarkPath))
				{
				/*if (codecs[0].LoadImage (watermarkPath, out watermark) != ProgramErrorCodes.EXEC_OK)
				*/
				if (codecs[0].LoadImage (AppSettings.WatermarkPath, out watermark) != ProgramErrorCodes.EXEC_OK)
					{
					RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning_Center, "WatermarkPathUnavailable");

					e.Cancel = true;
					return;
					}
				}

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
						RDLocale.GetText ("ProcessingList"));
					}
				}

			// Удаление файла водяного знака из списка (касается только общего кодека)
			if (watermark != null)
				fileNames[0].Remove (AppSettings.WatermarkPath);
			/*fileNames[0].Remove (watermarkPath);
			*/

			// Обпределение режима поворота
			RotateFlipType rfType = (RotateFlipType)AppSettings.FlipType;
			switch (AppSettings.FlipType)
				{
				case ASFlipType.Horizontal:
				case ASFlipType.None:
					rfType |= (RotateFlipType)AppSettings.RotationType;
					break;

				case ASFlipType.Vertical:
					if (AppSettings.RotationType >= ASRotationType.Half)
						rfType = (RotateFlipType)ASFlipType.Both;
					rfType |= (RotateFlipType)AppSettings.RotationType;
					break;

				case ASFlipType.Both:
					rfType |= (RotateFlipType)AppSettings.RotationType;
					if (AppSettings.RotationType >= ASRotationType.Half)
						rfType &= (RotateFlipType)0x1;
					break;
				}

			/*switch (selectedRotation)
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
				}*/

			/* Определение типа цветового преобразования
			ASColorMode imageColorFormat = ASColorMode.AllColors;
			if (GreyscaleRadio.Checked)
				imageColorFormat = ASColorMode.Greyscale;
			if (BitmapRadio.Checked)
				imageColorFormat = ASColorMode.Bitmap;*/

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
						totalImages), string.Format (RDLocale.GetText ("ProcessingText"),
						Path.GetFileName (fileNames[c][n])));

					// Завершение работы, если получено требование от диалога (в том числе - на этапе сборки списка)
					if (bw.CancellationPending || e.Cancel)
						{
						e.Cancel = true;
						return;
						}

					#region Тест на возможность записи конечного изображения

					string outputPath = OutputPath.Text + "\\" + Path.GetFileNameWithoutExtension (fileNames[c][n]);
					/*if (codecs[outputCodecsNumbers[selectedOutputType]].TestOutputFile (outputPath,
						outputFormats[selectedOutputType]) == "")*/
					if (codecs[outputCodecsNumbers[(int)AppSettings.OutputImageType]].TestOutputFile (outputPath,
						outputFormats[(int)AppSettings.OutputImageType]) == "")
						{
						messages.Add (string.Format (RDLocale.GetText ("FileGeneric"),
							Path.GetFileName (fileNames[c][n])) + RDLocale.GetText ("FileOverwrite"));

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
							msg = RDLocale.GetText ("FileUnavailable");
							break;

						case ProgramErrorCodes.EXEC_INVALID_FILE:
							msg = RDLocale.GetText ("FileIncorrect");
							break;

						case ProgramErrorCodes.EXEC_MEMORY_ALLOC_FAIL:
							msg = RDLocale.GetText ("NotEnoughMemory");
							break;

						case ProgramErrorCodes.EXEC_UNSUPPORTED_OS:
							msg = RDLocale.GetText ("UnsupportedOS");
							break;

						case ProgramErrorCodes.EXEC_OK:
							break;

						// Других вариантов быть не должно
						default:
							throw new Exception (RDLocale.GetText ("DebugRequired") + " (1)");
						}

					if (!string.IsNullOrWhiteSpace (msg))
						{
						msg = string.Format (RDLocale.GetText ("FileGeneric"),
							Path.GetFileName (fileNames[c][n])) + msg;
						messages.Add (msg);

						bw.ReportProgress ((int)(HardWorkExecutor.ProgressBarSize *
							currentImage / totalImages), msg);
						continue;
						}
					float resolution = img.HorizontalResolution;

					#endregion

					#region Обработка размеров

					if (AbsoluteSize.Checked || (RelativeSize.Checked || RelativeCrop.Checked) &&
						((RelativeWidth.Value != 100) || (RelativeHeight.Value != 100)))
						{
						Bitmap img2;
						if (AbsoluteSize.Checked)
							{
							img2 = new Bitmap (img, new Size ((int)AbsoluteWidth.Value, (int)AbsoluteHeight.Value));
							img2.SetResolution (resolution, resolution);
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
								img2.SetResolution (resolution, resolution);
								}
							else
								{
								img2 = new Bitmap (w, h);
								img2.SetResolution (resolution, resolution);

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

					#region Поворот / отражение (только если преобразование действительно есть)

					if (rfType != 0)
						img.RotateFlip (rfType);

					#endregion

					#region Водяной знак

					// Контроль применимости
					bool watermarkIsTooBig = false;
					if (watermark == null)
						goto save;

					if ((watermark.Width > img.Width) || (watermark.Height > img.Height))
						{
						watermarkIsTooBig = true;
						goto save;
						}

					// Формирование параметров
					Graphics gi = Graphics.FromImage (img);
					int left;
					/*switch (watermarkPlacement % 3)
					*/
					switch (AppSettings.WatermarkPlacement % 3)
						{
						case 1:
							left = (img.Width - watermark.Width) / 2;
							break;

						case 2:
							left = img.Width - watermark.Width;
							break;

						default:
							left = 0;
							break;
						}

					int top;
					/*switch (watermarkPlacement / 3)
					*/
					switch (AppSettings.WatermarkPlacement / 3)
						{
						case 1:
							top = (img.Height - watermark.Height) / 2;
							break;

						case 2:
							top = img.Height - watermark.Height;
							break;

						default:
							top = 0;
							break;
						}

					// Отрисовка
					gi.DrawImage (watermark, new Rectangle (left, top, watermark.Width, watermark.Height),
						0, 0, watermark.Width, watermark.Height, GraphicsUnit.Pixel, sgAttributes);
					gi.Dispose ();

					#endregion

					#region Сохранение

					// До этого места контроль на совпадение имён уже выполнен.
					// Ошибки записи можно списать на недоступность папки
					save:

					img.SetResolution (resolution, resolution);
					/*if (codecs[outputCodecsNumbers[selectedOutputType]].SaveImage (img, outputPath, imageColorFormat,
						bitmapEdge, outputFormats[selectedOutputType]) != ProgramErrorCodes.EXEC_OK)*/
					if (codecs[outputCodecsNumbers[(int)AppSettings.OutputImageType]].SaveImage (img,
						outputPath, AppSettings.ColorMode, AppSettings.BitmapEdge,
						outputFormats[(int)AppSettings.OutputImageType]) != ProgramErrorCodes.EXEC_OK)
						{
						messages.Add (string.Format (RDLocale.GetText ("FileGeneric"),
							Path.GetFileName (fileNames[c][n])) + RDLocale.GetText ("OutputPathUnavailable"));
						img.Dispose ();

						bw.ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
							totalImages), messages[messages.Count - 1]);
						e.Cancel = true;
						return;
						}

					#endregion

					// Выполнено
					messages.Add (string.Format (RDLocale.GetText ("FileProcessed"),
						Path.GetFileName (fileNames[c][n])) +
						(watermarkIsTooBig ? RDLocale.GetText ("FileProcessedNoWatermark") : ""));

					bw.ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * currentImage /
						totalImages), messages[messages.Count - 1]);
					successes++;
					img.Dispose ();
					}
				}

			// Сброс ресурсов
			sgAttributes.Dispose ();
			if (watermark != null)
				watermark.Dispose ();
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
			if (ImageTypeCombo.Text.Contains (ColorTransition.OnlyGreyscaleMarker))
				{
				GreyscaleRadio.Checked = true;
				SaveColorsRadio.Enabled = GreyscaleRadio.Enabled = BitmapRadio.Enabled =
					DoNothingToColor.Enabled = false;
				}
			else if (ImageTypeCombo.Text.Contains (ColorTransition.OnlyBnWMarker))
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

		// Информация о программе
		private void AboutTheApp_Click (object sender, EventArgs e)
			{
			RDGenerics.ShowAbout (false);
			}

		// Информация о поддерживаемых форматах
		private void SupportedExt_Click (object sender, EventArgs e)
			{
			// Сборка справки
			string types = RDLocale.GetText ("SupportedFileTypes") + ":" + RDLocale.RNRN;
			for (int c = 0; c < codecs.Count; c++)
				{
				string s = codecs[c].ToString ();
				s = s.Substring (s.IndexOf ('.') + 1);
				types += (" • " + s + ": ");

				for (int t = 0; t < codecs[c].FileExtensions.Length - 1; t++)
					types += (codecs[c].FileExtensions[t].Substring (2).ToUpper () + ", ");

				types += codecs[c].FileExtensions[codecs[c].FileExtensions.Length - 1].Substring (2).ToUpper ();
				if (c < codecs.Count - 1)
					types += RDLocale.RN;
				}

			// Отображение
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

		// Выбор файла водяного знака
		private void WatermarkButton_Click (object sender, EventArgs e)
			{
			OFDialog.FileName = WatermarkPath.Text;
			OFDialog.ShowDialog ();
			}

		private void OFDialog_FileOk (object sender, CancelEventArgs e)
			{
			WatermarkPath.Text = OFDialog.FileName;
			}

		// Сохранение настроек
		private void BICForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			SaveSettings ();
			RDGenerics.SaveWindowDimensions (this);
			}

		private void SaveSettings ()
			{
			/*RDGenerics.SetAppSettingsValue (InputPath.Name, InputPath.Text);
				RDGenerics.SetAppSettingsValue (OutputPath.Name, OutputPath.Text);*/
			AppSettings.InputPath = InputPath.Text;
			AppSettings.OutputPath = OutputPath.Text;

			/*RDGenerics.SetAppSettingsValue (IncludeSubdirs.Name, IncludeSubdirs.Checked ? "ISD" : "0");
			*/
			AppSettings.IncludeSubdirs = IncludeSubdirs.Checked;

			if (RelativeCrop.Checked)
				/*RDGenerics.SetAppSettingsValue (AbsoluteSize.Name, "1");
				*/
				AppSettings.ResizingMode = ASResizingMode.RelativeCrop;
			else if (AbsoluteSize.Checked)
				/*RDGenerics.SetAppSettingsValue (AbsoluteSize.Name, "2");
				*/
				AppSettings.ResizingMode = ASResizingMode.AbsoluteSize;
			else
				/*RDGenerics.SetAppSettingsValue (AbsoluteSize.Name, "0")
				*/
				AppSettings.ResizingMode = ASResizingMode.RelativeSize;

			if (GreyscaleRadio.Checked)
				/*RDGenerics.SetAppSettingsValue (GreyscaleRadio.Name, "2");
				*/
				AppSettings.ColorMode = ASColorMode.Greyscale;
			else if (BitmapRadio.Checked)
				/*RDGenerics.SetAppSettingsValue (GreyscaleRadio.Name, "1");
				*/
				AppSettings.ColorMode = ASColorMode.Bitmap;
			else
				/*RDGenerics.SetAppSettingsValue (GreyscaleRadio.Name, "0");
				*/
				AppSettings.ColorMode = ASColorMode.AllColors;

			/*RDGenerics.SetAppSettingsValue (AbsoluteWidth.Name, ((int)AbsoluteWidth.Value).ToString ());
			RDGenerics.SetAppSettingsValue (AbsoluteHeight.Name, ((int)AbsoluteHeight.Value).ToString ());
			RDGenerics.SetAppSettingsValue (RelativeWidth.Name, ((int)RelativeWidth.Value).ToString ());
			RDGenerics.SetAppSettingsValue (RelativeHeight.Name, ((int)RelativeHeight.Value).ToString ());
			RDGenerics.SetAppSettingsValue (RelativeLeft.Name, ((int)RelativeLeft.Value).ToString ());
			RDGenerics.SetAppSettingsValue (RelativeTop.Name, ((int)RelativeTop.Value).ToString ());*/
			AppSettings.AbsoluteWidth = (uint)AbsoluteWidth.Value;
			AppSettings.AbsoluteHeight = (uint)AbsoluteHeight.Value;
			AppSettings.RelativeWidth = (uint)RelativeWidth.Value;
			AppSettings.RelativeHeight = (uint)RelativeHeight.Value;
			AppSettings.RelativeLeft = (uint)RelativeLeft.Value;
			AppSettings.RelativeTop = (uint)RelativeTop.Value;

			/*RDGenerics.SetAppSettingsValue (BitmapEdgeTrack.Name, BitmapEdgeTrack.Value.ToString ());
			*/
			AppSettings.BitmapEdge = (byte)BitmapEdgeTrack.Value;

			/*RDGenerics.SetAppSettingsValue (RotationCombo.Name, RotationCombo.SelectedIndex.ToString ());
			*/
			AppSettings.RotationType = (ASRotationType)((RotationCombo.SelectedIndex < 0) ? 0 :
				RotationCombo.SelectedIndex);

			/*RDGenerics.SetAppSettingsValue (FlipCombo.Name, FlipCombo.SelectedIndex.ToString ());
			*/
			AppSettings.FlipType = (ASFlipType)((FlipCombo.SelectedIndex < 0) ? 0 : FlipCombo.SelectedIndex * 2);

			/*RDGenerics.SetAppSettingsValue (ImageTypeCombo.Name, ImageTypeCombo.SelectedIndex.ToString ());
			*/
			AppSettings.OutputImageType = (uint)((ImageTypeCombo.SelectedIndex < 0) ? 0 : ImageTypeCombo.SelectedIndex);

			// Новые
			for (int i = 0; i < placements.Length; i++)
				if (placements[i].Checked)
					{
					/*RDGenerics.SetAppSettingsValue (WatermarkCM.Name, i.ToString ());
					*/
					AppSettings.WatermarkPlacement = (uint)i;
					break;
					}

			/*RDGenerics.SetAppSettingsValue (WatermarkPath.Name, WatermarkPath.Text);
			RDGenerics.SetAppSettingsValue (WaterOpacityField.Name, WaterOpacityField.Value.ToString ());*/
			AppSettings.WatermarkPath = WatermarkPath.Text;
			AppSettings.WatermarkOpacity = (uint)WaterOpacityField.Value;
			}
		}
	}
