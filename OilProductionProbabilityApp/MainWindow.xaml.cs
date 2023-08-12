using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BayesTheoremCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
                // Store the output in a file
                string filePath = "output.txt";
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (System.Data.DataRow row in dataTable.Rows)
                    {
                        string line = string.Join("\t", row.ItemArray);
                        writer.WriteLine(line);
                    }
                }

                MessageBox.Show("Calculation completed. Output stored in 'output.txt'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Invalid input. Please enter valid numeric values.");
            }
        }




    }
}
