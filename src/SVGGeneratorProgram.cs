using System;
using System.Windows.Forms;

namespace RD_AAOW
	{
	static class SVGGeneratorProgram
		{
		/// <summary>
		/// Главная точка входа для приложения.
		/// </summary>
		[STAThread]
		static void Main (string[] args)
			{
			// Инициализация
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			// Отображение справки и запроса на принятие Политики
			if (!ProgramDescription.AcceptEULA ())
				return;
			ProgramDescription.ShowAbout (true);

			// Контроль параметров запуска
			SVGGeneratorForm mainForm = null;

			// Контроль аргументов
			if (args.Length >= 1)
				{
				// Запрос справки
				if ((args[0] == "/?") || (args[0] == "-?"))
					{
					MessageBox.Show (Localization.GetText ("CommandLineInfo", Localization.CurrentLanguage),
						ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
					}

				// Выбор варианта
				string sourcePath = args[0],
					destPath = args[0] + ".svg",
					format = "svg";

				if (args.Length > 1)
					destPath = args[1];
				if (args.Length > 2)
					format = args[2];

				mainForm = new SVGGeneratorForm (sourcePath, destPath, format);
				}
			else
				{
				mainForm = new SVGGeneratorForm ();
				}
			}
		}
	}
