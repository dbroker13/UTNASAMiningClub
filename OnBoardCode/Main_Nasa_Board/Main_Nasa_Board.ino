#include <SoftwareSerial.h> //allows use of other pins as serial pins
#include "RoboClaw.h" //allows use of roboclaw objects and methods
#include <string.h> //allows use of strings
#include <NASAMotorControl.h> //includes string values of motor names and commands
#include <avr/wdt.h> //allows use of watchdog timer

SoftwareSerial serial(2, 3); //Roboclaw S2 to Arduino RX = pin 2; Roboclaw S1 to Arduino TX = pin 3
RoboClaw roboclaw(&serial, 10000); //sets up roboclaw object with 10 ms timeout

#define address_drive 0x80 //address of roboclaw controlling drive motors
#define address_loader 0x81 //address of roboclaw controlling actuator and belt motor

const byte EOT = 0x04;
const int transition_delay = 10;
//const int reset_pin = 12;

double left_drive_speed = 64;
double right_drive_speed = 64;
int actuator_speed = 64;
int belt_speed = 64;

//String MotorName, Command, MotorSpeed; //Declares message components as global variables so they can be used in all functions
//int new_speed;

void setup()
{
  pinMode(13, OUTPUT);
  //digitalWrite(reset_pin, HIGH);
  //delay(200);
  //pinMode(reset_pin, OUTPUT);

  Serial.begin(115200);
  delay(1000);
  roboclaw.begin(115200);

  roboclaw.ForwardBackwardM1(address_drive, left_drive_speed); //set left motor to zero speed
  roboclaw.ForwardBackwardM2(address_drive, right_drive_speed); //set right motor to zero speed
  roboclaw.ForwardBackwardM1(address_loader, actuator_speed); //set actuator to zero speed
  roboclaw.ForwardBackwardM2(address_loader, belt_speed); //set belt motor to zero speed

  //this sets up and starts the watchdog timer
  //wdt_disable(); //prevent timer from resetting arduino
  //WDTCSR |= 0b00011000; //Enter setup mode
  //3rd, 6th, 7th, 8th bits are used for timing: see chart for timing codes
  //Will do interrupt after 4 seconds then do reset after the next 4 seconds if a new command hasn't been received if setting is interrupt and reset
  //Even if it does receive a command after the interrupt, it will reset after the next 4 second of inactivity
  //WDTCSR = 0b00101000; //Set WDT for reset with 4 second interval
  //wdt_reset(); //reset the timer
}

void motorsDrive(String Command, int new_speed)
{
  Serial.println();

  double new_left_speed; //Stores the new speed value for the left motor (initialized to the old value in case an invalid command is sent)
  double new_right_speed; //Stores the new speed value for the right motor (initialized to the old value in case an invalid command is sent)

  //Sets a new speed for the left and right motors that they will be brought up to
  if (Command.equals(Forward) || Command.equals(Reverse) || Command.equals(Stop)) //Forward, Reverse, and Stop
  {
    new_left_speed = new_speed;
    new_right_speed = new_speed;
  }
  else if (Command.equals(Left)) //Soft Left Turn
  {
    new_left_speed = (int)(new_speed / 2 + 32);
    new_right_speed = new_speed;
  }
  else if (Command.equals(Right)) //Soft Right Turn
  {
    new_left_speed = new_speed;
    new_right_speed = (int)(new_speed / 2 + 32);
  }
  else if (Command.equals(CCW)) //Counter-Clockwise Point Turn
  {
    new_left_speed = 127 - new_speed;
    new_right_speed = new_speed;
  }
  else if (Command.equals(CW)) //Clockwise Point Turn
  {
    new_left_speed = new_speed;
    new_right_speed = 127 - new_speed;
  }

  double left_increment = 1, right_increment = 1;
  double right_difference = abs(right_drive_speed - new_right_speed), left_difference = abs(left_drive_speed - new_left_speed);

  if (left_difference > right_difference)
  {
    right_increment = right_difference / left_difference;
  }
  else if (left_difference < right_difference)
  {
    left_increment = left_difference / right_difference;
  }

  while (abs(left_drive_speed - new_left_speed) > .01 || abs(right_drive_speed - new_right_speed) > .01)
  {
    if (left_drive_speed > new_left_speed)
    {
      left_drive_speed -= left_increment;
    }
    else if (left_drive_speed < new_left_speed)
    {
      left_drive_speed += left_increment;
    }

    if (right_drive_speed > new_right_speed)
    {
      right_drive_speed -= right_increment;
    }
    else if (right_drive_speed < new_right_speed)
    {
      right_drive_speed += right_increment;
    }

    //makes sure that the motors and speed variables are set at exactly the new speeds at the end
    if (abs(left_drive_speed - new_left_speed) < .01 && abs(right_drive_speed - new_right_speed) < .01)
    {
      left_drive_speed = new_left_speed;
      right_drive_speed = new_right_speed;
    }

    delay(transition_delay);

    roboclaw.ForwardBackwardM1(address_drive, (int)left_drive_speed); //set left motor to forward, reverse, or stop at speed value sent
    roboclaw.ForwardBackwardM2(address_drive, (int)right_drive_speed); //set right motor to forward, reverse, or stop at speed value sent
    Serial.println("Drive Motors, " + Command + ", Left: " + (int)left_drive_speed + " Right: " + (int)right_drive_speed); //print out drive motors command and speeds for troubleshooting
  }
}

