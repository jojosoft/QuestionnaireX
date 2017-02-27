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
        private DataTable currentExperimentInput;
        private string lastLoadedInputDirectory;
        private ControlPanel controlPanel = new ControlPanel();

        public TitleScreen()
        {
            InitializeComponent();
            // If an configuration file exists, try to load it:
            if (Directory.Exists(dataFolderPath) && File.Exists(configFilePath))
            {
                try
                {
                    bool[] configuration = File.ReadAllLines(configFilePath).Select(str => str.ToLower().Equals("true")).ToArray();
                    List<CheckBox> checkBoxes = GetAllCheckboxes();
                    if (configuration.Length != checkBoxes.Count)
                    {
                        // This indicates that the configuration file might be out of date, so don't use it.
                        if (File.Exists(configFilePath + ".old"))
                        {
                            File.Delete(configFilePath + ".old");
                        }
                        File.Move(configFilePath, configFilePath + ".old");
                        File.AppendAllText(dataFolderPath + Path.DirectorySeparatorChar + "log.txt", DateTime.Now.ToString() + ": Tried to read config file. But for " + checkBoxes.Count + " checkboxes, there have been found " + configuration.Length + " values. This indicates that the config file is out of date or has been modified, so it got renamed.\r\n");
                    }
                    else
                    {
                        // Load the last configuration:
                        for (int i = 0; i < configuration.Length; i++)
                        {
                            checkBoxes[i].Checked = configuration[i];
                        }
                    }
                }
                catch { }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Here, the user has the opportunity to load one CSV file containing the questionnaire input.
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            dlg.Filter = "CSV Files (*.csv)|*.csv";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Try to read from the given input file:
                    currentExperimentInput = CsvEngine.CsvToDataTable(dlg.FileName, ';');
                    lastLoadedInputDirectory = Path.GetDirectoryName(dlg.FileName);
                    MessageBox.Show("Successfully loaded the input file named:\n" + Path.GetFileName(dlg.FileName), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button1.BackColor = Color.LightGreen;
                }
                catch
                {
                    MessageBox.Show("Couldn't parse contents of the file located at '" + dlg.FileName + "'.", "Invalid input files", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button1.BackColor = Color.Tomato;
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
            if (lastLoadedInputDirectory == null)
            {
                MessageBox.Show("Please load an input file before starting the questionnaire!", "No file loaded", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            // Store the current configuration in the config file:
            if (!Directory.Exists(dataFolderPath))
            {
                Directory.CreateDirectory(dataFolderPath);
            }
            else if (File.Exists(configFilePath))
            {
                File.Delete(configFilePath);
            }
            List<CheckBox> checkBoxes = GetAllCheckboxes();
            for (int i = 0; i < checkBoxes.Count; i++)
            {
                File.AppendAllLines(configFilePath, new string[] { checkBoxes[i].Checked.ToString() });
            }
            // Write the header of the output file:
            Directory.CreateDirectory("Output");
            File.AppendAllText("./Output/" + numericUpDown1.Value + ".txt", "pID\tpAge\tpGender\tqID\tqBlock\tqSBlock\tqSBlType\tqAnswer\ttimestampUTC\tanswerTimeMs");
            // Start the sequence of questions according to the input file(s) the user loaded beforehand.
            this.Hide();
            // If requested, cover the background with a black form:
            if (checkBox5.Checked)
            {
                Form background = new Form();
                background.BackColor = Color.Black;
                background.FormBorderStyle = FormBorderStyle.None;
                background.TopMost = true;
                background.Top = 0;
                background.Left = 0;
                background.Width = Screen.PrimaryScreen.Bounds.Width;
                background.Height = Screen.PrimaryScreen.Bounds.Height;
                background.Show();
                background.SendToBack();
            }
            // Show the control panel:
            controlPanel.Show();
            // Iterate through all files the experimenter selected
            PrepareDataTable(ref currentExperimentInput);
            for (int row = 0; row < currentExperimentInput.Rows.Count; row++)
            {
                DataRow question = currentExperimentInput.Rows[row];
                // Trim all data cells to get rid of any whitespaces before or after the value itself:
                for (int i = 0; i < question.ItemArray.Length; i++)
                {
                    question[i] = (question[i] as string).Trim();
                }
                // If randomization of sub-blocks is enabled, detect the start of a new sub-block and randomize it!
                bool newSubBlock = row == 0 || !GetFieldOfRow(row - 1, "Sub_Block_Number").Equals(question["Sub_Block_Number"] as string);
                bool newBlock = row == 0 || !GetFieldOfRow(row - 1, "Block_Number").Equals(question["Block_Number"] as string);
                if (checkBox1.Checked && newSubBlock)
                {
                    // The current row is either the first row of the file or the first row of a new sub-block. Randomize it!
                    DataRow[] subBlock = currentExperimentInput.Select("Sub_Block_Number = '" + GetFieldOfRow(row + 1, "Sub_Block_Number") + "' AND Block_Number = '" + GetFieldOfRow(row + 1, "Block_Number") + "'");
                    ShuffleDataRows(ref subBlock);
                }
                // Prepare the control panel and the raw data for the next question:
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
                    MessageBox.Show("Something went wrong while collecting all data from the input file!\nMake sure that file references are stated using relative paths from the input file (separator '/', paths without leading '/') and that the input file has all neccessary columns...\n\nException message:\n" + ex.Message, "Problem with input file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
                // After the next question would be ready, check if we'd need to take a break before we continue...
                if (newSubBlock && checkBox3.Checked || newBlock && checkBox2.Checked)
                {
                    BreakScreen bs = new BreakScreen();
                    controlPanel.SetCurrentQuestionForm(bs);
                    controlPanel.Show();
                    bs.ShowDialog();
                    if (bs.DialogResult == DialogResult.Abort)
                    {
                        // The experimenter ended the questionnaire during the pasue screen! Close this form and return.
                        this.Close();
                        return;
                    }
                    if (checkBox4.Checked)
                    {
                        controlPanel.Hide();
                    }
                }
                // Display the current question to the participant:
                string answer = "";
                DateTime startTime = DateTime.Now;
                while (answer == "")
                {
                    try
                    {
                        // Show the current question form:
                        answer = ShowQuestionForm((Form)Activator.CreateInstance(QuestionsIndex.INDEX[question["Type"] as string], question));
                    }
                    catch (Exception ex)
                    {
                        answer = null;
                        MessageBox.Show("The question form with type " + question["Type"].ToString() + " couldn't be instantiated!\nPlease make sure you've added your new question form to the questions index and read the comments in that class file.\n\nException message:\n" + ex.Message, "Problem with creating question", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if (answer == "" && checkBox4.Checked && !controlPanel.Visible && MessageBox.Show("The control panel needs to be hidden during the questioning.\nAre you the experimenter AND do you want to show the control panel?", "Show control panel?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // The user wants to end the experiment and show the control panel again.
                        controlPanel.Show();
                    }
                }
                if (answer == null)
                {
                    // The last answer was null, which means that the experimenter aborted the questionnaire.
                    this.Close();
                    return;
                }
                File.AppendAllText("./Output/" + numericUpDown1.Value + ".txt", "\r\n" + numericUpDown1.Value + "\t" + numericUpDown2.Value + "\t" + (radioButton2.Checked ? "F" : "M") + "\t" + (question["ID"] as string) + "\t" + (question["Block_Number"] as string).Replace('\n', ' ') + "\t" + (question["Sub_Block_Number"] as string).Replace('\n', ' ') + "\t" + (question["Sub_Block_Type"] as string).Replace('\n', ' ') + "\t" + answer + "\t" + ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString() + "\t" + ((Int32)(DateTime.Now - startTime).TotalMilliseconds).ToString());
            }
            controlPanel.SetRunning(false);
            MessageBox.Show("Thanks for participating in the experiment!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            controlPanel.Close();
            this.Close();
        }

        private List<CheckBox> GetAllCheckboxes()
        {
            List<CheckBox> result = new List<CheckBox>();
            int i = 1;
            Control[] searchResult;
            while ((searchResult = Controls.Find("checkBox" + i.ToString(), true)).Length > 0)
            {
                result.Add(searchResult[0] as CheckBox);
                i++;
            }
            return result;
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
        /// Gets a specific data cell from the current input file.
        /// </summary>
        /// <param name="rowIndex">The index of the row you're interested in.</param>
        /// <param name="columnName">The name of the column you're interested in. Be aware that spaces need to be replaced by underscores!</param>
        /// <returns></returns>
        private string GetFieldOfRow(int rowIndex, string columnName)
        {
            return currentExperimentInput.Rows[rowIndex][columnName] as string;
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

        public static string dataFolderPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "QuestionnaireX";
            }
        }

        public static string configFilePath
        {
            get
            {
                return dataFolderPath + Path.DirectorySeparatorChar + "config";
            }
        }
    }
}
