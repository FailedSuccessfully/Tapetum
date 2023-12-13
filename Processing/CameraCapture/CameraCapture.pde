import hypermedia.net.*;

import processing.video.*;

Capture video;
UDP udp;
PVector point;

int tolerance;

void settings(){
  size(640, 480);
}
void setup() {
  tolerance = 0;
  String[] settings = loadStrings("settings.cfg");
  if (settings.length > 0){
    tolerance = int(settings[0]);
    println(tolerance);
  }
  point = new PVector();
  String[] cameras = Capture.list();
  video = new Capture(this, cameras[0]); 
  udp = new UDP(this);

  video.start();
}

void draw() {
  video.loadPixels();
  image(video, 0, 0 );

  stroke(255, 0, 0);
  strokeWeight(4);
  noFill();
  ellipse(point.x, point.y, 10, 10);
  udp.send(point.toString(), "localhost", 13000);
}

void captureEvent(Capture c) {
  c.read();
  findPoint(c);
}
void findLaserPoint(Capture feed, color laser){
  int bestX = 0, bestY = 0;
  float bestD = 100;
  float r2 = red(laser), g2 = green(laser), b2 = blue(laser);
  
  for (int x =0; x< feed.width; x++){
    for (int y = 0; y < feed.height; y++){
      int loc = x+y*feed.width;
      color current = feed.pixels[loc];
      float r1 = red(current);
      float g1 = green(current);
      float b1 = blue(current);
      
      float d = dist(r1, g1, b1, r2, g2, b2);
      
      if (d < bestD){
        bestX = x;
        bestY = y;
        bestD = d;
      }
    }
  }
  
  point.set(bestX, bestY);
}
void findPoint(Capture feed){
    
  color brightest = max(feed.pixels);
  float avgX = 0;
  float avgY = 0;
  int count = 0;
  //IntList hits = new IntList();
  for (int x =0; x< feed.width; x++){
    for (int y = 0; y < feed.height; y++){
      int loc = x + y * feed.width;
      color current = feed.pixels[loc];
      float r1 = red(current);
      float b1 = blue(current);
      float g1 = green(current);
      float r2 = red(brightest);
      float b2 = blue(brightest);
      float g2 = green(brightest);
      
      float d = dist(r1, g1, b1, r2, g2, b2);
      
      if (d < tolerance){
        avgX += x;
        avgY += y;
        count++;
      }
  }
  }
  if (count > 0){
   point.set(avgX/count, avgY/count);
  }
}
