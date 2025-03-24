# ⚠️ Archived Repository ⚠️

This repository is no longer maintained and has been **archived**.

The project has moved to a new repository. You can find the latest version and updates here: [UMI3D-BROWSER](https://github.com/UMI3D/UMI3D-BROWSER).

Please visit the new repository for the most up-to-date code and information.

# UMI3D-OpenVR-Browser
UMI3D is a web protocol that enables the creation of 3D media in which users of any AR/VR device can collaborate in real time. The 3D media is created once and hosted on a server or on a local computer. Any AR/VR device can display and interact with it remotely thanks to a dedicated UMI3D browser.

This git is an implementation of an UMI3D SteamVR browser with [Unity](https://unity.com) for Windows

For more information about UMI3D, visit the [UMI3D Consortium's website](https://umi3d-consortium.org)

### Version And Documentation

The Current UMI3D-SDK version is 2.1.
The documentation can be found [here](https://umi3d.github.io/UMI3D-SDK/index.html).

* [The UMI3D SDK for Unity](https://github.com/UMI3D/UMI3D-SDK)
* [Server Samples Scenes](https://github.com/UMI3D/UMI3D-Samples)

## Installation :

1. Clone this repository on your desktop.
2. This Unity project requires the Steam VR Plugin. So after opening the project within Unity install it from the [Asset Store](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647). After its installation, if you are asked to install the legacy VR mode or the Unity XR plugin, click on "Cancel".
3. Next, move the “SteamVR” folder in another folder named “Assets/AssetStoreTools”.  
4. Then go to Windows -> SteamVR Input. Click on “Save and generates”.
5. Restart Unity.
6. Go to Edit -> Project Settings -> Open VR and make sure that "Stereo rendering mode" is set to "Multi Pass".
6. You are now ready to use this project !

## Getting Started

* Launch a Server (Server sample scenes can be found [here](https://github.com/UMI3D/UMI3D-Samples))
* The Ip and HttpPort of the server can be found on the "UMI3DCollaborationServer" script (usualy on the "Server" node)
* Launch the Browser Unity Project.
* Launch the "StartScene" Scene and fill up the Ip and the Port.
* Click on "Connection"
