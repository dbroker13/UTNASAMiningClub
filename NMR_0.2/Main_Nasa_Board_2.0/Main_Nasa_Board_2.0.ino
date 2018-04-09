//#include <SoftwareSerial.h>
//#include <WiFi.h>

//SoftwareSerial mySerial(0, 1); // RX, TX
#include <SoftwareSerial.h>
#include "RoboClaw.h"
#include <string.h>
#include <NASAMotorControl.h>

SoftwareSerial serial(2,3); //S2 2 S1 3 
RoboClaw roboclaw_DriveMotor(&serial,10000);

#define address 0x80
const byte EOT = 0x04;

void setup() {
  // Open
  pinMode (13,OUTPUT);
  Serial.begin(115200);
  delay(1000);

  roboclaw_DriveMotor.begin(115200);
}

void loop() { // run over and over
    //byte inByte = 3;
    //uint8_t answer[] = {0x44,0x72,0x69,0x76,0x65};
    //uint8_t answer = [D,r,i,v,e];
    //uint8_t answer[] = {'D','r','i','v','e'};
    delay(1);  
    if(Serial.available()>0){
      //inByte = Serial.read();
      delay(100);
      size_t len = Serial.available();
      uint8_t sbuf[len];
      Serial.readBytesUntil(EOT, sbuf, len);
      //delay(1000);
      String messageIn = "";      
      messageIn = String((char*)sbuf);
      //Serial.print(messageIn);
      int commaOne = 0;
      int commaTwo = 0;
          
      for(int i = 0; i < messageIn.length(); i++)
      {
        if(messageIn.charAt(i) == ',' && commaOne == 0){
          commaOne = i;
        }
        else if(messageIn.charAt(i) == ',' && commaOne != 0 && commaTwo == 0){
          commaTwo = i; 
        }
      }
      
      String MotorName = messageIn.substring(0, commaOne);
      String Command = messageIn.substring((commaOne+2), commaTwo);
      String StringPercent = messageIn.substring((commaTwo+1), (messageIn.length()-1));
      int Percent = StringPercent.toInt();

      Serial.print(MotorName + " " + Command + " " + Percent);
     // Serial.print(messageIn.substring(0, commaOne));
         
      if(((MotorName.equals(DriveMotor))) && Command.equals(Forward)){
        //digitalWrite(13, HIGH);
        roboclaw_DriveMotor.ForwardBackwardM1(address, 80); //set to forward
        roboclaw_DriveMotor.ForwardBackwardM2(address, 80);
      }
      else if((MotorName.equals(DriveMotor)) && Command.equals(Stop)){
        //digitalWrite(13, LOW);
        roboclaw_DriveMotor.ForwardBackwardM1(address, 64); //set to zero
        roboclaw_DriveMotor.ForwardBackwardM2(address, 64);
      }
      else if((MotorName.equals(DriveMotor)) && Command.equals(Reverse)){
        roboclaw_DriveMotor.ForwardBackwardM1(address, 48);
        roboclaw_DriveMotor.ForwardBackwardM2(address, 48);
          //digitalWrite(13, LOW);
      }
      else if(sbuf[0] == 'c'){
        roboclaw_DriveMotor.ForwardBackwardM1(address, 96);
      }
    }
}






