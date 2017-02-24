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

using FileHelpers;

namespace QuestionnaireX
{
    public partial class Buttons : Form
    {
        private int selectedID;
        private List<Button> activeButtons;
        
        /// <summary>
        /// Instanciates the form and sets all input values according to the data inside the raw question row.
        /// For the input file doesn't contain a start value yet, the default value of the main constructor will be used.
        /// </summary>
        /// <param name="rawQuestionData">A row representing the question this question form should display.</param>
        public Buttons(DataRow rawQuestionData)
            : this(rawQuestionData["Question"] as string, readAnswers((rawQuestionData["Answers"] as string).Split(',')), readIDs((rawQuestionData["Answers"] as string).Split(',')))
        { }
        
        /// <summary>
        /// Instanciates the form and sets all input values according to the given parameters.
        /// </summary>
        /// <param name="question">The question that should be displayed to the user.</param>
        /// <param name="answers">An array with up to ten elements containing </param>
        public Buttons(string question, string[] answers, int[] ids)
        {
            if (answers.Length > 10)
            {
                throw new Exception("You specified more than ten answers, but this question form is only capable of displaying at maximum ten buttons.");
            }
            InitializeComponent();
            // Display the given question using the rich text box:
            richTextBox1.Text = question;
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
            int useWidth = 1160;
            float spacingRatio = 0.052f;
            int partWidth = useWidth / activeButtons.Count;
            for (int i = 0; i < activeButtons.Count; i++)
            {
                activeButtons[i].Location = new Point((int)Math.Round(15 + partWidth * i + spacingRatio * partWidth / (activeButtons.Count - 1)), activeButtons[i].Location.Y);
                activeButtons[i].Width = (int)Math.Round(partWidth * (1 - spacingRatio));
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
