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
    public partial class Text : Form
    {
        private string userText;
        
        /// <summary>
        /// Instanciates the form and sets all input values according to the data inside the raw question row.
        /// </summary>
        /// <param name="rawQuestionData">A row representing the instruction that should be displayed to the participant.</param>
        public Text(DataRow rawQuestionData)
            : this(rawQuestionData["Question"] as string)
        { }
        
        /// <summary>
        /// Instanciates the form and sets all input values according to the given parameters.
        /// </summary>
        /// <param name="text">The instruction that should be displayed to the user.</param>
        public Text(string text)
        {
            InitializeComponent();
            // Display the given question using the rich text box:
            richTextBox1.Text = text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Once the user finished typing in their answer to the question, save it and close this form again.
            if (!this.textBox1.Text.Trim().Equals(""))
            {
                this.userText = textBox1.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                // The participant didn't enter anything!
                MessageBox.Show("You have not entered something to answer the question. To proceed with the questionnaire, please enter your answer in the lower text box.", "No input!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public override string ToString()
        {
            // For this simple form, the result is always the same: The user pressed the button and confirmed that they understood the instruction.
            return userText;
        }
    }
}
