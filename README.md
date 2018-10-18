# MayaVerse Updated Alfa Versione 0.3

MayaVerse is an experimental Metaverse - Immersive Virtual World

This document describes the development and creation of the first Immersive Virtual World. MayaVerse will be a fully immersive computer recreated universe. It will be developed using the best technologies available to overcome the current paradigm of the "Immersive Virtual Worlds" and launch the user in a new vision.
What will be outlined is actually an idea, a project that we hope will soon become a reality. We believe il will be soon a reality, hoping that drawing a plot and describing what are the possibilities will encourage further progress and implementations.

Let's start from the correct definition of "MayaVerse":
3D immersive virtual environment recreated by computer, shared with multiple users over the Internet with a persistent possibility of insertion , modification and deletion of objects and agents and direct interaction through virtual devices. Furthermore programmable through a scripting language.
The above definition only partially clarifies the possibilities offered by a Metaverse. Here, then, is a brief "manifesto" that can detail more the characteristics of the MayaVerse project.
A micro manifesto for the perfect "Metaverse":

* Immersive Virtual Reality: Immersive Virtual Reality, designed for a sense of "presence" and "interaction" at a high level. One should not think of a simple HMD viewer connected to an environment recreated on the computer, but of a platform that "involves" the user and above all gives him the possibility of interacting with other users, with objects and also with automatic systems or systems guided by Artificial Intelligence.

* Open Source/Royalty Free protocols, standards, queues: the entire Metaverse source code should be released under Open Source lincense, but above all based on standards and non-proprietary protocols, open and certified by international bodies. Examples are the VRML standards and its evolution X3D which, however, after a period of good diffusion, have been "forgotten" by the IT industry. Recently, Carmack has also underlined this possibility with the creation of an open standard for describing a 3D scene and its transfer through the internet via glTF.

* Sandboxing User Generated contents In-World/Out-World: the ability to offer users tools for creating and editing any content within the virtualized environment is essential. The ability to import creations from other systems and software is also a goal that amplifies users' creativity and offers an unprecedented level of interaction. All current "Metaverse" offer this opportunity at various levels, which can then be specialized to create games, experiments and artistic forms of all kinds.

* Scripting system using software language as Javascript, C#, Go, Swift: directly correlated with the previous point is the need to program the entire environment through high-level languages. C#, Go and Swift seem to be perfect candidates even though the scene is currently dominated by Javascript. However, it is essential to give developers the option of creating links, automatically modifying the environment and interfacing it with other systems and platforms.

* Metaverse as protocol not only as API: it is quite obvious to think that the Metaverse exposes APIs, but it is even more important to assume and design a real protocol that allows different Metaverses to communicate with each other. . In the past, there have been several attempts such as MMOX, which has failed to overcome the draft state of the IETF.

* Crypto Currency inside Metaverse: Why not use Bitcoin or Ethereum directly? Despite many ads, still no Metaverse uses Crypto-currencies any kind to allow trade between users. This feature would instead make the platform unique and above all would involve the user more closely.

* Decentralized architectures: Almost all platforms for virtual worlds are based on classic client-server architecture. Moving to a peer-to-peer system would have enormous advantages. JanusVR, for example, allows you to use the incredible ipfs (Inter-Planetary File System) for the fast recovery of different assets.

Implementation of the Alfa version.

The software is currently only developed for Windows platform.

The current Client system is based on SW:

3D Graphics Engine Config:

Versions
  Unity: 2017.4.13f1
  
  VRTK: 3.3.0 (may not be correct if source is GitHub)
  
  SteamVR: 1.2.3
  
VR Settings
  Standalone: None, OpenVR
Scripting Define Symbols
  Standalone: VRTK_DEFINE_SDK_STEAMVR;VRTK_DEFINE_STEAMVR_PLUGIN_1_2_1_OR_NEWER;VRTK_DEFINE_STEAMVR_PLUGIN_1_2_2_OR_NEWER
  WSA: VRTK_DEFINE_SDK_WINDOWSMR
  WSA: VRTK_DEFINE_SDK_WINDOWSMR

See list below

All server-side code relies on opensource services and software for Linux Server.
The tests were carried out on the following HW VR:

 - HTC Vive 1.2 
 - Oculus Rift CV1
 
## Licenses

 - **Only the code in `Assets/Scripts` is released under the MIT license**
 - Other code libraries in `Assets/Libraries` all come with their own licenses
 - Any binary resources are more or less there on a "fair use" basis, **don't assume that you can just copy and use them**

## Useful links:
Configuration git:
[Link to file .gitignore](https://gist.github.com/Shogan/dad6786c58c5ad88e0ec)

## Open source Assets List ##

- https://github.com/thestonefox/VRTK (Commit: d88c9eafc31975bb9566e5747c12e48e5f75fa3d) (MIT)
- https://github.com/ValveSoftware/steamvr_unity_plugin (BSD 3-Clause)
- https://github.com/KhronosGroup/UnityGLTF (MIT)
- https://github.com/atteneder/glTFast (MIT)
- https://github.com/AKuellar/LSky-v2 (MIT)
- https://github.com/BananaHemic/Mumble-Unity (MIT)
- https://github.com/dentedpixel/LeanTween (MIT)

### Tools ###

- https://github.com/gportelli/UnityScreenLogger (MIT)
- https://github.com/Tayx94/graphy (MIT)
- https://github.com/sampaiodias/unity-cli (NOT USED BUT INCLUDED) (Creative Commons 4.0)
- https://github.com/stillwwater/command_terminal (MIT)
- https://github.com/densylkin/QuickPool (MIT)
- https://github.com/gportelli/UnityScreenLogger (MIT)

### Networking ###

- https://github.com/DarkRiftNetworking/Hazel-Networking (MIT)
- https://github.com/Vytek/HazelTestUDPClientUnity (MIT)
- https://github.com/Vytek/HazelTest
- https://google.github.io/flatbuffers/ (APACHE 2.0)
- https://github.com/PimDeWitte/UnityMainThreadDispatcher (APACHE 2.0)
- https://github.com/Vytek/MVUpdateDB
- https://github.com/mbdavid/LiteDB (MIT)

## Free Assets ##

- https://assetstore.unity.com/packages/tools/particles-effects/bfw-simple-dynamic-clouds-85665
- https://assetstore.unity.com/packages/tools/particles-effects/nvidia-vrworks-83505
- https://avatarsdk.com/

--

See project wiki for info: https://github.com/Vytek/MayaVerse03/wiki

[![ko-fi](https://www.ko-fi.com/img/donate_sm.png)](https://ko-fi.com/V7V4EXM8)
