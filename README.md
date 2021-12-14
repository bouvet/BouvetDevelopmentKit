![header-bdk](https://user-images.githubusercontent.com/48277920/145983130-52739f63-b2da-4856-971b-6d05e43008af.png)

# Bouvet Development Kit

The Bouvet Development Kit (BDK) package allows you to easily create applications for the Hololens 2 using Unity Software.

## Supported version

 | [![windows](https://user-images.githubusercontent.com/48277920/145994942-b90d9c8a-60b3-444f-abbd-673434ce6096.png)](https://developer.microsoft.com/windows/downloads/windows-10-sdk) [<p align="center">Windows SDK</p>](https://developer.microsoft.com/windows/downloads/windows-10-sdk)| [![unity](https://user-images.githubusercontent.com/48277920/145994938-f3637380-5050-45b1-a35a-2054229b535e.png)](https://unity3d.com/get-unity/download/archive) [<p align="center">Unity 2020 LTS</p>](https://unity3d.com/get-unity/download/archive)| [![visualstudio](https://user-images.githubusercontent.com/48277920/145994943-eb8bbddc-7b97-4c0e-87d7-129181387c43.png)](http://dev.windows.com/downloads) [<p align="center">Visual Studio 2019</p>](http://dev.windows.com/downloads)| 
| :--- | :--- | :--- | 

BDK has only been tested with the following versions of Unity and Visual studio so far. There are plans to move to Unity 2021 once it enters LTS, using 2021 is on your own risk.

Please refer to the [Install the tools](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/install-the-tools) page for more detailed information.

## Installation via Git in UPM

Open the Unity package manager and navigate to *"Add package from git URL..."*

 <!-- Add image for add from git thing -->

![Package Manager > + > add from Git URL](../../wiki/images/upm-git-add.PNG)

Supply the following URL:

`https://github.com/bouvet/BouvetDevelopmentKit.git`

## Setup project settings

1. Go to build settings `(Ctrl + Shift + B)` and switch to the Universal Windows Platform. Then set target device to HoloLens and target architecture to ARM64.

2. Next go into Player Settings and under Other settings there is a setting called `Active Input Handlig` that should be set to `Input System Package (New)`.

3. Finally, you need to go to the `XR Plug-in Management` tab in the Player Settings and enable `Windows Mixed Reality`.

## Create your scene

To start creating with BDK, you need to add the `BDK Hololens 2 Prefab` ([read more](../../wiki/BDK-Hololens-2-Prefab)) to your scene. You can find it by searching in the packages folder. Next find the `InputManager` game object in the hirarchy of the prefab and enable the input options you wnt to use. We usually use at least `Use Hand Tracking`, `Allow Manipulation`, and `Use Interaction Beams`.

Now you can add some object to your scene and add the `Grabbable`, `Two-Hand-Grabbable`, or `Interactable` scripts to them to start adding in functionality to your project. You can also go to [Samples](../../wiki/samples) and add BDK Essentials to find a few nifty menus and buttons.

## Building for the HoloLens 2

To build your app, click the `build` button in build settings and create a new folder for build output. This will generate a visual Studio solution that you can use to deploy your project. for more info on this see Microsoft's own [guide](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio?tabs=hl2).

Set configuration to release, platform to ARM 64 and run on Device

Connect your Hololens to the PC and go to Debug > Start without debugging
	Visual studio will now build and run your project on the hololens
	
You might have to Set your hololens to developer mode and get a PIN code from it to allow building and running.
	
for more detailed information on building from visual studio, see: https://docs.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio?tabs=hl2

Note, some settings might differ between MRTK and BDK here. chack out this README, the wiki, or ask(issue?, discord?, slack?) if there is anything you are wondering about

## BDK Documentation

BDK documentation is split into 2 categories, Guide and Advanced. The guide is for how to use BDK for you projects, Advanced wiki is a guide on how BDK works and how you can help us develope BDK further.


