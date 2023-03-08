# Tapetum

Tapetum is an application that is part of project Midbarium. It is an eductional attraction in which visitors direct a flashlight towards stickers representing animal eyes to learn which animal these eyes belong to.
The application is built on top of unity. It relies on serial communication from an arduino controller in order to function.
The arduino controller is connected to a 9DOF (9 Degrees of Freedom) sensor that uses an accelerometer, a magnetometer, and a gyroscope in order to approximate orientation in 3D Space.

## Preface
This README ommits some information that is already covered in the "forked repository" which is linked below. That's a good place to check if you feel that I failed to expand on some concept.
I will also assume familiarity with Arduino, Unity and Serial Communication.
Hopefully I have managed cover all the information you will need, but I might (probably) have missed some key details. Feel free to open issues telling me how bad my documantation is if that's the case.

## Terminology:
- AHRS - stands for "Altitude and Heading Reference System". It is a type of device that utilizes the aforementioned sensors in order to measure orientation and direction, primarly in aircrafts.
- Declination - a property of magnetic readings the relates to geographic location, and also changes over time. As of writing these lines, magnetic declination in israel revolves around 5 degrees. If you notice obvious drift in accuracy over time, you might need to update this value. For more details, see the filter section below. To get relevant declination, just query "magnetic declination [*Country name*]" into your preferred search engine.

Here are the steps required in order to use this project:

## A. Arduino Side
All programs are taken from the same repo, which i have forked and somewhat modified - [My Fork](https://github.com/FailedSuccessfully/LSM9DS1-AHRS)
Unless stated otherwise, the files I will mention live in the forked repo.

In order for the readings to be accurate, it is necessary to callibrate the sensors before use. It might be necessary to perform calibration as routine maintainance.

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
In this repository there is a precompiled exe of magneto. To use it, run the exe to open a command line interface. Type in the path to the relevant CSV file, and when it asks you how to proceed, choose the default option. The data will be output into the console interface. It will then ask you if you want to save the modified readings, which isn't needed and can be skipped.
Do this for a_cal.csv and m_cal.csv
If you find the need to compile Magneto yourself, the source code is in the forked repo.
- 3. Custom AHRS filter -
The forked repository contains two variations on the Mahony AHRS filter, both implmented on top of Sparkfun LSM9DS1 arduino library.
The UW version is the one which I modified for this project. These changes aren't reflected in the other version, so take that into account if you use it for some reason.
The most meaningful changes are:
1. I skip Yaw, Pitch and Roll calculations, as those are derived from quaternions, which makes them redundant. Writing out the quaternion value allows us to more easily parse it in unity and helps us avoid nasty issues like gimbal lock.
2. Management and reporting of idle\awake state. This allows us to turn on the flashlight (via the relay unit) when movement occurs, and turn it off if it hasn't been moved for a determined time.
3. I perform several reads, between each write and avarage the results. This helps produce a more stable and accurate behavior over unity. I believe it can be further improved, but it wan't necessary as the reading was accurate enough.

Before you compile and upload the program to the controller, you will need to set some variables using the data we collected:
- *G_offset* - Replace the numbers inside the curly brackets to those produced in the first step (g_cal.csv).
- *A_B* and *A_Ainv* - replace the **acceleriometer** data from step 2.
- *A_M* and *M_Ainv* - replace the **magnetometer** data from step 2.
- *declination* - verify declination is representative of location.

Once you compile and upload the program successfully, the arduino controller is ready to communicate with our unity player.

## B. Unity Side

In unity, we receive serial messages from the arduino controller inform and give feedback to the player. The messages are constructed as to contain iformation about both orientarion and state in a single message. In unity, our code will parse this message and trigger events when relevant.

1. **Ardity** - A package for unity that helps to facilitate serial communication between arduino and unity via components.
Ardity opens and maintains a serial port connection, allowing us to easily read serial messages and parse them in our own scripts.
2. **Management of dispaly through states** - Several application states are defined, and are managed through a static calss which is encapsulated within the *TapetumController Monobehaviour*. Take note that, somewhat unwisely, the application state and the flashlight state are managed independantly of each other, which may lead to conflicts.
3. **A "simulation" of a flashlight projected on the wall** - we have two objects in the scene to represent the flashlight and the wall. The psuedo flashlight listens to OnRotation event, which invokes with the parsed quaternion as to translate the orientation from real space to simulated space. It can also maintain a rotation offset if needed. It then passes it's rotation to the tapetum controller, which determines if an animal is within angular range to be considered in scope. Admittably, writing this now I can see this step is redundant and convoluted and probably adds some latency overhead. It helped in presenting the application to the client, but it could be a good idea to bypass the whole thing completely, if time would have it.
4. **An internal dataset of predetermined angles that represent the orientation for each animal to show** - We store an array of Animal type, which is a scriptable object with the animal's picture and some positioning data. When we check if an animal is in scope, we order the array by the relative angle between the projector and the animal member. We the retereive the first member of the ordered array, and check to see if angle between them and the flashlight does not exceed a minimum threshold. If the condition is met, the animal is captured and application state is considered in scope.
5. **input management to control the application** - This allows some control over the application. Namely - 
- Use ESC button to close application
- Use R Button in order to offset the simulated flashlight, so that current orientation is treated as if it were identity.
- Use numbers 1-n (n being the animal count) in order to save current flashlight orientation as the position for the selected animal. This changes will persist unless manually reverted.
- Use D Button to revert all animals to their saved defaults. These defaults are defined within the scriptable objects.
