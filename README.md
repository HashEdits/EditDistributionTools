
# Face Tracking Patcher

  

This is a simple tool to automate the ease the distribution and creation of face tracked avatars for VRChat.

  

Due to how curent 3D model files are set up there isn't a way to share shape-keys without being tied to a mesh. Making the process of sharing such failes rather tedious or to be left in the context of a commision where sharing mesh data is in general allowed between the commisioner and the artist.

  

After being involved in the scene it was clear that the previous methods were sub optimal: overwriting the original model and requiering the user to navigate to a subfolder unknown in advanced.

  

My piece of software let's your customers having a reliable path to navigate to, building a reliable way of interacting with your products.



If used please credit HashEdits

  

# Pre-requisites

  

[Python](https://www.python.org/downloads/)

  

[Auto py to exe](https://pypi.org/project/auto-py-to-exe/)

  

> pip install auto-py-to-exe

[Py Intsaller](https://pypi.org/project/auto-py-to-exe/)

>pip install pyinstaller

  

  
  
  


## How to use
  
Start FT_Builder.exe in the Builder folder
 
 Follow the steps

start Auto py to exe

  

>auto-py-to-exe

  

Select your generated patcher script (in the same directory "! YourModelNamePatcher")

  

Select your icon

  
Click convert .py to .exe

  

Click "open output folder"

Select anything but:

- your exe

- base_library

- python(your Python version).dll

  

 navigate to YourNameCustomDir/NameCustomAvatarDir/patcher/data/

Put it in the data folder

navigate to YourNameCustomDir/NameCustomAvatarDir/patcher/

Get evrything else and put it in the patcher folder



## Distribute a face tracked avatar
copy your custom generated directory earlier and drag and drop it in the Assets folder of your unity project

make sure that your avatar has the required parameters, FX and additive controllers set up for VRCFaceTracking.

The use of [Jerry's template](https://github.com/Adjerry91/VRCFaceTracking-Templates) highly recomended

make sure the avatar works in game

(if not check for potential avatar animation overwrite conflicts in your animators)

create a prefab (preferably in a prefab folder under the face tracked avatar's name)

select your custom folder

right click

export as UnityPackage

deselect your face tracked FBX

deselect any of the avatar's original files

click export

you're ready to sell your custom face tracked avatar!
(make sure to always check the creator's licence agreement on attachements)
  

## Build FT_Builder from source

  
start Auto py to exe

  

>auto-py-to-exe

select FT_Builder script

Click convert.py to .exe

Click "open output folder"

copy evrything under the Builder directory

  

## Credits

**[HDiffPatch](https://github.com/sisong/HDiffPatch)**