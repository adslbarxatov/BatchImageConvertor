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
	public partial class PalettesManager:Form
		{
		// Переменные
		private List<IPaletteCodec> codecs = new List<IPaletteCodec> ();
		private string[] filters = new string[] {
			"Adobe Color Table (*.act)|*.act",
			"Microsoft palette (*.pal)|*.pal",
			"JASC palette (*.pal)|*.pal",

			"Adobe Color Swatches (*.aco)|*.aco",
			"Adobe Swatches Exchange (*.ase)|*.ase",
			"Windows bitmap (*.bmp, *.dib, *.rle)|*.bmp;*.dib;*.rle",
			"ZSoft Paintbrush (*.pcx, *.pcc)|*.pcx;*.pcc"
			};
		private bool allowExit = false;
		private BMPCodec bmpCodec = new BMPCodec ();
		private SupportedLanguages al;

		/// <summary>
		/// Конструктор. Запускает форму
		/// </summary>
		/// <param name="InterfaceLanguage">Язык интерфейса</param>
		public PalettesManager (SupportedLanguages InterfaceLanguage)
			{
			InitializeComponent ();
			al = InterfaceLanguage;

			// Настройка контролов
			this.Text = Localization.GetText ("PalettesManager", al);

			OFDialog.Title = Localization.GetText ("OpenPaletteDialogTitle", al);
			SFDialog.Title = Localization.GetText ("SavePaletteDialogTitle", al);
			CFDialog.Title = Localization.GetText ("ChangePaletteDialogTitle", al);

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
			ColorGrid.Columns[0].Name = Localization.GetText ("ColorColumn", al);
			ColorGrid.Columns.Add (new DataGridViewColumn (cell2));
			ColorGrid.Columns[1].Name = Localization.GetText ("AlphaColumn", al);

			ExitButton.Text = Localization.GetText ("BExit", al);
			Label01.Text = Localization.GetText ("OpacityLabel", al);
			AbortAlpha.Text = Localization.GetText ("BCancel", al);

			LoadPalette.Text = Localization.GetText ("BLoad", al);
			SavePalette.Text = Localization.GetText ("BSave", al);
			SetPalette.Text = Localization.GetText ("BReplace", al);

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
			MessageBox.Show (Localization.GetText ("IncorrectAlpha", al),
				ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
			ColorGrid.Rows.Add (new object[] { "(" + CDialog.Color.R.ToString () + "; " + CDialog.Color.G.ToString () + "; " +
					CDialog.Color.B.ToString () + ")", "255" });
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
					msg = Localization.GetText ("FileUnavailable", al);
					break;

				case ProgramErrorCodes.EXEC_INVALID_FILE:
					msg = Localization.GetText ("FileIncorrect", al);
					break;

				case ProgramErrorCodes.EXEC_MEMORY_ALLOC_FAIL:
					msg = Localization.GetText ("NotEnoughMemory", al);
					break;

				case ProgramErrorCodes.EXEC_NO_PALETTE_AVAILABLE:
					msg = Localization.GetText ("PalettesNotFound", al);
					break;

				case ProgramErrorCodes.EXEC_UNSUPPORTED_COLORS:
					MessageBox.Show (string.Format (Localization.GetText ("FileGeneric", al), OFDialog.FileName) +
						Localization.GetText ("UnsupportedColors", al),
						ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					// Без отмены загрузки (msg не заполняется)
					break;

				case ProgramErrorCodes.EXEC_OK:
					break;

				// Других вариантов быть не должно
				default:
					throw new Exception (Localization.GetText ("DebugRequired", al) + " (2)");
				}

			if (msg != "")
				{
				msg = string.Format (Localization.GetText ("FileGeneric", al), OFDialog.FileName) + msg;
				MessageBox.Show (msg, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Загрузка палитры в интерфейс
			ColorGrid.Rows.Clear ();
			for (int i = 0; i < palette.Count; i++)
				{
				ColorGrid.Rows.Add (new object[] { "(" + palette[i].R.ToString () + "; " + palette[i].G.ToString () + "; " +
					palette[i].B.ToString () + ")", palette[i].A.ToString () });
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
				MessageBox.Show (Localization.GetText ("TooMuchColors", al) +
					codecs[SFDialog.FilterIndex - 1].MaxColors.ToString (), ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			if ((ColorGrid.Rows.Count > 256) &&
				(MessageBox.Show (Localization.GetText ("ColorsCountExceedsRecommended", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes))
				{
				return;
				}

			// Перегонка
			List<Color> palette = new List<Color> ();
			for (int i = 0; i < ColorGrid.Rows.Count; i++)
				{
				palette.Add (Color.FromArgb (int.Parse (ColorGrid.Rows[i].Cells[1].Value.ToString ()),
					ColorGrid.Rows[i].Cells[0].Style.BackColor.R,
					ColorGrid.Rows[i].Cells[0].Style.BackColor.G,
					ColorGrid.Rows[i].Cells[0].Style.BackColor.B));
				}

			// Сохранение
			if (codecs[SFDialog.FilterIndex - 1].SavePalette (SFDialog.FileName, palette) != ProgramErrorCodes.EXEC_OK)
				{
				MessageBox.Show (Localization.GetText ("OutputPathUnavailable", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
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
				MessageBox.Show (Localization.GetText ("TooMuchColors", al) +
					codecs[neededCodec].MaxColors.ToString (), ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Перегонка
			List<Color> palette = new List<Color> ();
			for (int i = 0; i < ColorGrid.Rows.Count; i++)
				{
				palette.Add (Color.FromArgb (int.Parse (ColorGrid.Rows[i].Cells[1].Value.ToString ()),
					ColorGrid.Rows[i].Cells[0].Style.BackColor.R,
					ColorGrid.Rows[i].Cells[0].Style.BackColor.G,
					ColorGrid.Rows[i].Cells[0].Style.BackColor.B));
				}

			// Сохранение
			if (codecs[neededCodec].SavePalette (CFDialog.FileName, palette) != ProgramErrorCodes.EXEC_OK)
				{
				MessageBox.Show (Localization.GetText ("OutputPathUnavailable", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}
	}
