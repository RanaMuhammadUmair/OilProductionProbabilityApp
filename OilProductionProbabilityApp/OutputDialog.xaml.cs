using System.Diagnostics;
using System.Windows;

namespace BayesTheoremCalculator
{
	public partial class OutputDialog : Window
	{
		public OutputDialog()
		{
			InitializeComponent();
		}

		public void SetMessageAndFilePath(string message, string filePath)
		{
			MessageTextBlock.Text = message;
			CsvFilePathLabel.Text = filePath;
		}

		private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
		{
			if (CsvFilePathLabel.Text != null && CsvFilePathLabel.Text is string filePath)
			{
				// Open the folder containing the CSV file
				string folderPath = System.IO.Path.GetDirectoryName(filePath);

				// Open the folder with full access
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = "explorer.exe",
					Arguments = folderPath,
					Verb = "open"
				};
				Process.Start(psi);
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
