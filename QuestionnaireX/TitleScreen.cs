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
        string lastLoadedInputDirectory;

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
                lastLoadedInputDirectory = Path.GetDirectoryName(dlg.FileNames[0]);
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
            // Write the header of the output file:
            File.AppendAllText("../../../" + numericUpDown1.Value + ".txt", "pID\tpAge\tpGender\tqID\tqFile\tqBlock\tqSBlock\tqAnswer");
            // Start the sequence of questions according to the input file(s) the user loaded beforehand.
            this.Hide();
            // Show the control panel:
            controlPanel.Show();
            controlPanel.SetTimer((int)numericUpDown3.Value);
            // Iterate through all files the experimenter selected
            for (int i = 0; i < experimentInputs.Count; i++)
            {
                DataTable currentFile = experimentInputs[i];
                PrepareDataTable(ref currentFile);
                for (int row = 0; row < experimentInputs[i].Rows.Count; row++)
                {
                    DataRow question = experimentInputs[i].Rows[row];
                    // If randomization of sub-blocks is enabled, detect the start of a new sub-block and randomize it!
                    if (checkBox1.Checked)
                    {
                        int totalRowsAmount = experimentInputs[i].Rows.Count;
                        string thisSubBlock = question["Sub_Block_Number"] as string;
                        Func<int, string, string> getFieldOfRow = delegate(int rowIndex, string columnName)
                        {
                            return experimentInputs[i].Rows[rowIndex][columnName] as string;
                        };
                        if (row == 0 || !getFieldOfRow(row - 1, "Sub_Block_Number").Equals(thisSubBlock))
                        {
                            // The current row is either the first row of the file or the first row of a new sub-block. Randomize it!
                            DataRow[] subBlock = experimentInputs[i].Select("Sub_Block_Number = '" + getFieldOfRow(row + 1, "Sub_Block_Number") + "' AND Block_Number = '" + getFieldOfRow(row + 1, "Block_Number") + "'");
                            ShuffleDataRows(ref subBlock);
                        }
                    }
                    // Display the current question to the participant:
                    string answer = "";
                    while (answer == "")
                    {
                        try
                        {
                            // Update the experiment status in the control panel:
                            controlPanel.UpdateQuestionID(question["ID"] as string);
                            controlPanel.UpdateBlock(question["Block_Number"] as string);
                            controlPanel.UpdateSubBlock(question["Sub_Block_Number"] as string);
                            controlPanel.UpdateSubBlockType(question["Sub_Block_Type"] as string);
                            // If there are any references in this line of the input file marked with "file:", replace them by the respective file contents.
                            for (int j = 0; j < question.ItemArray.Length; j++)
                            {
                                string dataCell = question.ItemArray[j] as string;
                                if (dataCell.StartsWith("file:"))
                                {
                                    // The actual question was swapped out to a file, so try to read it:
                                    question[j] = System.IO.File.ReadAllText(lastLoadedInputDirectory + Path.DirectorySeparatorChar + dataCell.Substring(5));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            answer = null;
                            MessageBox.Show("Something went wrong while collecting all data from the input file!\n\nException message:\n" + ex.Message, "Problem with input file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        try
                        {
                            // Show the next question form:
                            answer = ShowQuestionForm((Form)Activator.CreateInstance(QuestionsIndex.INDEX[question["Type"] as string], question));
                        }
                        catch (Exception ex)
                        {
                            answer = null;
                            MessageBox.Show("Must be handled by a programmer:\nThe question form with type " + question["Type"].ToString() + " couldn't be instantiated!\nPlease make sure you've added your new question form to the questions index and read the comments in that class file.\n\nException message:\n" + ex.Message, "Problem with question type", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    if (answer == null)
                    {
                        // The last answer was null, which means that the experimenter aborted the questionnaire.
                        this.Close();
                        return;
                    }
                    File.AppendAllText("../../../" + numericUpDown1.Value + ".txt", "\n" + numericUpDown1.Value + "\t" + numericUpDown2.Value + "\t" + (radioButton2.Checked ? "F" : "M") + "\t" + (question["ID"] as string) + "\t" + (i + 1).ToString() + "\t" + (question["Block_Number"] as string).Replace('\n', ' ') + "\t" + (question["Sub_Block_Number"] as string).Replace('\n', ' ') + "\t" + answer);
                }
            }
            controlPanel.Close();
            this.Close();
        }

        /// <summary>
        /// This helper function cleans up a data table so it can be used to show the questionnaire.
        /// It removes empty lines or lines without the field "ID" and removes the read-only flag for all colums.
        /// </summary>
        /// <param name="table">The data table you want to prepare.</param>
        private void PrepareDataTable(ref DataTable table)
        {
            foreach (DataColumn col in table.Columns)
            {
                // This just sucks. All columns seem to be read-only from the start, but we want to modify them!
                col.ReadOnly = false;
            }
            // Delete data rows without an id! (This means they're empty or invalid...)
            List<int> deleteRowIndices = new List<int>();
            foreach (DataRow r in table.Rows)
            {
                if (r["ID"].Equals(""))
                {
                    deleteRowIndices.Add(table.Rows.IndexOf(r));
                }
            }
            // To not shift the indices, sort the list of indices descending. That way, we'll remove the rows from the bottom to the top!
            deleteRowIndices.Sort((a, b) => -1 * a.CompareTo(b));
            foreach (int rIndex in deleteRowIndices)
            {
                table.Rows[rIndex].Delete();
            }
        }

        /// <summary>
        /// This helper function can randomly shuffle an array of data rows.
        /// </summary>
        /// <param name="rows">A reference to an array containing data rows.</param>
        private void ShuffleDataRows(ref DataRow[] rows)
        {
            // Run the Fisherman-Yates-Shuffle on the range we identified as one sub-block:
            Random rand = new Random(DateTime.Now.Millisecond);
            for (int j = rows.Length - 1; j > 1; j--)
            {
                int k = rand.Next(0, j);
                if (k == j)
                {
                    continue;
                }
                else
                {
                    object[] tmp = rows[j].ItemArray;
                    rows[j].ItemArray = rows[k].ItemArray;
                    rows[k].ItemArray = tmp;
                }
            }
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
