#include "RF24.h"
#include "RF24Network.h"
#include "RF24Mesh.h"
#include <SPI.h>
#include <EEPROM.h>
//#include <printf.h>

const int buttonPin = 2;     
const int ledPin =  3;  
int buttonState = LOW;
int ledState = LOW;
int previousState = LOW;
long turnLightsOfAt = 0;

RF24 radio(7, 8);
RF24Network network(radio);
RF24Mesh mesh(radio, network);

struct payload_t {
  unsigned long msToHigh;
};

void setup() {
 // Serial.begin(115200);
  pinMode(ledPin, OUTPUT);
  pinMode(buttonPin, INPUT);

  mesh.setNodeID(1);
  mesh.begin();
}


void loop() {
  mesh.update();
  sendButtonState();
  checkForMessage();
  toggleLed();
  renewConnection();
}

long timeToRenew;
void renewConnection(){
  if(millis() > timeToRenew)
  {
  //  Serial.println("Renewing connection");
    mesh.checkConnection();
        mesh.renewAddress();
        timeToRenew =  millis() + 5000;
  }
}
void sendButtonState()
{
  buttonState = digitalRead(buttonPin);
  if(buttonState == HIGH && previousState != buttonState)
  {
    //    Serial.println("Sending button state");
    if(!mesh.write(&buttonState, 'B', sizeof(buttonState)))
    {
    //          Serial.println("Send failed");
      if ( ! mesh.checkConnection() ) {
        mesh.renewAddress();
      }
    }
  }
 previousState = buttonState;
}

void toggleLed()
{
  bool turnOnLed = false;
  if((millis() < turnLightsOfAt))
  {
    turnOnLed = true;
  //  Serial.print("turn off at turnLightsOfAt");
  //  Serial.println(turnLightsOfAt);
  }
  if((turnOnLed == true) && (ledState == LOW))
  {
     ledState = HIGH;
     digitalWrite(ledPin, ledState);
   //  Serial.println("Turn on");
  }
  else if((turnOnLed == false) && (ledState == HIGH))
  {
    ledState = LOW;
    digitalWrite(ledPin, ledState);
 //   Serial.println("Turn off");
  }
}

void checkForMessage(){
   while (network.available()) {
    RF24NetworkHeader header;
    payload_t payload;
    network.read(header, &payload, sizeof(payload));
    turnLightsOfAt = millis() + payload.msToHigh;
//    Serial.println(payload.msToHigh);
  }
}
