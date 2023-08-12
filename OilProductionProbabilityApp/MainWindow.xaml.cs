using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace BayesTheoremCalculator
{
    public partial class MainWindow : Window
    {
        public string filePath;
        private SQLiteConnection dbConnection;

        public MainWindow()
        {
            InitializeComponent();
            // Create or open the SQLite database file
            string dbFilePath = "OutputDatabase.db";
            dbConnection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;");
            dbConnection.Open();

            // Create a table if it doesn't exist
            string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS OutputData (
                Id INTEGER PRIMARY KEY,
                Timestamp TEXT,
                StateOfNature TEXT,
                OriginalRisk REAL,
                ConditionalProbability REAL,
                JointProbabilities REAL,
                RevisedRisk REAL
            );
        ";
            using (SQLiteCommand createTableCommand = new SQLiteCommand(createTableQuery, dbConnection))
            {
                createTableCommand.ExecuteNonQuery();
            }
        }
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get user input values
                double pE1 = double.Parse(ProbabilityE1TextBox.Text);
                double pE2 = double.Parse(ProbabilityE2TextBox.Text);
                double pBGivenE1 = double.Parse(ProbabilityBGivenE1TextBox.Text);
                double pBGivenE2 = double.Parse(ProbabilityBGivenE2TextBox.Text);

                // Calculate probabilities using Bayes' Theorem
                double pE1GivenBNot = (pBGivenE1 * pE1) / ((pBGivenE1 * pE1) + (pBGivenE2 * pE2));
                double pE2GivenBNot = (pBGivenE2 * pE2) / ((pBGivenE1 * pE1) + (pBGivenE2 * pE2));

                // Create a DataTable to hold the output
                var dataTable = new System.Data.DataTable();
                dataTable.Columns.Add("State of nature");
                dataTable.Columns.Add("Original risk (probabilities)\nP(Ei)");
                dataTable.Columns.Add("Conditional Probability by interpretation\nP(B'|Ei)");
                dataTable.Columns.Add("Joint Probabilities \nP(B'|Ei) P(Ei)");
                dataTable.Columns.Add("Revised Risk Estimates\nP(Ei|B')");
                dataTable.Rows.Add(" i=1: no reservoir", pE1.ToString("0.0"), pBGivenE1.ToString("0.0"), (pBGivenE1 * pE1).ToString("0.00"), pE1GivenBNot.ToString("0.000"));
                dataTable.Rows.Add(" i=2: average reservoir", pE2.ToString("0.0"), pBGivenE2.ToString("0.0"), (pBGivenE2 * pE2).ToString("0.00"), pE2GivenBNot.ToString("0.000"));
                dataTable.Rows.Add(" Totals", (pE1 + pE2).ToString("0.0"), (pBGivenE1+pBGivenE2).ToString("0.0"), (pBGivenE1 * pE1 + pBGivenE2 * pE2).ToString("0.00"), "");

                // Display the DataTable in the DataGrid
                OutputDataGrid.ItemsSource = dataTable.DefaultView;
                OutputDataGrid.Visibility = Visibility.Visible;
                this.Height = 600;
             
                // Insert records into the SQLite database
                using (SQLiteCommand insertCommand = new SQLiteCommand("INSERT INTO OutputData (Timestamp, StateOfNature, OriginalRisk, ConditionalProbability, JointProbabilities, RevisedRisk) VALUES (@Timestamp, @StateOfNature, @OriginalRisk, @ConditionalProbability, @JointProbabilities, @RevisedRisk);", dbConnection))
                {
                    insertCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString());
                    insertCommand.Parameters.AddWithValue("@StateOfNature", "i=1: no reservoir");
                    insertCommand.Parameters.AddWithValue("@OriginalRisk", pE1);
                    insertCommand.Parameters.AddWithValue("@ConditionalProbability", pBGivenE1);
                    insertCommand.Parameters.AddWithValue("@JointProbabilities", pBGivenE1 * pE1);
                    insertCommand.Parameters.AddWithValue("@RevisedRisk", pE1GivenBNot);
                    insertCommand.ExecuteNonQuery();
                    //ExportToCSV("output.csv");
                }

                string csvFilePath = ExportToCSV("output.csv");

                var outputDialog = new OutputDialog();
                outputDialog.SetMessageAndFilePath("Calculations Completed, Results are updated in Database and stored in 'output.csv' file.", csvFilePath);
                outputDialog.ShowDialog();




                //MessageBox.Show("Calculation completed. Results are updated in Database and stored in 'output.cvs file'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Invalid input. Please enter valid numeric values.");
            }
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numeric input (digits and decimal point)
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true; // Prevent non-numeric input
            }

            // Prevent multiple decimal points
            if (e.Text == "." && ((TextBox)sender).Text.Contains("."))
            {
                e.Handled = true; // Prevent multiple decimal points
            }
        }
        //exporting database to cvs function
        private string ExportToCSV(string fileName)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(folderPath, fileName);

            using (SQLiteCommand selectCommand = new SQLiteCommand("SELECT * FROM OutputData;", dbConnection))
            using (SQLiteDataReader reader = selectCommand.ExecuteReader())
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write the header
                string header = string.Join(",", reader.GetColumnSchema().Select(col => col.ColumnName));
                writer.WriteLine(header);

                // Write the data
                while (reader.Read())
                {
                    string[] values = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        values[i] = reader[i].ToString();
                    }
                    string line = string.Join(",", values);
                    writer.WriteLine(line);
                }
                return filePath;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            dbConnection.Close();
        }





    }
}
