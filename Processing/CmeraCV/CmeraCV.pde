import gab.opencv.*;
import processing.video.*;
import hypermedia.net.*;


final String cfgFile = "settings.cfg";
OpenCV feed;
PImage pi, pimask, mask;
Capture source;
UDP udp;

int threshold = 150;
float polygonFactor = 20;
int maxPoints = 2000, minPoints = 100;
String maskImgPath = "mask.jpg";
int udpPort = 13000;
int resolutionW = 1920;
int resolutionH =  1080;

void settings(){
  size(resolutionW, resolutionH);
  udp = new UDP(this);
}

void setup(){
  JSONObject settings;
  File f = new File(dataPath(cfgFile));
  if (f.exists()){
    settings = loadJSONObject(cfgFile);
    threshold = settings.getInt("threshold");
    polygonFactor = settings.getFloat("polygon_factor");
    maxPoints = settings.getInt("maximum_polygon_points");
    minPoints = settings.getInt("minimum_polygon_points");
    maskImgPath = settings.getString("mask_image_path");
    udpPort = settings.getInt("udp_port");
    resolutionW = settings.getInt("resolution_width");
    resolutionH = settings.getInt("resolution_height");
    windowResize(resolutionW,resolutionH);
  } else {
    settings = new JSONObject();
    settings.setInt("threshold", threshold);
    settings.setFloat("polygon_factor", polygonFactor);
    settings.setInt("maximum_polygon_points", maxPoints);
    settings.setInt("minimum_polygon_points", minPoints);
    settings.setString("mask_image_path", maskImgPath);
    settings.setInt("udp_port", udpPort);
    settings.setInt("resolution_width", resolutionW);
    settings.setInt("resolution_height", resolutionH);
    saveJSONObject(settings, "data\\" +cfgFile);
  }
  
  pi = new PImage(width,height);
  pimask = new PImage(width, height);
  mask = loadImage(maskImgPath);
  feed = new OpenCV(this, pimask, true);
  source = new Capture(this, Capture.list()[0]);
  source.start();
  
  stroke(0, 200, 200);
  strokeWeight(5);
  noFill();
}

void draw(){
  
  background(0);
  image(pi, 0,0);
  //image(feed.getOutput(),0,0);
  //image(pimask,0,0);
  
  ArrayList<Contour> contours = feed.findContours(true, false);
  //println(contours.size());
  if (contours.size() > 0) {
    for (Contour contour : contours) {

      contour.setPolygonApproximationFactor(polygonFactor);
      if (contour.numPoints() > minPoints && contour.numPoints() < maxPoints) {
        Contour convex = contour.getPolygonApproximation().getConvexHull();
        convex.draw();
        
        int sumX=0, sumY=0, count = 0;
        for (PVector point : convex.getPoints()) {
          sumX += point.x;
          sumY += point.y;
          count++;
        }
        udp.send(new PVector(sumX / count, sumY / count).toString(), "localhost", udpPort);
        
      }
    }
  }
  
}
void captureEvent(Capture c) {
  c.read();
  source.loadPixels();
  pi = source.get();
  pimask = source.get();
  //pimask.mask(mask);
  for (int x = 0; x < width; x++)
  {
    for (int y = 0; y < height; y++)
    {
      color col = pi.get(x,y);
      color maskCol = mask.get(x,y);
      color black = color(0);
      if (maskCol == black)
      {
        pimask.set(x,y,0);
      }
      else
      {
        pimask.set(x,y,col);
      }
    }
  }
  pimask.updatePixels();
  //PImage temp = pimask.copy();
  //temp.updatePixels();
  feed.loadImage(pimask);
  feed.gray();
  feed.threshold(threshold);
}
