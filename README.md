![SONIA logo](https://github.com/HealthX-Lab/SONIA/blob/main/Assets/Materials/SONIA.png)

# SONIA

*immer**S**ive m**O**dular **N**euro learn**I**ng pl**A**tform*



## Description

Modular system for displaying and inspecting brain structures, connections, and subsystems. Built over the course of 4 months by Research Assistant Owen Hellum at Concordia University for Health-X Lab. The project was supervised by Dr. Yiming Xiao, Assistant Professor in the Department of Computer Science and Software Engineering at Concordia University, and Principal Investigator at Health-X Lab. The systems were designed for Virtual Reality (VR) in Unity with SteamVR and a HTC VIVE Pro Eye HMD (head-mounted display).

## Features

- Modular design
- Novel visualization methods
- Fine-tuned use of colour and spatiality for user experience
- Able to load and visualize high volumes of complex data

## Requirements

### Development:
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
A built version (without TTS voiceover) can be downloaded from the [releases page](https://github.com/HealthX-Lab/SONIA/releases).

## How To Use

If downloading for development, the following factors should be considered:
- New atlases/datasets can be added by placing the required files in their own folder in the `Assets/Resources` folder
	- For a description of the required files, please consult the `MiniBrain.cs` script in the `Assets/Scripts` folder
- In the `Main Scene` scene, the `MiniBrain.cs` script can be found attached to two example GameObjects that are children of the `SteamVR CameraRig` GameObjects
	- The active GameObject is loaded with a custom atlas of anxiety response structures/connections
	- The inactive GameObject is loaded with the AAL atlas (may present errors, as it has only mildly been tested)
- To run the scene with a newly added atlas, simply duplicate the active Mini Brain GameObject (making sure to disable it aftrwards), and set the required variables in the `MiniBrain.cs` script
- All scripts are fully documented, and can provide useful information, should an error occurr

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
