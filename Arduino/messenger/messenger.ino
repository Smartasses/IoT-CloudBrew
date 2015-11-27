#include "RF24Network.h"
#include "RF24.h"
#include "RF24Mesh.h"
#include <SPI.h>
//Include eeprom.h for AVR (Uno, Nano) etc. except ATTiny
#include <EEPROM.h>

RF24 radio(7,8);
RF24Network network(radio);
RF24Mesh mesh(radio,network);

struct payload_t {
  unsigned long msToHigh;
};

void setup() {
  Serial.begin(115200);
  mesh.setNodeID(0);
  mesh.begin();
}

void sendButtonMessage()
{
  if(network.available()){
    RF24NetworkHeader header;
    network.peek(header);
    
    int buttonState;
    switch(header.type){
      // Display the incoming millis() values from the sensor nodes
      case 'B': network.read(header,&buttonState,sizeof(buttonState));  Serial.print("*KCHH*" + padding(buttonState,10) + "OVER"); break;
      default: network.read(header,0,0);break;
    }
}}

void sendCommand()
{
  if (Serial.available() > 0)
  {
    unsigned long lightTime = Serial.parseInt();
    payload_t payload = {lightTime};
    for (int i = 0; i < mesh.addrListTop; i++) {
      RF24NetworkHeader header(mesh.addrList[i].address, OCT);
      network.write(header, &payload, sizeof(payload));
    }  
  }
}

void loop() {
   mesh.update();
   mesh.DHCP();
   sendButtonMessage();
   sendCommand();
}


String padding(unsigned long number, byte width ) {
  unsigned long currentMax = 10;
  String paddedNumber = "";
  for (byte i=1; i<width; i++){
    if (number < currentMax) {
      paddedNumber += "0";
    }
    currentMax *= 10;
  } 
  paddedNumber += number;
  return paddedNumber;
}
