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
    /// <summary>
    /// VAS stands for Visual Analog Scale.
    /// The difference to the "Scale" form is that no number of the current value is shown, the scale is always between 0 and 1000 and the return value between 0 and 1.
    /// Furthermore, the slider component has been optimized for setting the cursor by directly clicking into the control.
    /// </summary>
    public partial class ScaleVAS : Form
    {
        /// <summary>
        /// Instanciates the form and sets all input values according to the data inside the raw question row.
        /// For the input file doesn't contain a start value yet, the default value of the main constructor will be used.
        /// </summary>
        /// <param name="rawQuestionData">A row representing the question this question form should display.</param>
        public ScaleVAS(DataRow rawQuestionData)
            : this(rawQuestionData["Question"] as string, rawQuestionData["Descriptor_Min"] as string, rawQuestionData["Descriptor_Max"] as string, 0, 1000)
        { }
        
        /// <summary>
        /// Instanciates the form and sets all input values according to the given parameters.
        /// </summary>
        /// <param name="question">The question that should be displayed to the user.</param>
        /// <param name="descriptorMin">The description for the user of what the minimum value means.</param>
        /// <param name="descriptorMax">The description for the user of what the maximum value means.</param>
        /// <param name="minimumSliderValue">The minimum value the user can select using the slider.</param>
        /// <param name="maximumSliderValue">The maximum value the user can select using the slider.</param>
        public ScaleVAS(string question, string descriptorMin, string descriptorMax, int minimumSliderValue, int maximumSliderValue)
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
            // Set the slider to between the start and end value and update the label the first time:
            trackBar1.Value = minimumSliderValue + (maximumSliderValue - minimumSliderValue) / 2;
        }

        private void trackVolume_MouseDown(object sender, MouseEventArgs e)
        {
            // The width of the track bar gets multiplied by a factor so the cursor jumps exactly where the participant clicked.
            float factor = 1.03f;
            int newValue = trackBar1.Minimum + (int)Math.Round((e.X + (float)trackBar1.Width * (1 - factor) / 2) / (float)trackBar1.Width * factor * (trackBar1.Maximum - trackBar1.Minimum));
            // Limit the new value to the track bar to prevent the setter from throwing an exception when the participant clicked outside the scale range...
            trackBar1.Value = newValue > trackBar1.Maximum ? trackBar1.Maximum : newValue < trackBar1.Minimum ? trackBar1.Minimum : newValue;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Once the user finished thinking about the question and tweaking the slider, just close this form again.
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public override string ToString()
        {
            return (trackBar1.Value / 1000f).ToString(new System.Globalization.CultureInfo("en-US"));
        }
    }
}
