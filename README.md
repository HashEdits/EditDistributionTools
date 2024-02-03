


# EditDistributor 🗳️

This is a tool to automate and the ease the distribution of modifications of Unity 3D moddels

![ ](https://cdn.discordapp.com/attachments/337986548664500236/1203413914616856576/image.png)

Due to how curent 3D model files are set up there isn't a way to share shape-keys without being tied to a mesh. Making the process of sharing such files rather tedious or to be left to the context of a commision where sharing mesh data is in general allowed between the commisioner and the artist.


After being involved in the scene it was clear that the previous mainstream methods were sub optimal: overwriting the original model and requiering the user to navigate to a subfolder unknown in advanced.

My piece of software let's you build a patcher with ease, saving you time and letting your customers have a reliable path to navigate to. Building a reliable way for them to interact with your products.
It also now supports adding a directory with your different localized descriptions and readme to make the process of putting the avatar on the storefront much less painfull

I am going to be using my tool from now on to build my Face Tracking Add-ons avaliable on my [Booth](https://hashedits.booth.pm/) and [Ko-Fi](https://ko-fi.com/hashedits/shop) shops

If you end up using the tools please credit my socials and this GitHub Page

## Pre-requisites 🤓

A Unity project

A 3D moddel

A modification of that 3D moddel

## How to use 😎
  
  - Make sure both your model and your face tracked model are set up in a unity project
  - (optional) put all of your descriptions and readme in a folder to ease the creation of pages
  
- Open your python IDLE
(the windows taskbar is your friend)
- Click File -> Open

- Navigate to where you have downloaded the repo -> FT_Builder

- Press F5

- Follow the instructions

## Exemple 📑
You'll find an exemple readme in **Description&ReadMeExemples** that you can bundle with your avatar's package to let the customer know what to do with it.
There's also an exemple description there.
in summary any of the following:

`/*AVATAR NAME*/`,`/*AVATAR AUTHOR*/`, `/*StoreLink*/`, `/*PACKAGE NAME*/`, `/*DIR PATCHER*/`, `/*DIR PREFAB*/`

will be remplaced by something that makes sense in for your project

## Updater 🔧

You may want to update your creation in the future, instead of having to create a whole new patcher everytime I created an Updater tool



## Upcoming features

I've tried setting up a board for y'all to see what I'd like to add to flesh out my face tracking tools.

[Here's a link to it](https://github.com/users/HashEdits/projects/1/views/1)




## Credits 📕

**[VRCFT](https://github.com/benaclejames/VRCFaceTracking)**

**[HDiffPatch](https://github.com/sisong/HDiffPatch)**

**[StackEdit](stackedit.io)**
