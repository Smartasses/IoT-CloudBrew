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

#define nodeID 1

void setup() {
  pinMode(ledPin, OUTPUT);
  pinMode(buttonPin, INPUT);
}

struct payload_t {
  unsigned long msToHigh;
};

void loop() {
 
 sendButtonState();
 checkForMessage();
 toggleLed();

}

void sendButtonState()
{
  buttonState = digitalRead(buttonPin);
  if(buttonState == HIGH && previousState != buttonState)
  {
    mesh.write(&buttonState, 'B', sizeof(buttonState));
  }
 previousState = buttonState;
}

void toggleLed()
{
  int newLedState;
  if(millis() < turnLightsOfAt && ledState == LOW)
  {
    newLedState == HIGH;
  }
  else
  {
    newLedState = LOW;
  }
  if(ledState != newLedState)
  {
    ledState = newLedState;
    digitalWrite(ledPin, ledState);
  }
}

void checkForMessage(){
   while (network.available()) {
    RF24NetworkHeader header;
    payload_t payload;
    network.read(header, &payload, sizeof(payload));
    turnLightsOfAt = millis() + payload.msToHigh;
  }
}

