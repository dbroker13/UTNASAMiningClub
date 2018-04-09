using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace NasaRobot
{
    public partial class Form1 : Form
    {
        System.Net.Sockets.TcpClient sock = new TcpClient();
        bool addValue = true;
        int motorSpeed = 64;
        int actuatorSpeed = 64;

        public Form1()
        {
            InitializeComponent();
            cmbMotorSelect.SelectedIndex = 0;
            cmbActuatorSelect.SelectedIndex = 0;
            connectSocket();
        }

        private void connectSocket()
        {
            string board = "192.168.69.120"; //make sure to update this once IP is made static
            sock.Connect(board, 23);

        }
        

        private void sendMessage(string deviceName, string command, int value)
        {
            NetworkStream serverStream = sock.GetStream();
            //calculate actual value to pass to motor. value of 64 is stop, > 64 is forward, < 64 is backwards. range is 0-128
            value = (value * ((addValue) ? 1 : -1)) + 64;
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(deviceName + ", " + command + ", " + value + "\x04");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();



            //read back buffer. We were getting issues, so commented out for now
            /*byte[] inStream = new byte[99999];
            serverStream.Read(inStream, 0, (int)sock.ReceiveBufferSize);
            string returnData = System.Text.Encoding.ASCII.GetString(inStream);
            lblDisplay.Text = returnData;
            */

        }


        #region MOTOR CONTROLS

        #region RADIO BUTTONS
        private void rbMotorEighth_CheckedChanged(object sender, EventArgs e)
        {
            motorSpeed = 8;
        }

        private void rbMotorQuarter_CheckedChanged(object sender, EventArgs e)
        {
            motorSpeed = 16;
        }

        private void rbMotorThreeEighths_CheckedChanged(object sender, EventArgs e)
        {
            motorSpeed = 24;
        }

        private void rbMotorHalf_CheckedChanged(object sender, EventArgs e)
        {
            motorSpeed = 32;
        }

        private void rbMotorFiveEighths_CheckedChanged(object sender, EventArgs e)
        {
            motorSpeed = 40;
        }

        private void rbMotorThreeQuarters_CheckedChanged(object sender, EventArgs e)
        {
            motorSpeed = 48;
        }

        private void rbMotorSevenEighths_CheckedChanged(object sender, EventArgs e)
        {
            motorSpeed = 56;
        }

        private void rbMotorFull_CheckedChanged(object sender, EventArgs e)
        {
            motorSpeed = 64;
        }

        #endregion

        #region BUTTON COMMANDS
        private void btnForward_Click(object sender, EventArgs e)
        {
            addValue = true;
            sendMessage(cmbMotorSelect.SelectedText, "Forward", motorSpeed);
        }

        private void btnMotorStop_Click(object sender, EventArgs e)
        {
            sendMessage(cmbMotorSelect.SelectedText, "Stop", motorSpeed);
        }

        private void btnReverse_Click(object sender, EventArgs e)
        {
            addValue = false;
            sendMessage(cmbMotorSelect.SelectedText, "Reverse", motorSpeed);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            sendMessage(cmbMotorSelect.SelectedText, "Left", motorSpeed);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            sendMessage(cmbMotorSelect.SelectedText, "Right", motorSpeed);
        }
        #endregion

        #endregion

        #region ACTUATOR CONTROLS

        #region RADIO BUTTONS

        private void rbActEighth_CheckedChanged(object sender, EventArgs e)
        {
            actuatorSpeed = 8;
        }

        private void rbActQuarter_CheckedChanged(object sender, EventArgs e)
        {
            actuatorSpeed = 16;
        }

        private void rbActThreeEighths_CheckedChanged(object sender, EventArgs e)
        {
            actuatorSpeed = 24;
        }

        private void rbActHalf_CheckedChanged(object sender, EventArgs e)
        {
            actuatorSpeed = 32;
        }

        private void rbActFiveEighths_CheckedChanged(object sender, EventArgs e)
        {
            actuatorSpeed = 40;
        }

        private void rbThreeQuarters_CheckedChanged(object sender, EventArgs e)
        {
            actuatorSpeed = 48;
        }

        private void rbSevenEighths_CheckedChanged(object sender, EventArgs e)
        {
            actuatorSpeed = 56;
        }

        private void rbActFull_CheckedChanged(object sender, EventArgs e)
        {
            actuatorSpeed = 64;
        }

        #endregion

        #region BUTTON COMMANDS

        private void btnUp_Click(object sender, EventArgs e)
        {
            addValue = true;
            sendMessage(cmbActuatorSelect.SelectedText, "Up", actuatorSpeed);
        }

        private void btnActuatorStop_Click(object sender, EventArgs e)
        {
            sendMessage(cmbActuatorSelect.SelectedText, "Up", actuatorSpeed);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            addValue = false;
            sendMessage(cmbActuatorSelect.SelectedText, "Up", actuatorSpeed);
        }




        #endregion

        #endregion

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
