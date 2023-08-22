# Face Tracking Patcher

This is a simple tool to automate the ease the distribution and creation of face tracked avatars for VRChat.

Due to how curent 3D model files are set up there isn't a way to share shape-keys without being tied to a mesh. Making the process of sharing such failes rather tedious or to be left in the context of a commision where sharing mesh data is in general allowed between the commisioner and the artist.

After being involved in the scene it was clear that the previous methods were sub optimal: overwriting the original model and requiering the user to navigate to a subfolder unknown in advanced.

My piece of software let's your customers having a reliable path to navigate to, building a reliable way of interacting with your products.

# Pre-requisites

[Python](https://www.python.org/downloads/)

[Auto py to exe](https://pypi.org/project/auto-py-to-exe/)

	> pip install auto-py-to-exe
 
[Py Intsaller](https://pypi.org/project/auto-py-to-exe/)
	
	>pip install pyinstaller

  




## How to use

remplace the paths to your model and diff files in the script

  
start Auto py to exe

	>auto-py-to-exe

  

select your icon

  

generate exe

  

go to the folder, select anything but:
- your exe
- base_library
- python(your Python version).dll

  

put it in the data folder

get evrything else and put it in the patcher folder

## Generating diff files

use the .bat files in the diff file generator folder

## Auto generation of diff files

todo

## Auto generation of sub folder

todo

## Credits
**[HDiffPatch](https://github.com/sisong/HDiffPatch)**
