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
    public partial class Instructions : Form
    {
        /// <summary>
        /// Instanciates the form and sets all input values according to the data inside the raw question row.
        /// </summary>
        /// <param name="rawQuestionData">A row representing the instruction that should be displayed to the participant.</param>
        public Instructions(DataRow rawQuestionData)
            : this(rawQuestionData["Instruction"] as string)
        { }
        
        /// <summary>
        /// Instanciates the form and sets all input values according to the given parameters.
        /// </summary>
        /// <param name="text">The instruction that should be displayed to the user.</param>
        public Instructions(string text)
        {
            InitializeComponent();
            // Display the given instructions using the rich text box:
            richTextBox1.Text = text;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            // Once the user finished thinking about the question and tweaking the slider, just close this form again.
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public override string ToString()
        {
            // For this simple form, the result is always the same: The user pressed the button and confirmed that they understood the instruction.
            return "OK";
        }
    }
}
