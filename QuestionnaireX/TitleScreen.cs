/*
 * Author: Johannes Schirm, MPI for Biological Cybernetics
 * Written on behalf of Betty Mohler.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using FileHelpers;

namespace QuestionnaireX
{
    public partial class TitleScreen : Form
    {
        List<DataTable> experimentInputs = new List<DataTable>();
        ControlPanel controlPanel = new ControlPanel();

        public TitleScreen()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Here, the user has the opportunity to load one or more CSV files containing the questionnaire input.
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            dlg.Filter = "CSV Files (*.csv)|*.csv";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                experimentInputs.Clear();
                string errorMessages = "";
                string loadedFiles = "";
                foreach (string file in dlg.FileNames)
                {
                    try
                    {
                        // Each input is stored as a data table for easier access:
                        experimentInputs.Add(CsvEngine.CsvToDataTable(file, ';'));
                        loadedFiles += Path.GetFileName(file) + "\n";
                    }
                    catch
                    {
                        errorMessages += "Couldn't parse contents of the file located at '" + file + "'.\n";
                    }
                }
                if (errorMessages.Length > 0 && experimentInputs.Count > 0)
                {
                    // At least one file wasn't read in correctly and at least one file was read in correctly, so ask the user what to do:
                    if (MessageBox.Show(errorMessages + "Would you like to use the remaining files anyway?", "Invalid input files", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                    {
                        Console.WriteLine("The user cancelled loading the selected input files because some of them are invalid.");
                        experimentInputs.Clear();
                        button1.BackColor = Color.Tomato;
                    }
                    else
                    {
                        Console.WriteLine("The user decided to use " + experimentInputs.Count + " of " + dlg.FileName.Length + " input files available because the other files are invalid.");
                        button1.BackColor = Color.SandyBrown;
                    }
                }
                else if (experimentInputs.Count == 0)
                {
                    // All of the input files weren't read in correctly, so show an error message to the user:
                    MessageBox.Show("None of the input files was valid. Please make sure they're in the CSV format!", "Invalid input files", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine("All input files are invalid.");
                    button1.BackColor = Color.Tomato;
                }
                else
                {
                    MessageBox.Show("Successfully loaded " + experimentInputs.Count.ToString() + " input files in the following order:\n" + loadedFiles, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button1.BackColor = Color.LightGreen;
                }
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            // Redirect the event if the user clicked on the "START!" label instad on the logo...
            pictureBox1_Click(sender, e);
        }
        
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Start the sequence of questions according to the input file(s) the user loaded beforehand.
            this.Hide();
            // Show the control panel:
            controlPanel.Show();
            controlPanel.StartTimer((int)numericUpDown3.Value, checkBox1.Checked);
            for (int i = 0; i < experimentInputs.Count; i++)
            {
                foreach (DataRow question in experimentInputs[i].Rows)
                {
                    string answer = "";
                    while (answer == "")
                    {
                            try
                            {
                                answer = ShowQuestionForm((Form)Activator.CreateInstance(QuestionsIndex.INDEX[question["Type"] as string], question));
                            }
                            catch
                            {
                                answer = null;
                                MessageBox.Show("Must be handled by a programmer:\nThe question form with type " + question["Type"].ToString() + " couldn't be instantiated!\nPlease make sure you've added your new question form to the questions index and read the comments in that class file.");
                            }
                        }
                        if (answer == null)
                        {
                            // The last answer was null, which means that the experimenter aborted the questionnaire.
                            this.Close();
                            return;
                        }
                        File.AppendAllLines("../../../" + numericUpDown1.Value + ".txt", new string[] { numericUpDown1.Value + "\t" + numericUpDown2.Value + "\t" + (radioButton2.Checked ? "F" : "M") + "\t" + "Exp" + (i + 1).ToString() + "\t" + (question["ID"] as string) + "\t" + answer });
                }
            }
            controlPanel.Close();
            this.Close();
        }

        /// <summary>
        /// Shows an already instantiated form and returns if the answer was given successfully.
        /// </summary>
        /// <param name="questionForm">The main parameter to pass an already instantiated form (with your parameters) that should be shown.</param>
        /// <returns>Returns whether the question was answered successfully.</returns>
        private string ShowQuestionForm(Form questionForm)
        {
            string result = "";
            Console.WriteLine("Currently showing question of type " + questionForm.GetType().ToString());
            controlPanel.SetCurrentQuestionForm(questionForm);
            questionForm.ShowDialog();
            if (questionForm.DialogResult == DialogResult.OK)
            {
                Console.WriteLine("Retrieved answer from question form: '" + questionForm.ToString() + "'");
                result = questionForm.ToString();
            }
            else if (questionForm.DialogResult == DialogResult.Abort)
            {
                return null;
            }
            return result;
        }
    }
}
