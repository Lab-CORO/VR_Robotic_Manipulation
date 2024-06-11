# Unity VR project for Robot Realtime Manipulation

This project facilitates the remote control of collaborative robots using virtual reality by creating a digital twin of the real robot within Unity, connected through ROS. The Unity scripts included are largely robot-agnostic, allowing this project to serve as a foundation for future developments. Additionally, the project integrates scripts that connect to ROS for video and point cloud streaming, and features an animated twin of the Robotiq 2F 85 gripper. This project is shared publicly to accelerate the development of digital twins for new robots within Unity for VR projects. The ROS scripts, which are robot-dependent, were created to enable real-time manipulation of the robots based on VR input from the Unity project.

# Installation Instruction

## Prerequisites

- **Operating System**: This project was developed using Unity 2021.3.28f1 on Windows 11 and ROS Noetic on Ubuntu 20.04.
- **Hardware**: Two separate computers are recommended; one for VR and another for robot control.


## Unity Part

To install the Unity project, import the VR_Zorg_Etude folder and open it with Unity Hub.

## ROS Part
This project incorporates multiple different Git repositories due to its development involving several robots. Installation of the ROS components varies depending on the robot used, as each robot has its unique requirements and ROS workspace setups. The scripts in the Robot_scripts folder need to be imported into the workspace of your robot. **Make sure you have created a new ROS workspace for this project**.

