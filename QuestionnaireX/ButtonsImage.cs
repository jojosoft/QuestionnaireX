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
    public partial class ButtonsImage : Form
    {
        private int selectedID;
        private List<Button> activeButtons;
        
        /// <summary>
        /// Instanciates the form and sets all input values according to the data inside the raw question row.
        /// </summary>
        /// <param name="rawQuestionData">A row representing the question this question form should display.</param>
        public ButtonsImage(DataRow rawQuestionData)
            : this(rawQuestionData["Question"] as string, readAnswers((rawQuestionData["Answers"] as string).Split(',')), readIDs((rawQuestionData["Answers"] as string).Split(',')), rawQuestionData["Image"] as string)
        { }
        
        /// <summary>
        /// Instanciates the form and sets all input values according to the given parameters.
        /// </summary>
        /// <param name="question">The question that should be displayed to the user.</param>
        /// <param name="answers">An array with up to ten possible answers.</param>
        /// <param name="ids">An array that provides the corresponding id for each possible answer.</param>
        /// <param name="image">Either an absolute path to an image file or a base64 encoded image.</param>
        public ButtonsImage(string question, string[] answers, int[] ids, string image)
        {
            if (answers.Length > 10)
            {
                throw new Exception("You specified more than ten answers, but this question form is only capable of displaying at maximum ten buttons.");
            }
            else if (answers.Length <= 1)
            {
                throw new Exception("You need to provide at least two possible answers! Use the question type 'Instruction' for displaying text with one answer button to the participant.");
            }
            InitializeComponent();
            // Display the given question using the rich text box:
            richTextBox1.Text = question;
            // Load the image either from an absolute path or base64 string.
            pictureBox1.Image = File.Exists(image) ? Image.FromFile(image) : Base64ToImage(image);
            // Modify buttons and reposition them:
            activeButtons = new List<Button>(10);
            for (int i = 1; i <= 10;  i++)
            {
                Button currentButton = Controls.Find("button" + i.ToString(), true)[0] as Button;
                if (i <= answers.Length)
                {
                    currentButton.Name = "button" + ids[i - 1];
                    currentButton.Text = answers[i - 1];
                    activeButtons.Add(currentButton);
                }
                else
                {
                    currentButton.Visible = false;
                }
            }
            // Now, align the buttons so they take all the space available:
            int useHeight = 554;
            float spacingRatio = 0.052f;
            int partHeight = useHeight / activeButtons.Count;
            for (int i = 0; i < activeButtons.Count; i++)
            {
                activeButtons[i].Location = new Point(activeButtons[i].Location.X, (int)Math.Round(225 + partHeight * i + spacingRatio * partHeight / (activeButtons.Count - 1)));
                activeButtons[i].Height = (int)Math.Round(partHeight * (1 - spacingRatio));
            }
        }

        /// <summary>
        /// A helper function that extracts only the answers from the input array and trims them.
        /// </summary>
        /// <param name="input">An array that contains ids and answers, each pair seperated by a colon, one pair per index.</param>
        /// <returns>An array containing the trimmed answers in the same order than the ones in the input file.</returns>
        private static string[] readAnswers(string[] input)
        {
            string[] answers = new string[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                answers[i] = input[i].Split(':')[1].Trim();
            }
            return answers;
        }
        
        /// <summary>
        /// A helper function that extracts only the ids from the input array and parses them.
        /// </summary>
        /// <param name="input">An array that contains ids and answers, each pair seperated by a colon, one pair per index.</param>
        /// <returns>An array containing the parsed ids in the same order than the ones in the input file.</returns>
        private static int[] readIDs(string[] input)
        {
            int[] ids = new int[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                ids[i] = int.Parse(input[i].Split(':')[0].Trim());
            }
            return ids;
        }

        /// <summary>
        /// A helper function that converts a base64 string into an image.
        /// </summary>
        /// <param name="base64String">Your input stream encoded as base64 string.</param>
        /// <returns>The resulting image.</returns>
        public Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        private void button_Click(object sender, EventArgs e)
        {
            // Once the user finished thinking about the question and tweaking the slider, just close this form again.
            selectedID = int.Parse((sender as Button).Name.Substring(6));
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public override string ToString()
        {
            return selectedID.ToString();
        }
    }
}
