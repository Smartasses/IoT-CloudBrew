const int ledPin = 3;
int ledState = LOW;
long timeToShine;

void setup() {
  Serial.begin(115200);
  pinMode(ledPin, OUTPUT);
}

void loop() {
    toggleLed();
    readDuration();
}

void toggleLed(){
    if(millis() < timeToShine && ledState == LOW)
    {
      ledState = HIGH;
      digitalWrite(ledPin, ledState);      
      Serial.println("Turned on the led");
    }
    else if (millis() > timeToShine && ledState == HIGH){
      ledState = LOW;
      digitalWrite(ledPin, ledState);
      Serial.println("Turned off the led");
    }
}

void readDuration(){
  if(Serial.available()){
    int duration = Serial.parseInt();
    Serial.print("Received: ");
    Serial.println(duration);
    timeToShine = millis() + duration;
  }  
}
