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
    public partial class Slider : Form
    {
        /// <summary>
        /// Instanciates the form and sets all input values according to the data inside the raw question row.
        /// For the input file doesn't contain a start value yet, the default value of the main constructor will be used.
        /// </summary>
        /// <param name="rawQuestionData">A row representing the question this question form should display.</param>
        public Slider(DataRow rawQuestionData)
            : this(rawQuestionData["Question"] as string, rawQuestionData["Descriptor_Min"] as string, rawQuestionData["Descriptor_Max"] as string, int.Parse(rawQuestionData["Min"] as string), int.Parse(rawQuestionData["Max"] as string))
        { }
        
        /// <summary>
        /// Instanciates the form and sets all input values according to the given parameters.
        /// </summary>
        /// <param name="question">The question that should be displayed to the user.</param>
        /// <param name="descriptorMin">The description for the user of what the minimum value means.</param>
        /// <param name="descriptorMax">The description for the user of what the maximum value means.</param>
        /// <param name="minimumSliderValue">The minimum value the user can select using the slider.</param>
        /// <param name="maximumSliderValue">The maximum value the user can select using the slider.</param>
        /// <param name="startValue">An optional parameter for supplying a value that the slider should be initially set to.</param>
        public Slider(string question, string descriptorMin, string descriptorMax, int minimumSliderValue, int maximumSliderValue, int startValue = 0)
        {
            InitializeComponent();
            // Display the given question using the rich text box:
            richTextBox1.Text = question;
            // Display the descriptors for the minimum and maximum value:
            label2.Text = descriptorMin.Replace("\\n", "\n");
            label3.Text = descriptorMax.Replace("\\n", "\n");
            // Set the minimum and maximum of the trackbar to the given values:
            trackBar1.Minimum = minimumSliderValue;
            trackBar1.Maximum = maximumSliderValue;
            // Set the slider to the start value and update the label the first time:
            if (startValue >= minimumSliderValue && startValue <= maximumSliderValue)
            {
                trackBar1.Value = startValue;
            }
            else
            {
                Console.WriteLine("The start value for the slider was not inside the given range. Using average by integer division as start value.");
                trackBar1.Value = minimumSliderValue + (maximumSliderValue - minimumSliderValue) / 2;
            }
            trackBar1_ValueChanged(this, null);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            // If the user changed the value of the slider, update the label displaying the value to the user, as well.
            label1.Text = trackBar1.Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Once the user finished thinking about the question and tweaking the slider, just close this form again.
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public override string ToString()
        {
            return trackBar1.Value.ToString();
        }
    }
}