* **Doosan Robot**: For real-time manipulation, build from a specific branch on their GitHub. This branch is tailored for real-time manipulation and can be found [here](https://github.com/ETS-J-Boutin/doosan-robot_RT). For detailed setup instructions, refer to this [issue](https://github.com/doosan-robotics/doosan-robot/issues/99) which also explains the procedure.
  * The ROS launches and scripts required to work with a Doosan robot are found [here](https://github.com/Lab-CORO/vr_unity_ros_doosan).
* **Kinova Robot**: Access the repository [here](https://github.com/Kinovarobotics/ros_kortex). Installation procedures are provided on the main page of the repository.
* **Flexiv Robot**: The real-time control API for Flexiv requires C++, whereas the other robot used Python. Follow the C++ RDK installation instructions [here](https://github.com/flexivrobotics/flexiv_rdk).

## Additional ROS Packages:

* **Unity-ROS Bridge**: This can be found at the [Unity Robotics Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub) and is used to connect Unity to ROS. For tutorials and guidance on setting up ROS in Unity, including the ROS-TCP-Endpoint, follow their [ROS_setup tutorial](https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/pick_and_place/0_ros_setup.md). ROS Noetic was used for this project. **REQUIRED FOR ALL ROBOTS.** Note: you will need to change the first line of code of the file ROS-TCP-Endpoint/src/ros_tcp_endpoint/default_server_endpoint.py to
  ```bash
  #!/usr/bin/env python3
  ```
  This make sure that the Endpoint works with Pyhton 3.

* **Robotiq 2F 85 Gripper**: An updated fork compatible with ROS Noetic is available [here](https://github.com/alexandre-bernier/robotiq_85_gripper).
* **Realsense ROS**: Driver for Realsense camera can be found [here](https://github.com/rjwb1/realsense-ros).

Scripts are organized by robot type. Simply add the folder to your workspace for Python scripts. For the Flexiv robot, which uses C++, include the standard CMakeLists.txt required for C++ in ROS.

# Usage

This project was specifically created for an experiment featured in an article. Consequently, all the text, instructions, and menus are in French. However, all the code is written and commented in English.

## Scenes

Three scenes are available within the Unity project:
* **VR Tutorial Room**: Designed to introduce VR to new users. This scene explains different controller inputs and how to navigate within a scene.
* **Test Scene**: Created specifically to test functionalities. It can be removed or retained for the same purpose.
* **Doosan Control Room**: Features a Doosan M1013 robot that can be manipulated using VR. This scene includes a UI that manages VR inputs, robot control, and camera control. It is already set up for VR control.

## Scripts

Scripts are organized into folders based on their functionality:

* **Actions**: Scripts that perform actions on GameObjects within the scene.
* **Physics**: Scripts that either simulate physical behaviors or require physics (e.g., spring effects) to function.
* **Robots**: Scripts that affect how imported robots behave. These are primarily used to generate the digital twin and perform robot-specific actions.
* **ROS**: Scripts that interact with ROS. RosPub scripts are publishers, and RosSub scripts are subscribers.
* **Test Scripts**: Temporary scripts for testing purposes. These can be removed if unnecessary.
* **Tutorial**: Scripts used exclusively in the Tutorial scene.
* **UI**: Scripts that specifically interact with UI GameObjects or components.
* **XR**: Scripts used to modify or control VR-specific interactions, such as adding actions for certain VR inputs.

## Overall usage
The Unity project is designed to work with any robot to produce a digital twin that will sync with a joint state and replicate its movements in the robot.

### Setting Up Your Robot in Unity
1. **Add the URDF Folder**: Place your robot's URDF folder within the project's URDF folder to maintain structure. You can choose another location if desired.
2. **Import Your Robot**: Use the URDF importer to add your new robot to the project.
3. **Configure Articulation Body**: In the first ArticulationBody (usually base_link), set it to immovable to prevent the robot from falling due to Unity's gravity.
4. **Adjust Physical Parameters**: On the first GameObject in the hierarchy or the one containing the Controller component, set the stiffness, damping, and force limit parameters to prevent the robot from collapsing. Typical values used are 10000 for stiffness, 100 for damping, and 10000 for force limit, although adjustments may be necessary based on the robot model.
5. **Add JointsController Script**: Attach this script to the same GameObject and populate the Joints List variable with all the links of your robot, in the order they appear in your robot's joint state message.
6. **Setup ROS Subscriptions**: On the same GameObject or a new empty one, add the RosSubJointState script. Adjust the Topic Name to match the corresponding ROS topic and link the GameObject containing the JointsController in the Joints variable.
7. **Configure ROS Settings**: Navigate to Robotics -> ROS Settings in Unity and ensure the ROS IP Address matches your ROS Master and that you are on the same network.
8. **Launch ROS-TCP-Endpoint**: Start your robot's driver and launch the ROS-TCP-endpoint:
  `roslaunch ros_tcp_endpoint endpoint.launch`
9. **Start Unity Play Mode**: After a few seconds, Unity should connect to ROS, and the digital twin should align with the actual robot's position.

### Real-Time Manipulation (Example with Doosan)

To enable real-time manipulation with the Doosan robot, follow these steps to set up and launch the necessary ROS nodes and scripts:

1. **Setup ROS Publishing**: 
   - In a new GameObject, add the `ROSPubRTTwist` scripts.
   - Configure the Topic name to the required one. This script sends `Twist.msg` to ROS.

2. **Adjust Speed Control**: 
   - Set the speed coefficient to your desired value for proportional control of the speed.

3. **Prepare the End Effector**: 
   - On the end effector of your robot, create a duplicate and make it grabbable by VR by adding an `XRGrabInteractable` component. Set its Layer to `Robot Interactor` to allow grabbing without interference.

4. **Reference Object Setup**: 
   - Update the Objects References for the end effector and its duplicate in the GameObject with the `ROSPubRTTwist` scripts. Place the duplicate in the `Target` slot as it will be the one that moves.

5. **Limit Movement**: 
   - Set the `Height reference` to the end effector, and the `Minimal Height` to the lowest point you want the robot to move in Z. This helps prevent collisions.

6. **ROS Launch Commands**: 
   - Launch the various ROS components needed for real-time manipulation using the following commands in multiple command window:
   ```bash
   roslaunch dsr_ros_control doosan_interface_moveit.launch # Launch the ROS driver for real-time control.
   roslauch moveit_servo spacenav_cpp.launch # Adjust config to match your robot. See MoveIt tutorials for setup.
   roslaunch unity_ros_doosan vr_realtime.launch # Launches camera, gripper, and conversion scripts. Permissions for the gripper may need adjustment (`sudo chmod 777 /dev/ttyUSB0`).
   roslaunch ros_tcp_endpoint endpoint.launch # LAunch the Unity-ROS bridge
   rosrun rqt_controller_manager rqt_controller_manager # Switch Doosan controller to velocity control.


# Contributing

We welcome contributions to our project. If you have suggestions or improvements, please fork the repository and submit a pull request. For substantial changes, please open an issue first to discuss what you would like to change. Please ensure to update tests as appropriate.
