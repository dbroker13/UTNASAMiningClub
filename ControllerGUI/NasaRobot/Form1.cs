﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Windows.Threading;

namespace NasaRobot
{
    public partial class Form1 : Form
    {
        System.Net.Sockets.TcpClient sock = new TcpClient();
        bool addValue = true; //true if we're moving forward, false if we're going backwards
        int motorSpeed = 8;
        int actuatorSpeed = 8;
        Controller controller = new Controller(UserIndex.One);
        bool useController = false;


        DispatcherTimer timer = new DispatcherTimer();

        public Form1()
        {
            InitializeComponent();
            if (controller.IsConnected)
            {
                var response = MessageBox.Show("Xbox Controller connected. Use that instead?", "", MessageBoxButtons.YesNo);
                useController = response == DialogResult.Yes ? true : false;
            }

            //default comboboxes to first item in collection
            cmbMotorSelect.SelectedIndex = 0;
            cmbActuatorSelect.SelectedIndex = 0;

            connectSocket();

            if (useController)
            {
                pnlCompControl.Visible = false;
                pnlXboxControl.Visible = true;
                this.Visible = true;
                controlWithXbox();
            }
        }

        /// <summary>
        /// Establishes a socket connection with the wifi chip
        /// </summary>
        private void connectSocket()
        {
            string board = "192.168.69.113"; //make sure to update this once IP is made static
            sock.Connect(board, 23);

        }
        private void controlWithXbox()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += timer_Tick;
            timer.Start();
          
        }

        void timer_Tick(object sender, EventArgs e)
        {
            UpdateController();
        }

