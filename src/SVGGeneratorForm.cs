using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму программы
	/// </summary>
	public partial class SVGGeneratorForm:Form
		{
		// Переменные
		private bool showSuccessMessage = true;
		private SupportedLanguages al = Localization.CurrentLanguage;

		/// <summary>
		/// Конструктор. Запускает главную форму программы
		/// </summary>
		public SVGGeneratorForm ()
			{
			InitializeComponent ();

			// Настройка контролов
			this.Text = ProgramDescription.AssemblyTitle;

			LanguageCombo.Items.AddRange (Localization.LanguagesNames);
			try
				{
				LanguageCombo.SelectedIndex = (int)al;
				}
			catch
				{
				LanguageCombo.SelectedIndex = 0;
				}

			// Запуск
			this.ShowDialog ();
			}

		/// <summary>
		/// Конструктор. Выполняет генерацию изображения в тихом режиме
		/// </summary>
		/// <param name="SourcePath">Имя файла сценария</param>
		/// <param name="DestinationPath">Имя файла изображения</param>
		/// <param name="Format">Формат фалйа изображения</param>
		public SVGGeneratorForm (string SourcePath, string DestinationPath, string Format)
			{
			InitializeComponent ();

			// Настройка окна под тихое выполнение
			OFName.Text = SourcePath;
			SFName.Text = DestinationPath;

			LanguageCombo.Items.AddRange (Localization.LanguagesNames);
			try
				{
				LanguageCombo.SelectedIndex = (int)al;
				}
			catch
				{
				LanguageCombo.SelectedIndex = 0;
				}

			switch (Format.ToLower ())
				{
				case "emf":
					SFDialog.FilterIndex = 2;
					break;

				default:
					SFDialog.FilterIndex = 1;
					break;
				}

			showSuccessMessage = false;
			GenerateImage.Enabled = BExit.Enabled = OFSelect.Enabled = SFSelect.Enabled =
				SaveSample.Enabled = LanguageCombo.Enabled = false;

			// Выполнение
			this.Show ();
			this.Visible = false;
			GenerateImage_Click (null, null);
			this.Close ();
			}

		// Выбор входного файла
		private void OFSelect_Click (object sender, EventArgs e)
			{
			OFDialog.ShowDialog ();
			}

		private void OFDialog_FileOk (object sender, CancelEventArgs e)
			{
			OFName.Text = OFDialog.FileName;
			}

		// Выбор выходного файла
		private void SFSelect_Click (object sender, EventArgs e)
			{
			SFDialog.ShowDialog ();
			}

		private void SFDialog_FileOk (object sender, CancelEventArgs e)
			{
			SFName.Text = SFDialog.FileName;
			}

		// Выход
		private void BExit_Click (object sender, System.EventArgs e)
			{
			this.Close ();
			}

		// Генерация
		private void GenerateImage_Click (object sender, EventArgs e)
			{
			// Контроль параметров
			if (OFName.Text == "")
				{
				MessageBox.Show (Localization.GetText ("InputFileNotSpecified", al), ProgramDescription.AssemblyTitle,
					 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			if (SFName.Text == "")
				{
				MessageBox.Show (Localization.GetText ("OutputFileNotSpecified", al), ProgramDescription.AssemblyTitle,
					 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Чтение файла сценария
			SVGScriptReader svgsr = new SVGScriptReader (OFName.Text);
			if (svgsr.InitResult != SVGScriptReader.InitResults.Ok)
				{
				string msg = Localization.GetText ("ScriptReadingError", al);

				switch (svgsr.InitResult)
					{
					case SVGScriptReader.InitResults.BrokenLineColor:
					case SVGScriptReader.InitResults.BrokenLinePoint:
					case SVGScriptReader.InitResults.BrokenLineWidth:

					case SVGScriptReader.InitResults.BrokenOxColor:
					case SVGScriptReader.InitResults.BrokenOxNotch:
					case SVGScriptReader.InitResults.BrokenOxOffset:
					case SVGScriptReader.InitResults.BrokenOxWidth:

					case SVGScriptReader.InitResults.BrokenOyColor:
					case SVGScriptReader.InitResults.BrokenOyNotch:
					case SVGScriptReader.InitResults.BrokenOyOffset:
					case SVGScriptReader.InitResults.BrokenOyWidth:

					case SVGScriptReader.InitResults.BrokenText:
						msg += (string.Format (Localization.GetText (svgsr.InitResult.ToString (), al), svgsr.CurrentLine));
						break;

					case SVGScriptReader.InitResults.CannotCreateTMP:
					case SVGScriptReader.InitResults.FileNotAvailable:
					case SVGScriptReader.InitResults.IncludeDeepOverflow:
						msg += Localization.GetText (svgsr.InitResult.ToString (), al);
						break;

					case SVGScriptReader.InitResults.CannotIncludeFile:
						msg += (string.Format (Localization.GetText (svgsr.InitResult.ToString (), al), svgsr.FaliedIncludeFile));
						break;

					default:	// f.e., NotInited
						throw new Exception (Localization.GetText ("ExceptionMessage", al) + " (1)");
					}

				MessageBox.Show (msg, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Запись файла SVG
			IVectorAdapter vectorAdapter;
			switch (SFDialog.FilterIndex)
				{
				// EMF
				case 2:
					vectorAdapter = new EMFAdapter (SFName.Text, (uint)(svgsr.MaxX - svgsr.MinX), (uint)(svgsr.MaxY - svgsr.MinY));
					break;

				// SVG
				default:
					vectorAdapter = new SVGAdapter (SFName.Text, (uint)(svgsr.MaxX - svgsr.MinX), (uint)(svgsr.MaxY - svgsr.MinY));
					break;
				}
			if (vectorAdapter.InitResult != VectorAdapterInitResults.Opened)
				{
				string msg = Localization.GetText ("FileWritingError", al);

				switch (vectorAdapter.InitResult)
					{
					case VectorAdapterInitResults.CannotCreateFile:
					case VectorAdapterInitResults.IncorrectImageSize:
						msg += Localization.GetText (vectorAdapter.InitResult.ToString (), al);
						break;

					default:	// f.e., Closed, NotInited
						throw new Exception (Localization.GetText ("ExceptionMessage", al) + " (4)");
					}

				MessageBox.Show (msg, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Кривые
			for (int i = 0; i < svgsr.LinesX.Count; i++)
				{
				vectorAdapter.OpenGroup ();

				for (int j = 0; j < svgsr.LinesX[i].Count - 1; j++)
					{
					vectorAdapter.DrawLine (svgsr.LinesX[i][j], svgsr.LinesY[i][j], svgsr.LinesX[i][j + 1], svgsr.LinesY[i][j + 1],
						svgsr.LinesWidths[i], svgsr.LinesColors[i]);
					}

				vectorAdapter.CloseGroup ();
				}

			// Ось Ox
			if (svgsr.DrawOx)
				{
				// Группировка
				vectorAdapter.OpenGroup ();

				// Ось
				vectorAdapter.DrawLine (svgsr.MinX, svgsr.OxOffset, svgsr.MaxX, svgsr.OxOffset, svgsr.OxWidth, svgsr.OxColor);

				// Засечки
				for (int i = 0; i < svgsr.OxNotchesOffsets.Count; i++)
					{
					vectorAdapter.DrawLine (svgsr.OxNotchesOffsets[i], svgsr.OxOffset - svgsr.OxNotchesSizes[i] / 2.0,
						svgsr.OxNotchesOffsets[i], svgsr.OxOffset + svgsr.OxNotchesSizes[i] / 2.0, svgsr.OxWidth, svgsr.OxColor);
					}

				// Завершение
				vectorAdapter.CloseGroup ();
				}

			// Ось Oy
			if (svgsr.DrawOy)
				{
				// Группировка
				vectorAdapter.OpenGroup ();

				// Ось
				vectorAdapter.DrawLine (svgsr.OyOffset, svgsr.MinY, svgsr.OyOffset, svgsr.MaxY, svgsr.OyWidth, svgsr.OyColor);

				// Засечки
				for (int i = 0; i < svgsr.OyNotchesOffsets.Count; i++)
					{
					vectorAdapter.DrawLine (svgsr.OyOffset - svgsr.OyNotchesSizes[i] / 2.0f, svgsr.OyNotchesOffsets[i],
						svgsr.OyOffset + svgsr.OyNotchesSizes[i] / 2.0f, svgsr.OyNotchesOffsets[i], svgsr.OyWidth, svgsr.OyColor);
					}

				// Завершение
				vectorAdapter.CloseGroup ();
				}

			// Текстовые подписи
			for (int i = 0; i < svgsr.Texts.Count; i++)
				{
				vectorAdapter.DrawText (svgsr.TextX[i], svgsr.TextY[i], svgsr.Texts[i], svgsr.TextFonts[i], svgsr.TextColors[i]);
				}

			// Исходный скрипт в качестве комментария
			vectorAdapter.AddComment (svgsr.SourceScript);

			// Завершено
			vectorAdapter.CloseFile ();

			if (showSuccessMessage)
				{
				MessageBox.Show (Localization.GetText ("FileCreated", al), ProgramDescription.AssemblyTitle,
					 MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}

		// Сохранение образца сценария
		private void SaveSample_Click (object sender, EventArgs e)
			{
			SSDialog.ShowDialog ();
			}

		private void SSDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Попытка создания файла образца
			FileStream FS = null;
			try
				{
				FS = new FileStream (SSDialog.FileName, FileMode.Create);
				}
			catch
				{
				MessageBox.Show (Localization.GetText ("CannotCreateSample", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			if (al == SupportedLanguages.ru_ru)
				FS.Write (RD_AAOW.Properties.SVGGenerator.Sample_ru_ru, 0, RD_AAOW.Properties.SVGGenerator.Sample_ru_ru.Length);
			else
				FS.Write (RD_AAOW.Properties.SVGGenerator.Sample_en_us, 0, RD_AAOW.Properties.SVGGenerator.Sample_en_us.Length);

			FS.Close ();
			}

		// Отображение справки
		private void MainForm_HelpButtonClicked (object sender, CancelEventArgs e)
			{
			// Отмена обработки события вызова справки
			e.Cancel = true;

			// Отображение
			AboutForm af = new AboutForm (al, "*", "*",
				"",

				"This tool allows you to generate vector image (SVG or EMF) using script file with adjustable " +
				"parameters of curves, axes and text labels. This application is the side product of Geomag " +
				"data drawer project. For more info save example file from the utility and open it as text\r\n\r\n" +

				"Этот инструмент позволяет генерировать векторное изображение (SVG или EMF), используя файл сценария " +
				"с настраиваемыми параметрами кривых, осей и текстовых меток. Это приложение является побочным продуктом " +
				"проекта Geomag data drawer. Для получения дополнительной информации сохраните файл образца из утилиты и " +
				"откройте его в текстовом представлении");
			}

		// Выбор языка интерфейса
		private void LanguageCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			// Сохранение
			Localization.CurrentLanguage = al = (SupportedLanguages)LanguageCombo.SelectedIndex;

			// Локализация
			OFDialog.Title = OFLabel.Text = Localization.GetText ("OFDialogTitle", al);
			OFDialog.Filter = SSDialog.Filter = Localization.GetText ("OFDialogFilter", al);

			SFDialog.Title = SFLabel.Text = Localization.GetText ("SFDialogTitle", al);
			SFDialog.Filter = Localization.GetText ("SFDialogFilter", al);

			SSDialog.Title = Localization.GetText ("SSDialogTitle", al);

			SaveSample.Text = Localization.GetText ("SaveSampleText", al);
			GenerateImage.Text = Localization.GetText ("GenerateImageText", al);
			BExit.Text = Localization.GetText ("BExitText", al);
			}
		}
	}
