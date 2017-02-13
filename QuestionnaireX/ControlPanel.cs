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
using System.Runtime.InteropServices;
using System.Media;

namespace QuestionnaireX
{
    public partial class ControlPanel : Form
    {
        private Form currentQuestionForm;
        private int timeRemaining = 0;
        private SoundPlayer beep;
        private bool running;

        public ControlPanel()
        {
            InitializeComponent();
            beep = new SoundPlayer(Properties.Resources.BEEP);
            SetTimer((int)numericUpDown1.Value);
            running = true;
        }

        #region KeepingWindowEnabledDuringShowDialog

        [DllImport("user32.dll")]
        private static extern void EnableWindow(IntPtr handle, bool enable);

        protected override void WndProc(ref System.Windows.Forms.Message msg)
        {
            if (msg.Msg == 0x000a /* WM_ENABLE */ && msg.WParam == IntPtr.Zero)
            {
                EnableWindow(this.Handle, true);
                return;
            }
            base.WndProc(ref msg);
        }

        #endregion KeepingWindowEnabledDuringShowDialog

        public void SetCurrentQuestionForm(Form newQuestionForm)
        {
           // The main window is telling the control panel that another question form is shown from now on.
            this.currentQuestionForm = newQuestionForm;
        }

        public void SetTimer(int seconds)
        {
            timeRemaining = seconds;
            DisplayRemainingTime();
            numericUpDown1.Value = seconds;
        }

        public void SetRunning(bool running)
        {
            this.running = running;
        }

        public void UpdateQuestionID(string id)
        {
            labelQuestion.Text = id;
        }

        public void UpdateBlock(string block)
        {
            labelBlock.Text = block;
        }

        public void UpdateSubBlock(string subBlock)
        {
            labelSubBlock.Text = subBlock;
        }

        public void UpdateSubBlockType(string subBlockType)
        {
            labelSubBlockType.Text = "(" + subBlockType + ")";
        }

        private void DisplayRemainingTime()
        {
            label1.Text = String.Format("{0}:{1}", (timeRemaining / 60).ToString("D2"), (timeRemaining % 60).ToString("D2"));
        }

        private void StartTimer()
        {
            button2.Text = "PAUSE TIMER";
            timer1.Start();
        }
        
        private void StopTimer()
        {
            button2.Text = "START TIMER";
            timer1.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // The experimenter wants to start or stop the timer.
            if (timeRemaining > 0)
            {
                if (timer1.Enabled)
                {
                    StopTimer();
                }
                else
                {
                    StartTimer();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Reset the timer to the given start time:
            StopTimer();
            SetTimer((int)numericUpDown1.Value);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // As long as there's time remaining, decrease the timer display.
            timeRemaining--;
            DisplayRemainingTime();
            if (timeRemaining == 0)
            {
                StopTimer();
                beep.Play();
            }
        }

        private void ControlPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.running)
            {
                // If the experiment is not running anymore, do not ask the experimenter if they want to close it...
                return;
            }
            this.Hide();
            if (MessageBox.Show(this, "Do you really want to quit the questionnaire?\nThe progress until now will be saved but you won't be able to directly start again from where you left after exiting the questionnaire.", "Quit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                StopTimer();
                if (currentQuestionForm != null)
                {
                    currentQuestionForm.DialogResult = DialogResult.Abort;
                    currentQuestionForm.Close();
                }
            }
            else
            {
                this.Show();
                e.Cancel = true;
            }
        }
    }
}
