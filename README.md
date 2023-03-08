# Tapetum

Tapetum is an application that is part of project Midbarium. It is an eductional attraction in which visitors direct a flashlight towards stickers representing animal eyes to learn which animal these eyes belong to.
The application is built on top of unity. It relies on serial communication from an arduino controller in order to function.
The arduino controller is connected to a 9DOF (9 Degrees of Freedom) sensor that uses an accelerometer, a magnetometer, and a gyroscope in order to approximate orientation in 3D Space.

Here are, in short summary, the steps required in order to use this project: (TODO: add links and in dpeth instructions)

## A. Arduino Side
All programs are taken from the same repo, which i have forked and somewhat modified - [My Fork](https://github.com/FailedSuccessfully/LSM9DS1-AHRS) 
Unless stated otherwise, the files I will mention live in the forked repo.

Terminology:
- AHRS - stands for "Altitude and Heading Reference System". It is a type of device that utilizes the aforementioned sensors in order to measure orientation and direction, primarly in aircrafts.
- Declination - a property of magnetic readings the relates to geographic location, and also changes over time. As of writing these lines, magnetic declination in israel revolves around 5 degrees. If you notice obvious drift in accuracy over time, you might need to update this value. For more detail, see the filter section below.

### Calibration
In order for the readings to be accurate, it is necessary to callibrate the sensor before use of the sensors. It might be necessary to perform calibration as routine maintainance.

- 1. Data Collection for Calibration - 
Data for sensor calibration is collected by using the arduino program *LSM9DS1_cal_data*, which i modified slightly. Compiling and uploading the program to the controller will initiate a 2 part sequence. The program will output user instructions over serial messages. The sequence looks like this:
  - a. Collection of gyro data. The controller should not be moved during collection.
  - b. Collection of acceleration and magnetic data. The controller should be rotated slowly on 3 axes for the duration of data collection. 
You may use the unity script in this project, *Collector.cs*, to output the readings into CSV format. The spacebar key will set the serial connection on\off. The script will output the instructions onto the unity debug logger, and will collect the data from the readings. After the data collection is finished, press the "S" key to produce 3 csv files, one for each sensor.
- 2. Magneto - 
Magneto will take your acceleration and magnetic readings and produce some wierd looking data that looks like this: 
```
 float B[3]
  { -133.33,   72.29, -291.92};

 float Ainv[3][3]
  {{  1.00260,  0.00404,  0.00023},
  {  0.00404,  1.00708,  0.00263},
  {  0.00023,  0.00263,  0.99905}};
```
Save these outputs, we will use them in the next step.
In this repository there is a precompiled exe of magneto. To use it, run the exe top open a command line interface. Type in the path to the relevant CSV file, and when it asks you how to proceed, choose the default option. The data will be output into the console interface. It will then ask you if you want to save the modified readings, which isn't needed and can be skipped.
If you find the need to compile Magneto yourself, the source code is in the forked repo.
- 3. Custom AHRS filter -
a somewhat modified version of the Mahony AHRS filter that takes sensor data and converts it to quaternion representation. Description of my modifications will be added.

## B. Unity Side

1. Ardity - A package for unity that helps to facilitate serial communication between arduino and unity
2. Management of dispaly through states
3. A "simulation" of a flashlight projected on the wall
4. An internal dataset of predetermined angles that represent the orientation for each animal to show
5. input management to control the application