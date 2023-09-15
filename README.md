

# Face Tracking Patcher 🗳️

This is a simple tool to automate the ease the distribution and creation of face tracked avatars for VRChat.

![ ](https://cdn.discordapp.com/attachments/337986548664500236/1143948164232126656/Thumbnail_Face_tracking_patcher_Git_Hub.png)

Due to how curent 3D model files are set up there isn't a way to share shape-keys without being tied to a mesh. Making the process of sharing such files rather tedious or to be left to the context of a commision where sharing mesh data is in general allowed between the commisioner and the artist.


After being involved in the scene it was clear that the previous mainstream methods were sub optimal: overwriting the original model and requiering the user to navigate to a subfolder unknown in advanced.

My piece of software let's you build a patcher with ease, saving you time and letting your customers have a reliable path to navigate to. Building a reliable way for them to interact with your products.
It also now supports adding a directory with your different localized descriptions and readme to make the process of putting the avatar on the storefront much less painfull

I am going to be using my tool from now on to build my Face Tracking Add-ons avaliable on my [Booth](https://hashedits.booth.pm/) and [Ko-Fi](https://ko-fi.com/hashedits/shop) shops

If used please credit my socials and this GitHub Page

## Pre-requisites 🤓

[Python](https://www.python.org/downloads/)

[Py Intsaller](https://pypi.org/project/auto-py-to-exe/)



>pip install pyinstaller

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
There is an extra theorical directory to show you what to expect from the builder if you input:

- WhereverOnYourProjectIsLocatedOnYourDrive\ProjectName\Assets\CreatorName\AvatarName\fbx\MyCoolAvatar.fbx

- WhereverOnYourProjectIsLocatedOnYourDrive\ProjectName\Assets\YourCustomDir\NameOfTheModel\fbx\MyCoolAvatar_FT.fbx

- NameOfYourCustomFolder
(something unique to keep all of your files in your addon)
- NameOfTheModel

- Path\To\My\Cool\Descriptions&ReadMe

- AvatarCreatorName

- [Creator.CoolStoreFront.com/item/MyCoolAvatar](https://www.youtube.com/watch?v=dQw4w9WgXcQ)

- NameOfThePackageCustomersOwn

you'll find an exemple readme in **Description&ReadMeExemples** that you can bundle with your avatar's package to let the customer know what to do with it.
There's also an exemple description there.
in summary any of the following:

`/*AVATAR NAME*/`,`/*AVATAR AUTHOR*/`, `/*StoreLink*/`, `/*PACKAGE NAME*/`, `/*DIR PATCHER*/`, `/*DIR PREFAB*/`

will be remplaced by something that makes sense in for your project


## Distribute a face tracked avatar 👨‍🏫
- Copy your custom generated directory earlier and drag and drop it in the Assets folder of your unity project

- Make sure that your avatar has the required parameters, FX and additive controllers set up for VRCFaceTracking.
(The use of [Jerry's template](https://github.com/Adjerry91/VRCFaceTracking-Templates) highly recomended)

- Make sure the avatar works in game
(would recommend using meowface, the very easy to use [Meowface](https://github.com/regzo2/VRCFaceTracking-MeowFace))
(or [LiveLink](https://github.com/kusomaigo/VRCFaceTracking-LiveLink) if you're Apple flavoured)

(If it doesn't check for potential avatar animation overwrite conflicts in your animators)

- Create a prefab
(this is done by dragging your avatar from your hierarchy to a prefab folder in the folder you generated for your avatar)

- Select your custom folder

- Right click

- Export Package

- Deselect your Face Tracked FBX

- Deselect any of the avatar's original files

- Click "Export..."

- You're ready to sell your custom face tracked avatar!
(make sure to always check the creator's licence agreement on attachements)

  

## Upcoming features

I've tried setting up a board for y'all to see what I'd like to add to flesh out my face tracking tools.

[Here's a link to it](https://github.com/users/HashEdits/projects/1/views/1)


  

## Credits 📕

**[VRCFT](https://github.com/benaclejames/VRCFaceTracking)**

**[HDiffPatch](https://github.com/sisong/HDiffPatch)**

**[Nimble Design System Icons](https://iconduck.com/sets/nimble-design-system-icons)**

**[StackEdit](stackedit.io)**