        public void UpdateController()
        {
            Point LeftThumb = new Point(0, 0);
            Point RightThumb = new Point(0, 0);
            float LeftTrigger, RightTrigger;
            int deadband = 2500;
            if (!controller.IsConnected)
            {
                return;
            }

            Gamepad gamepad = controller.GetState().Gamepad;

            LeftThumb.X = (int)((Math.Abs((float)gamepad.LeftThumbX) < deadband) ? 0 : (float)gamepad.LeftThumbX / short.MinValue * -100);
            LeftThumb.Y = (int)((Math.Abs((float)gamepad.LeftThumbY) < deadband) ? 0 : (float)gamepad.LeftThumbY / short.MaxValue * 100);
            RightThumb.Y = (int)((Math.Abs((float)gamepad.RightThumbX) < deadband) ? 0 : (float)gamepad.RightThumbX / short.MaxValue * 100);
            RightThumb.X = (int)((Math.Abs((float)gamepad.RightThumbY) < deadband) ? 0 : (float)gamepad.RightThumbY / short.MaxValue * 100);
            LeftTrigger = gamepad.LeftTrigger;
            RightTrigger = gamepad.RightTrigger;
            //State state = controller.GetState();

            if (gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
            {
                string command = getDirection(LeftThumb);
                //sendMessage(cmbMotorSelect.Text, command, motorSpeed);
            }
            else if (gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
            {
                //sendMessage(cmbMotorSelect.Text, "Stop", motorSpeed);
            }
            else if (gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
            {
                motorSpeed += 8;
            }
            else if (gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
            {
                motorSpeed -= 8;
            }

            lblDisplay.Text = gamepad.ToString();

        }

        private string getDirection(Point point)
        {
            string command = "";
            if (point.Y > 0 && point.X == 0)
            {
                command = "Forward";
            }
            else if (point.Y == 0 && point.X > 0)
            {
                command = "CCW";
            }
            else if (point.Y > 0 && point.X < 0)
            {
                command = "Left";
            }
            else if (point.Y < 0 && point.X == 0)
            {
                command = "Reverse";
            }
            else if (point.Y > 0 && point.X > 0)
            {
                command = "Right";
            }
            else if (point.Y == 0 && point.X < 0)
            {
                command = "CW";
            }
            else if (point.Y == 0 && point.X == 0)
            {
                command = "Stop";
            }

            return command;
        }

        private void sendMessage(string deviceName, string command, int value)
        {
            NetworkStream serverStream = sock.GetStream();
            //calculate actual value to pass to motor. value of 64 is stop, > 64 is forward, < 64 is backwards. range is 0-128
            value = (value * ((addValue) ? 1 : -1)) + 64; 
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(deviceName + ", " + command + ", " + value + ":!");
            string sendData = System.Text.Encoding.ASCII.GetString(outStream);

            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            //TODO: have this recieve command back
            //TODO: after recieving, compare to sent message to check for transmission errors 
            //TODO: implement timeout on recieving to ensure connection isnt lost

            //read back buffer. We were getting issues, so commented out for now
            byte[] inStream = new byte[99999];
            serverStream.Read(inStream, 0, (int)sock.ReceiveBufferSize);
            string returnData = System.Text.Encoding.ASCII.GetString(inStream);
            lblDisplay.Text = returnData;
            

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
            sendMessage(cmbMotorSelect.Text, "Forward", motorSpeed);
        }

        private void btnMotorStop_Click(object sender, EventArgs e)
        {
            addValue = true;
            sendMessage(cmbMotorSelect.Text, "Stop", 0); //the value here shouldnt matter as Stop is hardcoded in the board. But due to how the speed value is determined, passing in 0 will result in 64 being sent
        }

        private void btnReverse_Click(object sender, EventArgs e)
        {
            addValue = false;
            sendMessage(cmbMotorSelect.Text, "Reverse", motorSpeed);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            addValue = true;
            sendMessage(cmbMotorSelect.Text, "Left", motorSpeed);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            addValue = true;
            sendMessage(cmbMotorSelect.Text, "Right", motorSpeed);
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
            sendMessage(cmbActuatorSelect.Text, "Up", actuatorSpeed);
        }

        private void btnActuatorStop_Click(object sender, EventArgs e)
        {
            addValue = true;
            sendMessage(cmbActuatorSelect.Text, "Up", actuatorSpeed);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            addValue = false;
            sendMessage(cmbActuatorSelect.Text, "Up", actuatorSpeed);
        }




        #endregion

        #endregion

    

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (32)) //if the spacebar is pressed. allows quicker stopping of motors
            {
                sendMessage(cmbMotorSelect.Text, "Stop", 64);
            }
        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }
    }


    //class XInputController
    //{
    //    Controller controller;
    //    Gamepad gamepad;
    //    public bool connected = false;
    //    public int deadband = 2500;
    //    public Point leftThumb, rightThumb = new Point(0, 0);
    //    public float leftTrigger, rightTrigger;

    //    public XInputController()
    //    {
    //        controller = new Controller(UserIndex.One);
    //        connected = controller.IsConnected;
    //    }

    //    // Call this method to update all class values
    //    public void Update()
    //    {
    //        if (!connected)
    //            return;

    //        gamepad = controller.GetState().Gamepad;

    //        leftThumb.X = (int)((Math.Abs((float)gamepad.LeftThumbX) < deadband) ? 0 : (float)gamepad.LeftThumbX / short.MinValue * -100);
    //        leftThumb.Y = (int)((Math.Abs((float)gamepad.LeftThumbY) < deadband) ? 0 : (float)gamepad.LeftThumbY / short.MaxValue * 100);
    //        rightThumb.Y = (int)((Math.Abs((float)gamepad.RightThumbX) < deadband) ? 0 : (float)gamepad.RightThumbX / short.MaxValue * 100);
    //        rightThumb.X = (int)((Math.Abs((float)gamepad.RightThumbY) < deadband) ? 0 : (float)gamepad.RightThumbY / short.MaxValue * 100);

    //        leftTrigger = gamepad.LeftTrigger;
    //        rightTrigger = gamepad.RightTrigger;
    //    }
    //}



}