void actuator(String Command, int new_speed)
{
  while (actuator_speed != new_speed)
  {
    if (actuator_speed > new_speed)
    {
      actuator_speed -= 1;
      delay(transition_delay);
    }
    else if (actuator_speed < new_speed)
    {
      actuator_speed += 1;
      delay(transition_delay);
    }

    roboclaw.ForwardBackwardM1(address_loader, actuator_speed); //set actuator to forward, reverse, or stop at speed value sent

    Serial.println("Actuator " + Command + " " + actuator_speed); //print out actuator command and speed for troubleshooting
    Serial.println();
  }
}

void beltMotor(String Command, int new_speed)
{
  while (belt_speed != new_speed)
  {
    if (belt_speed > new_speed)
    {
      belt_speed -= 1;
      delay(transition_delay);
    }
    else if (belt_speed < new_speed)
    {
      belt_speed += 1;
      delay(transition_delay);
    }

    roboclaw.ForwardBackwardM2(address_loader, belt_speed); //set belt motor to forward, reverse, or stop at speed value sent

    Serial.println("Belt Motor " + Command + " " + belt_speed); //print out belt motor command and speed for troubleshooting
    Serial.println();
  }
}

void loop() { // run over and over
  delay(1);
  if (Serial.available() > 0)
  {
    delay(100);
    size_t len = Serial.available();
    uint8_t sbuf[len];
    Serial.readBytesUntil('!', sbuf, len);
    //delay(1000);
    String messageIn = "";
    messageIn = String((char*)sbuf);
    //Serial.print(messageIn);
    int commaOne = 0;
    int commaTwo = 0;
    int endOfCommand = 0;

    for (int i = 0; i < messageIn.length(); i++)
    {
      if (messageIn.charAt(i) == ',' && commaOne == 0)
      {
        commaOne = i;
      }
      else if (messageIn.charAt(i) == ',' && commaOne != 0 && commaTwo == 0)
      {
        commaTwo = i;
      }
      else if (messageIn.charAt(i) == ':' && endOfCommand == 0)
      {
        endOfCommand = i;
      }
    }

    if (commaOne < commaTwo && commaTwo < endOfCommand)
    {
      String MotorName = messageIn.substring(0, commaOne);
      String Command = messageIn.substring((commaOne + 2), commaTwo);
      String MotorSpeed = messageIn.substring((commaTwo + 1), (endOfCommand));
      //Serial.print(MotorName + " " + Command + " " + MotorSpeed);

      int new_speed = MotorSpeed.toInt(); //Sets the integer new_speed to the value stored in the string MotorSpeed

      if (new_speed > 127) //Sets the full speed value to 127 as that's the upper limit of ForwardBackward() command
      {
        new_speed = 127;
      }

      if (MotorName.equals(DriveMotor))
      {
        wdt_reset();
        motorsDrive(Command, new_speed);
      }
      else if (MotorName.equals(Actuator)) //Name subject to change based on GUI
      {
        wdt_reset();
        actuator(Command, new_speed);
      }
      else if (MotorName.equals(BeltMotor)) //Name subject to change based on GUI
      {
        wdt_reset();
        beltMotor(Command, new_speed);
      }
      else if (sbuf[0] == 'a') //(Debugging Purposes) Press a to run drive motors
      {
        wdt_reset();
        roboclaw.ForwardBackwardM1(address_drive, 96);
        roboclaw.ForwardBackwardM2(address_drive, 96);
      }
      else if (sbuf[0] == 'b') //(Debugging Purposes) Press b to stop drive motors
      {
        wdt_reset();
        roboclaw.ForwardBackwardM1(address_drive, 64);
        roboclaw.ForwardBackwardM2(address_drive, 64);
      }
      else if (sbuf[0] == 'c') //(Debugging Purposes) Press c to run actuator
      {
        wdt_reset();
        roboclaw.ForwardBackwardM1(address_loader, 96);
      }
      else if (sbuf[0] == 'd') //(Debugging Purposes) Press d to stop actuator
      {
        wdt_reset();
        roboclaw.ForwardBackwardM1(address_drive, 64);
      }
      else if (sbuf[0] == 'e') //(Debugging Purposes) Press e to run belt motor
      {
        wdt_reset();
        roboclaw.ForwardBackwardM2(address_drive, 96);
      }
      else if (sbuf[0] == 'f') //(Debugging Purposes) Press f to stop belt motor
      {
        wdt_reset();
        roboclaw.ForwardBackwardM2(address_drive, 64);
      }
    }
    else
    {
      Serial.println("ERORR: Message was not parsed correctly");
    }
  }
}

//Use to add commands before the Arduino resets, must change WDT register from 0b00101000 (reset) to 0b01100000 (interrupt) or 0b01101000 (interrupt and reset)
ISR (WDT_vect)
{
  //motorsDrive(Stop, 64);
  //digitalWrite(reset_pin, LOW);
}

