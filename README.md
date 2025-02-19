



# EditDistributor 🗳️

This is a tool to automate and ease the distribution of modifications of Unity 3D models.



<p align="center">
  <img src="https://raw.githubusercontent.com/HashEdits/EditDistributionTools/main/ReadMeFiles/header.png" alt="Overview of the tools"/>
</p>


This set of tools enables you to distribute your modifications to any 3D models with ease.

Its main use case is to generate a difference between your modifications on a 3D model and an original 3D model created by a third party, which you don't have the rights to distribute the original data.


My set of tools lets you build a patcher and maintain it with ease, saving you time and letting your customers have a reliable way to install your modifications, building a reliable way for them to interact with your products.

The updater tool lets you maintain your modifications if the original creator updates their model.

An option is also avaliable to automate the generation of localized descriptions for your storefronts.


This tool is used mainly by the avatar creators from [VRCFT](https://github.com/benaclejames/VRCFaceTracking) to distribute face tracking add-ons/DLC.

⚠️**If you end up using the tools please credit my socials and this GitHub Page**⚠️

## Pre-requisites 🤓

A Unity project

A 3D model

A modification of that 3D model

## How to use 😎
  
[![da tutorial](https://raw.githubusercontent.com/HashEdits/EditDistributionTools/main/ReadMeFiles/distribution_tools_thumbnail.png)](https://youtu.be/Scs3xrX7tCA?si=SA96iIs4U9auXUEA)

  - Make sure both your model and your face tracked model are in your unity project

  - Make sure your modified model is stored in the following file structure:
  `Assets/YourName/NameOfTheAvatar/fbx/YourCoolModel.fbx`
  
- Access the builder in your toolbar at
`Tools/Hash/EditDistributor/Builder`

- Drag and drop the prefab of the original model

- Drag and drop the prefab of the modified model

	- (optional) change the distribution name if you don't want to use the name of the original model

- Enter your name

- Enter the name of the original package that end users will import in their projects

	- (optional) gather up your descriptions and readme files in a folder to ease the creation of the store pages pages and fill in the relevent information

- Hit the `Build` button

## Builder🔧

<p align="center">
  <img src="https://raw.githubusercontent.com/HashEdits/EditDistributionTools/main/ReadMeFiles/Builderwindowindexwithnumbers.png" alt="Builder Window"/>
</p>

Number| Name| Description
-------- | -----| -----
1| Original model slot| A slot to drag and drop a prefab of the original model into (requires the moddel to have an avatar component for now) |
2| Modified model slot |  A slot to drag and drop a prefab of your modified model into (requires the moddel to have an avatar component for now) |
3|Overwrite Checkbox  | Will use the name of your modified model to created the patcher script and your menu bar if left unckecked. Will use the OverwriteName if checked
4| OverwriteName  | Name that will be used for the patcher script and hotbar menu if the overwrite checkbox is ticked
5| Your Name | Will be used to organize your patchers in the menu bar
6| Original package name | Will be used to let your user know which version of the original model they're expected to import. Very crucial to give them some sort of version number here that will resonate with your user.
7| Generate descriptions checkbox | (`Optional`) Will enable the description generator feature
8| Description folder selection button | This button will prompt you to select a description folder. All .txt files will be scanned and copied in the destination folder after hitting patch if the Generate descriptions checkbox is checked
9| Destination folder selection button | This button will prompt you to select a destination folder. All files found in the description folder will be coppied in this directory with the tags changed
10| Creator name | All `/*AVATAR AUTHOR*/` tags will be replaced by what you put in there 
11| Store page | All `/*StoreLink*/`  tags will be replaced by what you put in there
12| Build button | Will generate a patcher script, the diff files and the descriptions (if you've checked the option)


## Description generator tags 📑
You'll find an example readme in [here](https://github.com/HashEdits/EditDistributionTools/tree/main/demo%20packages/SampleDescriptions) to show you how to write your descriptions
and another one [there](https://github.com/HashEdits/EditDistributionTools/tree/main/demo%20packages/MyCubeDescriptions) to show you how they look like after everything's replaced.

Tag| Signification|
-------- | -----
`/*AVATAR NAME*/`|Will be replaced by the name of your custom avatar or the OverwriteName if the coresponding checkbox is checked
`/*AVATAR AUTHOR*/`|Will be replaced by the name of creator
 `/*StoreLink*/` |Will be replaced by a link to where the user can buy the original moddel [^(pro-tip for booth users, if you naviagte to the creator page first, it'll translate in the user's prefered language)]
 `/*PACKAGE NAME*/` |Will be replaced by the name of the package the user is expected to have on hand when using your patcher
 `/*DIR PREFAB*/` |Will be replaced by where your user will be able to find a working prefab of your moddel
 `/*DIR PATCHER*/` |Will be replaced by where your user will be able to find your menu bar menu

Will be replaced by something that makes sense in for your project

## Updater 🔧

You may want to update and maintain your modifications in the future. Instead of having to create a whole new patcher everytime, I created an Updater tool to make the process of porting to a newer version easier:


<p align="center">
  <img src="https://raw.githubusercontent.com/HashEdits/EditDistributionTools/main/ReadMeFiles/Updaterwindowindexwithnumbers.png" alt="Updater Window"/>
</p>


Number| Name| Description
-------- | -----| -----
1| Updated original model slot| A slot to drag and drop a prefab of the updated original model into (requires the model to have an avatar component for now) |
2| Updated Modified model slot |  A slot to drag and drop a prefab of your updated modified model into (requires the model to have an avatar component for now) |
3| Updated original package name | Will be used to let your user know which version of the original model they're expected to import. very crutial to give them some sort of version number here that will resonate with your user.
4| Generate descriptions checkbox | (`Optional`) Will enable the description generator feature
5| Description folder selection button | This button will prompt you to select a description folder. All txt files will be scanned and copied in the destination folder after hitting patch if the Generate descriptions checkbox is checked
6| Destination folder selection button | This button will prompt you to select a destination folder. all files found in the description folder will be coppied in this directory with the tags changed
7| Creator name | All `/*AVATAR AUTHOR*/` tags will be replaced by what you put in there 
8| Store page | All `/*StoreLink*/`  tags will be replaced by what you put in there
9| Update button | Will update the patcher script, the diff files and the descriptions (if you've checked the option)

## Upcoming features

I've tried setting up a board for y'all to see what I'd like to add to flesh out my face tracking tools.

[Here's a link to it!](https://github.com/users/HashEdits/projects/1/views/1)




## Credits 📕

**[VRCFT](https://github.com/benaclejames/VRCFaceTracking)**

**[HDiffPatch](https://github.com/sisong/HDiffPatch)**

**[StackEdit](stackedit.io)**
