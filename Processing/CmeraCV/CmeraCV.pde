import gab.opencv.*;
import processing.video.*;


OpenCV feed;
PImage pi, pimask, mask;
Movie source;
int threshold = 150;
float polygonFactor = 1;

void settings(){
  size(852, 480);
}

void setup(){
  pi = new PImage(width,height);
  pimask = new PImage(width, height);
  mask = loadImage("mask.png");
  feed = new OpenCV(this, pi, true);
  source = new Movie(this, "input.mp4");
  source.loop();
}

void draw(){
  //image(feed.getOutput(),0,0);
  image(pi, 0,0);
  ArrayList<Contour> contours = feed.findContours(false, false);
  println(contours.size());
  if (contours.size() > 0) {
    for (Contour contour : contours) {

      contour.setPolygonApproximationFactor(polygonFactor);
      if (contour.numPoints() > 50) {

        stroke(0, 200, 200);
        strokeWeight(5);
        beginShape();

        for (PVector point : contour.getPolygonApproximation ().getPoints()) {
          vertex(point.x, point.y);
        }
        endShape();
      }
    }
  }
}
void movieEvent(Movie m) {
  m.read();
  source.loadPixels();
  pi = source.get();
  pimask = source.get();
  pimask.mask(mask);
  feed.loadImage(pi);
  feed.gray();
  feed.threshold(threshold);
}
