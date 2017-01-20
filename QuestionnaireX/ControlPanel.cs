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

        public ControlPanel()
        {
            InitializeComponent();
            beep = new SoundPlayer(Properties.Resources.BEEP);
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

        public void StartTimer(int seconds, bool waitForConfirmation = false)
        {
            timeRemaining = seconds;
            DisplayRemainingTime();
            if (waitForConfirmation)
            {
                button2.Enabled = true;
            }
            else
            {
                timer1.Start();
            }
        }

        private void DisplayRemainingTime()
        {
            label1.Text = String.Format("{0}:{1}", (timeRemaining / 60).ToString("D2"), (timeRemaining % 60).ToString("D2"));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (currentQuestionForm != null && MessageBox.Show("Do you really want to quit the questionnaire?\nThe progress until now will be saved but you won't be able to directly start again from where you left after exiting the questionnaire.", "Quit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                timer1.Stop();
                currentQuestionForm.DialogResult = DialogResult.Abort;
                currentQuestionForm.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // The experimenter manually started the timer.
            if (timeRemaining > 0)
            {
                button2.Enabled = false;
                timer1.Start();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // As long as there's time remaining, decrease the timer display.
            timeRemaining--;
            DisplayRemainingTime();
            if (timeRemaining == 0)
            {
                timer1.Stop();
                beep.Play();
            }
        }
    }
}
