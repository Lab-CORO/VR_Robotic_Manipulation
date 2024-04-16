# Unity VR project for Robot Realtime Manipulation

This project facilitates the remote control of collaborative robots using virtual reality by creating a digital twin of the real robot within Unity, connected through ROS. The Unity scripts included are largely robot-agnostic, allowing this project to serve as a foundation for future developments. Additionally, the project integrates scripts that connect to ROS for video and point cloud streaming, and features an animated twin of the Robotiq 2F 85 gripper. This project is shared publicly to accelerate the development of digital twins for new robots within Unity for VR projects. The ROS scripts, which are robot-dependent, were created to enable real-time manipulation of the robots based on VR input from the Unity project.

# Installation Instruction
## Unity Part

To install the Unity project, import the VR_Zorg_Etude folder and open it with Unity Hub.

## ROS Part
This project incorporates multiple different Git repositories due to its development involving several robots. Installation of the ROS components varies depending on the robot used, as each robot has its unique requirements and ROS workspace setups. The scripts in the Robot_scripts folder need to be imported into the workspace of your robot.

* **Doosan Robot**: For real-time manipulation, build from a specific branch on their GitHub. This branch is tailored for real-time manipulation and can be found [here](https://github.com/BryanStuurman/doosan-robot/tree/ros_control_compat). For detailed setup instructions, refer to this [issue](https://github.com/doosan-robotics/doosan-robot/issues/99) which also explains the procedure.
* **Kinova Robot**: Access the repository [here](https://github.com/Kinovarobotics/ros_kortex). Installation procedures are provided on the main page of the repository.
* **Flexiv Robot**: The real-time control API for Flexiv requires C++, whereas the other robot used Python. Follow the C++ RDK installation instructions [here](https://github.com/flexivrobotics/flexiv_rdk).

## Additional ROS Packages:

* **Unity-ROS Bridge**: This can be found at the [Unity Robotics Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub) and is used to connect Unity to ROS. For tutorials and guidance on setting up ROS in Unity, including the ROS-TCP-Endpoint, follow their [ROS_setup tutorial](https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/pick_and_place/0_ros_setup.md). ROS Noetic was used for this project. **REQUIRED FOR ALL ROBOTS.**
* **Robotiq 2F 85 Gripper**: An updated fork compatible with ROS Noetic is available [here](https://github.com/alexandre-bernier/robotiq_85_gripper).

Scripts are organized by robot type. Simply add the folder to your workspace for Python scripts. For the Flexiv robot, which uses C++, include the standard CMakeLists.txt required for C++ in ROS.
