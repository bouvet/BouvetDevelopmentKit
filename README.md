# Bouvet Development Kit

The Bouvet Development Kit (BDK) package allows you to easily create Mixed Reality applications for the Hololens 2.

## Supported version

BDK has only been tested with the following versions of Unity and Visual studio so far. There are plans to move to Unity 2021 once it enters LTS, but unitll then support will only be experimental.

- Visual studio 2019
- Unity 2020.4 LTS

## Installation via Git in UPM

Open the Unity package manager and navigate to *"Add package from git URL..."*

 <!-- Add image for add from git thing -->

![Package Manager > + > add from Git URL](../../wiki/images/upm-git-add.PNG)

Supply the following URL:

```none
https://github.com/bouvet/com.bouvet.developmentkit.git
```

## Setup project settings

Go to build settings `(Ctrl + Shift + B)` and switch to the Universal Windows Platform. Then set target device to HoloLens and target architecture to ARM64.

Next go into Player Settings and under Other settings there is a setting called `Active Input Handlig` that should be set to `Input System Package (New)`.

Finally, you need to go to the `XR Plug-in Management` tab in the Player Settings and enable `Windows Mixed Reality`.

## Create your scene

To start creating with BTK, you need to add the `BDK Hololens 2 Prefab` ([read more](../../wiki/BTK-Hololens-2-Prefab)) to your scene. You can find it by searching in the packages folder. Next find the `InputManager` game object in the hirarchy of the prefab and enable the input options you wnt to use. We usually use at least `Use Hand Tracking`, `Allow Manipulation`, and `Use Interaction Beams`.

Now you can add some object to your scene and add the `Grabbable`, `Two-Hand-Grabbable`, or `Interactable` scripts to them to start adding in functionality to your project. You can also go to [Samples](../../wiki/samples) and add BTK Essentials to find a few nifty menus and buttons.

## Building for the HoloLens 2

To build your app, click the `build` button in build settings and create a new folder for build output. This will generate a visual Studio solution that you can use to deploy your project. for more info on this see Microsoft's own [guide](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio?tabs=hl2).

<!-- 
Set configuration to release, platform to ARM 64 and run on Device

Connect your Hololens to the PC and go to Debug > Start without debugging
	Visual studio will now build and run your project on the hololens
	
	You might have to Set your hololens to developer mode and get a PIN code from it to allow building and running.
	
	for more detailed information on building from visual studio, see: https://docs.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio?tabs=hl2

	Note, some settings might differ between MRTK and BDK here. chack out this README, the wiki, or ask(issue?, discord?, slack?) if there is anything you are wondering about
 -->
