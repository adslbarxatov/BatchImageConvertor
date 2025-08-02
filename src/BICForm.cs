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
		private List<ICodec> codecs = [];
		private List<int> outputCodecsNumbers = [];
		private List<object> outputFormats = [];
		private ICodec[] availableCodecs = [
			new GenericCodec (),
			new MetafileCodec (),
			new JP2Codec (),
			new AvifCodec (),
			new WebpCodec (),
			new TGACodec (),
			new PBMCodec (),
			new PCXCodec (),
			new ICOCodec (),
			];

		// Счётчики успешных обработок и общего числа изображений
		private uint successes = 0;
		private double totalImages = 0.0;

		// Транзактные переменные
		private List<string> messages = [];
		private bool allowPalettes = false;
		private RadioButton[] placements;

		// Запрет на перезагрузку настроек
		private bool allowSettingsReload = true;

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

			placements = [
				WatermarkLT,
				WatermarkCT,
				WatermarkRT,
				WatermarkLM,
				WatermarkCM,
				WatermarkRM,
				WatermarkLB,
				WatermarkCB,
				WatermarkRB,
				];

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
			RDGenerics.LoadWindowDimensions (this);
			string[] profiles = AppSettings.EnumerateProfiles ();
			if (profiles.Length < 1)
				{
				ProfileCombo.Enabled = ProfileRemoveButton.Enabled = false;
				}
			else
				{
				allowSettingsReload = false;

				ProfileCombo.Items.AddRange (profiles);
				try
					{
					ProfileCombo.SelectedIndex = (int)AppSettings.LastSelectedProfile;
					}
				catch
					{
					ProfileCombo.SelectedIndex = 0;
					}

				allowSettingsReload = true;
				}
			LoadSavedSettings (true);

			// Назначение заголовка окна
			this.Text = ProgramDescription.AssemblyTitle;
			if (!RDGenerics.AppHasAccessRights (false, true))
				this.Text += RDLocale.GetDefaultText (RDLDefaultTexts.Message_LimitedFunctionality);
			}

		// Метод загружает хранимые настройки
		private void LoadSavedSettings (bool Initial)
			{
			if (!allowSettingsReload)
				return;

			ImageTypeCombo.SelectedIndex = 0;
			try
				{
				// Безопасные настройки
				if (Initial)
					{
					InputPath.Text = AppSettings.InputPath;
					OutputPath.Text = AppSettings.OutputPath;
					IncludeSubdirs.Checked = AppSettings.IncludeSubdirs;
					}

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

				BitmapEdgeTrack.Value = AppSettings.BitmapEdge;
				RotationCombo.SelectedIndex = (int)AppSettings.RotationType;
				FlipCombo.SelectedIndex = ((int)AppSettings.FlipType) / 2;

				// Новые
				placements[(int)AppSettings.WatermarkPlacement].Checked = true;

				WatermarkPath.Text = AppSettings.WatermarkPath;
				WaterOpacityField.Value = AppSettings.WatermarkOpacity;

				// Настройки, требующие приведения к нижней границе
				AbsoluteWidth.Value = AppSettings.AbsoluteWidth;
				AbsoluteHeight.Value = AppSettings.AbsoluteHeight;
				RelativeWidth.Value = AppSettings.RelativeWidth;
				RelativeHeight.Value = AppSettings.RelativeHeight;
				RelativeLeft.Value = AppSettings.RelativeLeft;
				RelativeTop.Value = AppSettings.RelativeTop;

				// Настройки, которые могут зависеть от режима запуска и приводить к исключениям
				ImageTypeCombo.SelectedIndex = (int)AppSettings.OutputImageType;
				}
			catch { }
			}

		// Выбор языка интерфейса
		private void LanguageCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			// Подготовка
			int flipType = (FlipCombo.SelectedIndex < 0) ? 0 : FlipCombo.SelectedIndex;

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

			ProfileLabel.Text = RDLocale.GetText ("ProfileLabel");

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
				RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"InputPathNotSpecified");
				return;
				}

			if (string.IsNullOrWhiteSpace (OutputPath.Text))
				{
				RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"OutputPathNotSpecified");
				return;
				}

			if ((WaterOpacityField.Value > WaterOpacityField.Minimum) && string.IsNullOrWhiteSpace (WatermarkPath.Text))
				{
				RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"WatermarkPathNotSpecified");
				return;
				}

			if (RelativeCrop.Checked && ((RelativeLeft.Value + RelativeWidth.Value > RelativeWidth.Maximum) ||
				(RelativeTop.Value + RelativeHeight.Value > RelativeHeight.Maximum)))
				{
				RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"IncorrectCropValues");
				return;
				}

			// Подготовка транзактных переменных
			messages.Clear ();
			successes = 0;
			SaveSettings ();

			// Блокировка контролов
			ResultsList.Items.Clear ();
			SetInterfaceState (false);

			// Запуск
			RDInterface.RunWork (MasterImageProcessor, null, null,
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
			colorMatrix.Matrix33 = AppSettings.WatermarkOpacity / 100.0f;

			ImageAttributes sgAttributes = new ImageAttributes ();
			sgAttributes.SetColorMatrix (colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			Bitmap watermark = null;
			if ((AppSettings.WatermarkOpacity > 0) && !string.IsNullOrWhiteSpace (AppSettings.WatermarkPath))
				{
				if (codecs[0].LoadImage (AppSettings.WatermarkPath, out watermark) != ProgramErrorCodes.EXEC_OK)
					{
					RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
						"WatermarkPathUnavailable");

					e.Cancel = true;
					return;
					}
				}

			// Сбор списка изображений
			List<List<string>> fileNames = [];
			for (int i = 0; i < codecs.Count; i++)
				{
				fileNames.Add ([]);
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
						RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
							"InputPathUnavailable");

						e.Cancel = true;
						return;
						}

					bw.ReportProgress ((int)RDWorkerForm.ProgressBarSize,
						RDLocale.GetText ("ProcessingList"));
					}
				}

			// Удаление файла водяного знака из списка (касается только общего кодека)
			if (watermark != null)
				fileNames[0].Remove (AppSettings.WatermarkPath);

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

			// Определение общего числа обрабатываемых изображений
			double currentImage = 0.0;
			totalImages = 0.0;
			for (int c = 0; c < fileNames.Count; c++)
				totalImages += fileNames[c].Count;

			// Обработка
			for (int c = 0; c < fileNames.Count; c++)
				{
				for (int n = 0; n < fileNames[c].Count; n++)
					{
					// Счётчик
					currentImage++;

					bw.ReportProgress ((int)(RDWorkerForm.ProgressBarSize * currentImage /
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
					if (codecs[outputCodecsNumbers[(int)AppSettings.OutputImageType]].TestOutputFile (outputPath,
						outputFormats[(int)AppSettings.OutputImageType]) == "")
						{
						messages.Add (string.Format (RDLocale.GetText ("FileGeneric"),
							Path.GetFileName (fileNames[c][n])) + RDLocale.GetText ("FileOverwrite"));

						bw.ReportProgress ((int)(RDWorkerForm.ProgressBarSize *
							currentImage / totalImages), messages[messages.Count - 1]);
						continue;
						}

					#endregion

					#region Открытие изображения

					string msg = "";
					Bitmap img;
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

						bw.ReportProgress ((int)(RDWorkerForm.ProgressBarSize *
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
					if (codecs[outputCodecsNumbers[(int)AppSettings.OutputImageType]].SaveImage (img,
						outputPath, AppSettings.ColorMode, AppSettings.BitmapEdge,
						outputFormats[(int)AppSettings.OutputImageType]) != ProgramErrorCodes.EXEC_OK)
						{
						messages.Add (string.Format (RDLocale.GetText ("FileGeneric"),
							Path.GetFileName (fileNames[c][n])) + RDLocale.GetText ("OutputPathUnavailable"));
						img.Dispose ();

						bw.ReportProgress ((int)(RDWorkerForm.ProgressBarSize * currentImage /
							totalImages), messages[messages.Count - 1]);
						e.Cancel = true;
						return;
						}

					#endregion

					// Выполнено
					messages.Add (string.Format (RDLocale.GetText ("FileProcessed"),
						Path.GetFileName (fileNames[c][n])) +
						(watermarkIsTooBig ? RDLocale.GetText ("FileProcessedNoWatermark") : ""));

					bw.ReportProgress ((int)(RDWorkerForm.ProgressBarSize * currentImage /
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
			RDInterface.ShowAbout (false);
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
			RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.NoSound, types);
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

			_ = new PalettesManager ();
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

		// Метод сохраняет текущие настройки в реестр
		private void SaveSettings ()
			{
			AppSettings.InputPath = InputPath.Text;
			AppSettings.OutputPath = OutputPath.Text;

			AppSettings.IncludeSubdirs = IncludeSubdirs.Checked;

			if (RelativeCrop.Checked)
				AppSettings.ResizingMode = ASResizingMode.RelativeCrop;
			else if (AbsoluteSize.Checked)
				AppSettings.ResizingMode = ASResizingMode.AbsoluteSize;
			else
				AppSettings.ResizingMode = ASResizingMode.RelativeSize;

			if (GreyscaleRadio.Checked)
				AppSettings.ColorMode = ASColorMode.Greyscale;
			else if (BitmapRadio.Checked)
				AppSettings.ColorMode = ASColorMode.Bitmap;
			else
				AppSettings.ColorMode = ASColorMode.AllColors;

			AppSettings.AbsoluteWidth = (uint)AbsoluteWidth.Value;
			AppSettings.AbsoluteHeight = (uint)AbsoluteHeight.Value;
			AppSettings.RelativeWidth = (uint)RelativeWidth.Value;
			AppSettings.RelativeHeight = (uint)RelativeHeight.Value;
			AppSettings.RelativeLeft = (uint)RelativeLeft.Value;
			AppSettings.RelativeTop = (uint)RelativeTop.Value;
			AppSettings.BitmapEdge = (byte)BitmapEdgeTrack.Value;
			AppSettings.RotationType = (ASRotationType)((RotationCombo.SelectedIndex < 0) ? 0 :
				RotationCombo.SelectedIndex);
			AppSettings.FlipType = (ASFlipType)((FlipCombo.SelectedIndex < 0) ? 0 : FlipCombo.SelectedIndex * 2);
			AppSettings.OutputImageType = (uint)((ImageTypeCombo.SelectedIndex < 0) ? 0 : ImageTypeCombo.SelectedIndex);

			// Новые
			for (int i = 0; i < placements.Length; i++)
				if (placements[i].Checked)
					{
					AppSettings.WatermarkPlacement = (uint)i;
					break;
					}

			AppSettings.WatermarkPath = WatermarkPath.Text;
			AppSettings.WatermarkOpacity = (uint)WaterOpacityField.Value;
			}

		// Выбор профиля конверсии
		private void ProfileCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			// Контроль
			if (ProfileCombo.SelectedIndex < 0)
				return;
			AppSettings.LastSelectedProfile = (uint)ProfileCombo.SelectedIndex;

			// Попытка загрузки указанного профиля
			if (!AppSettings.LoadProfile (ProfileCombo.Text))
				RDInterface.LocalizedMessageBox (RDMessageFlags.Error | RDMessageFlags.CenterText,
					"BadProfileMessage", 2000);

			// Независимо от результата
			LoadSavedSettings (false);
			}

		// Сохранение профиля конверсии
		private void ProfileAddButton_Click (object sender, EventArgs e)
			{
			// Сохранение текущих параметров
			SaveSettings ();

			// Запрос имени
			string name = RDInterface.LocalizedMessageBox ("ProfileNameMessage", true, 30);

			// Попытка сохранения
			if (!AppSettings.SaveProfile (name))
				{
				RDInterface.LocalizedMessageBox (RDMessageFlags.Error | RDMessageFlags.CenterText,
					"BadProfileNameMessage");
				return;
				}

			// Добавление
			allowSettingsReload = false;

			ProfileCombo.Items.Add (name);
			ProfileCombo.SelectedIndex = ProfileCombo.Items.Count - 1;
			ProfileCombo.Enabled = ProfileRemoveButton.Enabled = true;

			allowSettingsReload = true;
			}

		// Удаление профиля конверсии
		private void ProfileRemoveButton_Click (object sender, EventArgs e)
			{
			// Защита
			if (RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
				RDLocale.GetText ("RemoveProfileMessage"),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_YesNoFocus),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)) ==
				RDMessageButtons.ButtonTwo)
				return;

			// Удаление
			AppSettings.RemoveProfile (ProfileCombo.Text);
			ProfileCombo.Items.RemoveAt (ProfileCombo.SelectedIndex);

			// Сброс
			if (ProfileCombo.Items.Count < 1)
				{
				ProfileCombo.Enabled = ProfileRemoveButton.Enabled = false;
				}
			else
				{
				allowSettingsReload = false;

				ProfileCombo.SelectedIndex = 0;

				allowSettingsReload = true;

				LoadSavedSettings (false);
				}
			}
		}
	}
