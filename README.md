![SONIA logo](https://github.com/HealthX-Lab/SONIA/blob/main/Assets/Materials/SONIA.png)

# SONIA

*immer**S**ive cust**O**mizable **N**euro learn**I**ng pl**A**tform*

**Publication:** O. Hellum, C. Steele, Y. Xiao, “SONIA: an immersive customizable virtual reality system for the education and exploration of brain networks,” arXiv: 2301.09772, 2023.



## Description

Customizable system for displaying and inspecting brain structures, connections, and subsystems. Built over the course of 4 months by Research Assistant Owen Hellum at Concordia University for Health-X Lab. The project was supervised by Dr. Yiming Xiao, Assistant Professor in the Department of Computer Science and Software Engineering at Concordia University, and Principal Investigator at Health-X Lab. The systems were designed for Virtual Reality (VR) in Unity with SteamVR and a HTC VIVE Pro Eye HMD (head-mounted display).



## Features

- Customizable design
- Novel visualization methods
- Fine-tuned use of colour and spatiality for user experience
- Able to load and visualize high volumes of complex data



## Requirements



### Development:
- Basic knowledge of Unity scene and GameObject navigation
- Unity version 2021 or later (though it would probably work with earlier versions back a couple years)
- SteamVR
- HTC VIVE Pro Eye HMD (though other VR headsets can theoretically also be used, I have not tested this)



### Build:
- Windows 10/11
- SteamVR
- HTC VIVE Pro Eye HMD (though other VR headsets can theoretically also be used, I have not tested this)



## Installation



### Development:
The full package can be downloaded from the [GitHub repository](https://github.com/HealthX-Lab/SONIA).



### Build:
A built version (without TTS voiceover, with the AAL atlas loaded) can be downloaded from the [releases page](https://github.com/HealthX-Lab/SONIA/releases).



## How To Use

If downloading for development, the following factors should be considered:
- In the `Main Scene` scene, the `MiniBrain.cs` script can be found attached to two example GameObjects that are children of the `SteamVR CameraRig` GameObjects
  - The active GameObject is loaded with a custom atlas of anxiety response structures/connections
  - The inactive GameObject is loaded with the AAL atlas (may present errors, as it has only mildly been tested)
- All scripts are fully documented, and can provide useful information, should an error occur
- To run the system with a customized atlas of your own, please see the following section
  - ***While the text for the optional tutorial is customizable, the in-code trigger points are not. For this reason, the use/customization of a tutorial is not recommended***



## Customizable System Setup



### Required files

- Structure model files (`.fbx`)
- Structure info file (`.txt` or `.csv`) (`|` delimited)
- Structure connectivity file (`.csv`) (`,` delimited)
- *Optional subsystem files:*
  - Subystem info file (`.txt` or `.csv`) (`|` delimited)
  - Subsystem connectivity file (`.csv`) (`,` delimited)
  - Subsystem connection description file (`.txt` or `.csv`) (`|` delimited)
- *Optional extra structure files:*
  - Extra structure model files (`.fbx`)
  - Extra structure connectivity file (`.csv`) (`,` delimited)
- *Optional tutorial files:*
  - Tutorial text file (`.txt`)



### Folder structure

1. Navigate to the `SONIA/Assets/Resources` folder
2. Create a folder, and name it accordingly
3. Place all required and optional files ***(except for extra structure files)*** loosely inside of the new folder *(do not create any subfolders within this new folder)*
4. If using extra structure files, create another folder in the `SONIA/Assets/Resources` folder, and name it accordingly, and place the extra structure files loosely inside ***(including the extra structure connectivity file)*** *(do not create any subfolders within this new folder)*



### Configuring scripts

1. Open the `Main Scene` scene in the `SONIA/Assets/Scenes` folder
2. Navigate to the `Mini Brain` GameObject (child of the `SteamVR CameraRig` GameObject)
3. Attached to the `Mini Brain` GameObject is the `MiniBrain.cs` script
4. Fill the following fields in the `MiniBrain.cs` script under the **Files** header *(type only the name of the folder/file, not the full path)* *(do not include the file extension)*:
   - **Path:** The name of the first folder you created in the `SONIA/Assets/Resources` folder
   - **Info Path:** The name of the structure info file
   - **Connectivity Path:** The name of the structure connectivity file
   - **Subsystems Info Path:** The name of the subsystem info file
   - **Subsystems Connectivity Path:** The name of the subsystem connectivity file
   - **Subsystems Connectivity Description Path:** The name of the subsystem connection description file
5. Fill the following fields in the `MiniBrain.cs` script under the **Connectivity** header:
   - **Highest Value:** The maximum strength of all connections in the entire connectivity matrix
6. Fill the following fields in the `MiniBrain.cs` script under the **Extra structures** header *(type only the name of the folder/file, not the full path)* *(do not include the file extension)*:
   - **Extra Path:** The name of the ***extra structures*** folder you created in the `SONIA/Assets/Resources` folder
   - **Extra Connectivity Path:** The name of the extra structures connectivity file
7. ***If you wish to include a tutorial***
   1. Navigate the the `Tutorial` GameObject
   2. Enable the `Tutorial` GameObject
   2. Attached to the `Tutorial` GameObject is the `TutorialLoader.cs` script
   3. Fill the following fields in the `TutorialLoader.cs` script *(type only the name of the folder/file, not the full path)* *(do not include the file extension)*:
      - **Path:** The name of the folder you created in the `SONIA/Assets/Resources` folder
      - **Tutorial Path:** The name of the tutorial text file



## Script Diagram

![SONIA script diagram](https://github.com/HealthX-Lab/SONIA/blob/main/Assets/Materials/SONIA_Script_Diagram.png)



## Extras

In the `old` branch, code and scenes from previous iterations of the system can be found. They would most likely have errors, and would require careful debugging to fix the issues. That being said, they are also fully documented.



## External Links

- [Health-X Lab's GitHub](https://github.com/HealthX-Lab)
- [Owen Hellum's GitHub](https://github.com/Owmacohe)
- [Unity](https://unity.com)
- [VIVE developer portal](https://developer.vive.com)
- [VIVE Setup](https://www.vive.com/ca/setup)
