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

### 1. Calibration Script - 
A short calibration process with an arduino program. Output needs to be captured and saved for step 2
2. Magneto - Running magneto on magnetometer and acceleromter readings, the output needs to be copied into the arduino code that handles approximation.
3. Custom AHRS filter - a somewhat modified version of the Mahony AHRS filter that takes sensor data and converts it to quaternion representation. Description of my modifications will be added.

## B. Unity Side

1. Ardity - A package for unity that helps to facilitate serial communication between arduino and unity
2. Management of dispaly through states
3. A "simulation" of a flashlight projected on the wall
4. An internal dataset of predetermined angles that represent the orientation for each animal to show
5. input management to control the application