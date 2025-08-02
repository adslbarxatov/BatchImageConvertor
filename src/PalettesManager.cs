using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает интерфейс работы с палитрами
	/// </summary>
	public partial class PalettesManager: Form
		{
		// Переменные
		private List<IPaletteCodec> codecs = [];
		private string[] filters = [
			"Adobe Color Table (*.act)|*.act",
			"Microsoft palette (*.pal)|*.pal",
			"JASC palette (*.pal)|*.pal",

			"Adobe Color Swatches (*.aco)|*.aco",
			"Adobe Swatches Exchange (*.ase)|*.ase",
			"Windows bitmap (*.bmp, *.dib, *.rle)|*.bmp;*.dib;*.rle",
			"ZSoft Paintbrush (*.pcx, *.pcc)|*.pcx;*.pcc"
			];
		private bool allowExit = false;
		private BMPCodec bmpCodec = new BMPCodec ();

		/// <summary>
		/// Конструктор. Запускает форму
		/// </summary>
		public PalettesManager ()
			{
			InitializeComponent ();
			RDGenerics.LoadWindowDimensions (this);

			// Настройка контролов
			this.Text = RDLocale.GetText ("PalettesManager").Replace ("&", "");

			OFDialog.Title = RDLocale.GetText ("OpenPaletteDialogTitle");
			SFDialog.Title = RDLocale.GetText ("SavePaletteDialogTitle");
			CFDialog.Title = RDLocale.GetText ("ChangePaletteDialogTitle");

			OFDialog.Filter = SFDialog.Filter = filters[0];
			for (int i = 1; i < 7; i++)
				{
				OFDialog.Filter += ("|" + filters[i]);

				if (i < 3)
					SFDialog.Filter += ("|" + filters[i]);
				}
			CFDialog.Filter = filters[5];

			codecs.Add (new ACTCodec ());
			codecs.Add (new PALwCodec ());
			codecs.Add (new PALjCodec ());

			codecs.Add (new ACOCodec ());
			codecs.Add (new ASECodec ());
			codecs.Add (bmpCodec);  // Для автоопределения
			codecs.Add (new PCXCodec ());

			// Настройка поля таблицы цветов
			DataGridViewCell cell1 = new DataGridViewTextBoxCell ();
			DataGridViewCell cell2 = new DataGridViewTextBoxCell ();
			cell2.ValueType = Type.GetType ("System.Byte");

			ColorGrid.Columns.Add (new DataGridViewColumn (cell1));
			ColorGrid.Columns[0].Name = RDLocale.GetText ("ColorColumn");
			ColorGrid.Columns.Add (new DataGridViewColumn (cell2));
			ColorGrid.Columns[1].Name = RDLocale.GetText ("AlphaColumn");

			ExitButton.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit);
			Label01.Text = RDLocale.GetText ("OpacityLabel");
			AbortAlpha.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel);
			ApplyAlpha.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK);

			LoadPalette.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Load);
			SavePalette.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Save);
			SetPalette.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Replace);

			// Запуск
			this.ShowDialog ();
			}

		// Закрытие формы
		private void ExitButton_Click (object sender, EventArgs e)
			{
			allowExit = true;
			this.Close ();
			}

		private void PalettesManager_FormClosing (object sender, FormClosingEventArgs e)
			{
			RDGenerics.SaveWindowDimensions (this);
			e.Cancel = !allowExit;
			}

		// Выбор цвета
		private void ColorGrid_CellDoubleClick (object sender, DataGridViewCellEventArgs e)
			{
			// Контроль
			if (e.RowIndex < 0)
				{
				return;
				}

			// Цвет
			if (e.ColumnIndex == 0)
				{
				CDialog.Color = ColorGrid.Rows[e.RowIndex].Cells[0].Style.BackColor;
				if (CDialog.ShowDialog () != DialogResult.OK)
					{
					return;
					}

				ColorGrid.Rows[e.RowIndex].Cells[0].Style.BackColor = CDialog.Color;
				ColorGrid.Rows[e.RowIndex].Cells[0].Value = "(" + CDialog.Color.R.ToString () + "; " +
					CDialog.Color.G.ToString () + "; " + CDialog.Color.B.ToString () + ")";
				if ((CDialog.Color.R + CDialog.Color.G + CDialog.Color.B) > 128 * 3)
					{
					ColorGrid.Rows[e.RowIndex].Cells[0].Style.ForeColor = Color.FromArgb (0, 0, 0);
					}
				else
					{
					ColorGrid.Rows[e.RowIndex].Cells[0].Style.ForeColor = Color.FromArgb (255, 255, 255);
					}
				}
			// Альфа
			else
				{
				AlphaValue.Value = decimal.Parse (ColorGrid.Rows[e.RowIndex].Cells[1].Value.ToString ());
				SetFormState (false);
				AlphaValue.Focus ();
				}
			}

		// Обработка ошибок ввода
		private void ColorGrid_DataError (object sender, DataGridViewDataErrorEventArgs e)
			{
			RDInterface.LocalizedMessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
				"IncorrectAlpha");
			}

		// Добавление цвета
		private void AddColor_Click (object sender, EventArgs e)
			{
			// Выбор цвета
			CDialog.Color = Color.FromArgb (255, 255, 255);
			if (CDialog.ShowDialog () != DialogResult.OK)
				{
				return;
				}

			// Добавление цвета
			ColorGrid.Rows.Add ([ "(" + CDialog.Color.R.ToString () + "; " +
				CDialog.Color.G.ToString () + "; " + CDialog.Color.B.ToString () + ")", "255" ]);
			ColorGrid.Rows[ColorGrid.Rows.Count - 1].Cells[0].Style.BackColor =
				ColorGrid.Rows[ColorGrid.Rows.Count - 1].Cells[0].Style.SelectionBackColor = CDialog.Color;
			if ((CDialog.Color.R + CDialog.Color.G + CDialog.Color.B) > 128 * 3)
				{
				ColorGrid.Rows[ColorGrid.Rows.Count - 1].Cells[0].Style.ForeColor = Color.FromArgb (0, 0, 0);
				}
			else
				{
				ColorGrid.Rows[ColorGrid.Rows.Count - 1].Cells[0].Style.ForeColor = Color.FromArgb (255, 255, 255);
				}

			// Включение кнопки удаления и отображение количества
			if (!DeleteColor.Enabled)
				DeleteColor.Enabled = true;
			ColorsCountLabel.Text = ColorGrid.Rows.Count.ToString ();
			}

		// Удаление цвета
		private void DeleteColor_Click (object sender, EventArgs e)
			{
			if (ColorGrid.SelectedRows.Count > 0)
				{
				ColorGrid.Rows.RemoveAt (ColorGrid.SelectedRows[0].Index);
				if (ColorGrid.Rows.Count == 0)
					{
					DeleteColor.Enabled = false;
					}

				ColorsCountLabel.Text = ColorGrid.Rows.Count.ToString ();
				}
			}

		// Блокировка/разблокировка окна
		private void SetFormState (bool State)
			{
			LoadPalette.Enabled = SavePalette.Enabled = SetPalette.Enabled =
				ExitButton.Enabled = AddColor.Enabled = DeleteColor.Enabled =
				ColorGrid.Enabled = State;

			AlphaPanel.Visible = AlphaValue.Enabled = AbortAlpha.Enabled = ApplyAlpha.Enabled = !State;
			}

		// Работа с альфа-каналом
		private void AbortAlpha_Click (object sender, EventArgs e)
			{
			SetFormState (true);
			}

		private void ApplyAlpha_Click (object sender, EventArgs e)
			{
			if (ColorGrid.SelectedRows.Count > 0)
				ColorGrid.SelectedRows[0].Cells[1].Value = (uint)AlphaValue.Value;

			SetFormState (true);
			}

		private void AlphaValue_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				{
				ApplyAlpha_Click (null, null);
				}
			else if (e.KeyCode == Keys.Escape)
				{
				AbortAlpha_Click (null, null);
				}
			}

		// Вызов изменений с клавиатуры
		private void ColorGrid_KeyDown (object sender, KeyEventArgs e)
			{
			// Контроль
			if (ColorGrid.SelectedRows.Count <= 0)
				return;

			// Перенаправление
			if (e.KeyCode == Keys.F1)
				{
				ColorGrid_CellDoubleClick (null, new DataGridViewCellEventArgs (0, ColorGrid.SelectedRows[0].Index));
				}
			else if (e.KeyCode == Keys.F2)
				{
				ColorGrid_CellDoubleClick (null, new DataGridViewCellEventArgs (1, ColorGrid.SelectedRows[0].Index));
				}
			}

		// Загрузка палитры
		private void LoadPalette_Click (object sender, EventArgs e)
			{
			OFDialog.ShowDialog ();
			}

		private void OFDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Получение палитры
			List<Color> palette;
			string msg = "";

			switch (codecs[OFDialog.FilterIndex - 1].LoadPalette (OFDialog.FileName, out palette))
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

				case ProgramErrorCodes.EXEC_NO_PALETTE_AVAILABLE:
					msg = RDLocale.GetText ("PalettesNotFound");
					break;

				case ProgramErrorCodes.EXEC_UNSUPPORTED_COLORS:
					RDInterface.MessageBox (RDMessageFlags.Warning,
						string.Format (RDLocale.GetText ("FileGeneric"), OFDialog.FileName) +
						RDLocale.GetText ("UnsupportedColors"));

					// Без отмены загрузки (msg не заполняется)
					break;

				case ProgramErrorCodes.EXEC_OK:
					break;

				// Других вариантов быть не должно
				default:
					throw new Exception (RDLocale.GetText ("DebugRequired") + " (2)");
				}

			if (!string.IsNullOrWhiteSpace (msg))
				{
				msg = string.Format (RDLocale.GetText ("FileGeneric"), OFDialog.FileName) + msg;
				RDInterface.MessageBox (RDMessageFlags.Warning, msg);
				return;
				}

			// Загрузка палитры в интерфейс
			ColorGrid.Rows.Clear ();
			for (int i = 0; i < palette.Count; i++)
				{
				ColorGrid.Rows.Add ([ "(" +
					palette[i].R.ToString () + "; " +
					palette[i].G.ToString () + "; " +
					palette[i].B.ToString () + ")",
					palette[i].A.ToString ()
					]);
				ColorGrid.Rows[ColorGrid.Rows.Count - 1].Cells[0].Style.BackColor =
					ColorGrid.Rows[ColorGrid.Rows.Count - 1].Cells[0].Style.SelectionBackColor =
					Color.FromArgb (palette[i].R, palette[i].G, palette[i].B);

				if ((palette[i].R + palette[i].G + palette[i].B) > 128 * 3)
					{
					ColorGrid.Rows[ColorGrid.Rows.Count - 1].Cells[0].Style.ForeColor = Color.FromArgb (0, 0, 0);
					}
				else
					{
					ColorGrid.Rows[ColorGrid.Rows.Count - 1].Cells[0].Style.ForeColor = Color.FromArgb (255, 255, 255);
					}
				if (!DeleteColor.Enabled)
					{
					DeleteColor.Enabled = true;
					}
				}
			ColorsCountLabel.Text = palette.Count.ToString ();
			}

		// Сохранение палитры
		private void SavePalette_Click (object sender, EventArgs e)
			{
			SFDialog.ShowDialog ();
			}

		private void SFDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Контроль
			if ((ColorGrid.Rows.Count == 0) || (ColorGrid.Rows.Count > codecs[SFDialog.FilterIndex - 1].MaxColors))
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					RDLocale.GetText ("TooMuchColors") + codecs[SFDialog.FilterIndex - 1].MaxColors.ToString ());
				return;
				}

			if ((ColorGrid.Rows.Count > 256) &&
				(RDInterface.LocalizedMessageBox (RDMessageFlags.Question | RDMessageFlags.CenterText,
				"ColorsCountExceedsRecommended",
				RDLDefaultTexts.Button_Yes, RDLDefaultTexts.Button_No) != RDMessageButtons.ButtonOne))
				return;

			// Перегонка
			List<Color> palette = [];
			for (int i = 0; i < ColorGrid.Rows.Count; i++)
				{
				palette.Add (Color.FromArgb (int.Parse (ColorGrid.Rows[i].Cells[1].Value.ToString ()),
					ColorGrid.Rows[i].Cells[0].Style.BackColor.R,
					ColorGrid.Rows[i].Cells[0].Style.BackColor.G,
					ColorGrid.Rows[i].Cells[0].Style.BackColor.B));
				}

			// Сохранение
			if (codecs[SFDialog.FilterIndex - 1].SavePalette (SFDialog.FileName, palette) != ProgramErrorCodes.EXEC_OK)
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					SFDialog.FileName));
			}

		// Замена палитры
		private void SetPalette_Click (object sender, EventArgs e)
			{
			CFDialog.ShowDialog ();
			}

		private void CFDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Контроль
			int neededCodec = codecs.IndexOf (bmpCodec);
			if ((ColorGrid.Rows.Count == 0) || (ColorGrid.Rows.Count > codecs[neededCodec].MaxColors))
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					RDLocale.GetText ("TooMuchColors") + codecs[neededCodec].MaxColors.ToString ());
				return;
				}

			// Перегонка
			List<Color> palette = [];
			for (int i = 0; i < ColorGrid.Rows.Count; i++)
				{
				palette.Add (Color.FromArgb (int.Parse (ColorGrid.Rows[i].Cells[1].Value.ToString ()),
					ColorGrid.Rows[i].Cells[0].Style.BackColor.R,
					ColorGrid.Rows[i].Cells[0].Style.BackColor.G,
					ColorGrid.Rows[i].Cells[0].Style.BackColor.B));
				}

			// Сохранение
			if (codecs[neededCodec].SavePalette (CFDialog.FileName, palette) != ProgramErrorCodes.EXEC_OK)
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					SFDialog.FileName));
			}
		}
	}
