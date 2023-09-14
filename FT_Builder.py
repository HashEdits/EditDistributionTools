import os
from pickle import TRUE
import tkinter as tk
from tkinter import filedialog
import sys
import shutil
import subprocess
import PyInstaller.__main__


#prints my cool ascii art
def print_ascii_art():
    ascii_art = r"""
 ____  ____    ____  _  _  __  __    ____  ____  ____ 
(  __)(_  _)  (  _ \/ )( \(  )(  )  (    \(  __)(  _ \
 ) _)   )(     ) _ () \/ ( )( / (_/\ ) D ( ) _)  )   /
(__)   (__)   (____/\____/(__)\____/(____/(____)(__\_)
    """
    credits = r"""
                 Made By Hash - vVersionNumber


    """
    print(ascii_art)
    print(credits)

def print_ascii_Ready():
    ascii_art = r"""
    

    








    





 
 _  _  __   _  _  ____    ____  __  __    ____  ____     
( \/ )/  \ / )( \(  _ \  (  __)(  )(  )  (  __)/ ___)    
 )  /(  O )) \/ ( )   /   ) _)  )( / (_/\ ) _) \___ \    
(__/  \__/ \____/(__\_)  (__)  (__)\____/(____)(____/    
  __   ____  ____    ____  ____   __   ____  _  _    _   
 / _\ (  _ \(  __)  (  _ \(  __) / _\ (    \( \/ )  / \  
/    \ )   / ) _)    )   / ) _) /    \ ) D ( )  /   \_/  
\_/\_/(__\_)(____)  (__\_)(____)\_/\_/(____/(__/    (_)  
    """
    LilText = r"""
    Evrything should be all up and ready in the folder

    """
    print(ascii_art)
    print(LilText)


def get_file_path(prompt):
    root = tk.Tk()
    root.withdraw()  # Hide the main window

    file_path = filedialog.askopenfilename(title=prompt)

    return file_path

def get_directory_path(prompt):
    root = tk.Tk()
    root.withdraw()  # Hide the main window

    directory_path = filedialog.askdirectory(title=prompt)

    return directory_path

#remplaces the sick descriptions you've made for your product with what you input if you so please
def replace_placeholders_in_files_in_directory(directory, AvatarName, AvatarCreatorName, BoothLink, PackageName, DirPatcher, DirPrefab):
    replacements = {
        '/*AVATAR NAME*/': AvatarName,
        '/*AVATAR AUTHOR*/': AvatarCreatorName,
        '/*StoreLink*/': BoothLink,
        '/*PACKAGE NAME*/': PackageName,
        '/*DIR PATCHER*/': DirPatcher,
        '/*DIR PREFAB*/': DirPrefab
    }

    current_directory = os.getcwd()
    retail_folder = os.path.join(current_directory, 'Retail Descriptions ' + AvatarName)
    os.makedirs(retail_folder, exist_ok=True)

    for root, _, files in os.walk(directory):
        for file_name in files:
            file_path = os.path.join(root, file_name)
            with open(file_path, 'r', encoding='utf-8') as file:
                file_content = file.read()

            for placeholder, value in replacements.items():
                file_content = file_content.replace(placeholder, value)

            relative_path = os.path.relpath(file_path, directory)
            new_file_path = os.path.join(retail_folder, relative_path)
            os.makedirs(os.path.dirname(new_file_path), exist_ok=True)
            with open(new_file_path, 'w', encoding='utf-8') as file:
                file.write(file_content)

    print(f"Modified files saved in '{retail_folder}' folder.")



#fonction that gives the relative path from the Assets folder
def get_assets_relative_path(file_path):
    asset_folder = "Assets"
    index = file_path.rfind(asset_folder)
    if index != -1:
        return file_path[index + len(asset_folder) + 1:]
    else:
        return None
    
#fonction that will take care of modifying the right lines of code in the patcher
def modify_python_patcher_script(original_model_path, original_meta_file_path, diff_file_name, meta_diff_file_name, output_name, NameCustomScript):
    with open('PythonPatcher.py', 'r', encoding='utf-8') as file:  # Open the file in UTF-8 encoding
        script_content = file.read()

    script_content = script_content.replace(
        'original_model_path="../../../ Location of your FBX relative to Assets"',
        f'original_model_path="../../../{original_model_path}"'
    )
    script_content = script_content.replace(
        'original_meta_file_path="../../../ Location of your FBX.meta relative to Assets"',
        f'original_meta_file_path="../../../{original_meta_file_path}"'
    )
    script_content = script_content.replace(
        'diff_file_path="data/DiffFiles/NameOfYourDiffFile.hdiff"',
        f'diff_file_path="data/DiffFiles/{diff_file_name}.hdiff"'
    )
    script_content = script_content.replace(
        'meta_diff_file_path="data/DiffFiles/NameOfYourMetaDiffFile.hdiff"',
        f'meta_diff_file_path="data/DiffFiles/{meta_diff_file_name}.hdiff"'
    )
    script_content = script_content.replace(
        'output_name = "MyCoolAvatar_FT"',
        f'output_name = "{output_name}"'
    )

    with open("! "+NameCustomScript+"Patcher.py", 'wb') as file:  # Open the file in binary write mode
        encoded_content = script_content.encode('utf-8')  # Encode the content as bytes
        file.write(encoded_content)

    print("Modified script and saved it as '! " + NameCustomScript + "Patcher.py'")




def get_valid_description_directory():
    while True:
        print("Please select the directory with your descriptions and readme files (leave empty to skip) (window may not be focused, check your alt+tab to find the window)")
        DescriptionDir = get_directory_path("Please select the directory with your descriptions and readme files (leave empty to skip)")

        if not DescriptionDir:
            # User left it empty or canceled, skip
            print("Description directory selection skipped.")
            return "", "", "", ""  # Return None for all values

        # Check if DescriptionDir is a valid directory
        if os.path.isdir(DescriptionDir):
            txt_files = [f for f in os.listdir(DescriptionDir) if f.endswith(".txt")]

            if txt_files:
                # Valid directory with .txt files, get additional inputs
                CreatorName = input("Please input the name of the creator: ")
                BoothPage = input("Please input the page of the avatar: ")
                PackageName = input("Please input the name of the package that users will have: ")
                return DescriptionDir, CreatorName, BoothPage, PackageName
            else:
                print("The selected directory does not contain any .txt files. Please try again.")
        else:
            print("Invalid directory. Please select a valid directory or leave empty to skip.")



def delete_files_in_directory(directory_path):
    try:
        # Check if the directory exists
        if os.path.exists(directory_path):
            # Iterate over all files and subdirectories in the directory
            for root, dirs, files in os.walk(directory_path, topdown=False):
                for file_name in files:
                    file_path = os.path.join(root, file_name)
                    os.remove(file_path)  # Delete the file
                for dir_name in dirs:
                    dir_path = os.path.join(root, dir_name)
                    shutil.rmtree(dir_path)  # Delete the subdirectory and its contents
            shutil.rmtree(directory_path)
            print(f"generated custom folder deleted")
        else:
            print(f"no custom folder found")
    except Exception as e:
        print(f"An error occurred while deleting the files in the custom folder, error:  {e}")







def main():
    os.environ['PYTHONIOENCODING'] = 'utf-8'

    print_ascii_art()


    # Get the required info to proceed

    print("Please select the original FBX file")
    OriginalFBX = get_file_path("Please select the original FBX file")
    while not os.path.isfile(OriginalFBX):
        print("Please select a valid original FBX file")
        OriginalFBX = get_file_path("Please select a valid original FBX file")

    print("Please select the Face tracked FBX file")
    FaceTrackedFBX = get_file_path("Please select the Face tracked FBX file")
    while not os.path.isfile(FaceTrackedFBX):
        print("Please select a valid Face tracked FBX file")
        FaceTrackedFBX = get_file_path("Please select a valid Face tracked FBX file")



        

    NameCustomDir = input("Please input the name of your custom directory: ")

    while os.path.isfile(NameCustomDir):
        print("Please input a name for your custom directory that isn't a file: ")
        NameCustomDir = input("Please input a name for your custom directory that isn't a file: ")
        
    print("Please input the name of the avatar's custom directory: ")
    NameCustomAvatarDir = input("Please input the name of the avatar's custom directory: ")
    while os.path.isfile(NameCustomAvatarDir) or os.path.isdir(NameCustomAvatarDir):
        print("Please input a name for your avatar's custom directory that isn't a file or please delete your old atempt: ")
        NameCustomAvatarDir = input("Please input a name for your avatar's custom directory that isn't a file or please delete your old atempt: ")


    DescriptionDir, CreatorName, BoothPage, PackageName = get_valid_description_directory()

    

    
    try:
        #creates the funky subfolders
        target_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher', 'data', 'DiffFiles'))
        os.makedirs(target_dir, exist_ok=True)


        #location of the hdiff builds
        hdiffz = os.path.join(os.path.dirname(__file__), 'hdiff', 'hdiffz.exe')
        hpatchz = os.path.join(os.path.dirname(__file__), 'hdiff', 'hpatchz.exe')

        #location of the diff files
        NameFBXDiffFile = NameCustomAvatarDir + 'Diff'
        NameMetaDiffFile = NameCustomAvatarDir + 'Meta' + 'Diff'
        FBXDiffFile = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher', 'data', 'DiffFiles', NameFBXDiffFile+".hdiff"))
        MetaDiffFile = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher', 'data', 'DiffFiles', NameMetaDiffFile+".hdiff"))

        #creates the FBXDiffFile
        if subprocess.run([hdiffz, '-f', OriginalFBX, FaceTrackedFBX, FBXDiffFile]).returncode != 0:
            raise Exception("Error occurred during creation of the diff file.")

        #creates the MetaFBXDiffFile
        if subprocess.run([hdiffz, '-f', OriginalFBX+".meta", FaceTrackedFBX+".meta", MetaDiffFile]).returncode != 0:
            raise Exception("Error occurred during creation of the meta diff file.")
        
        #location of the og script and the destination script
        OriginalScript = os.path.abspath(os.path.join(__file__, '..', 'PythonPatcher.py'))
        destination_path = os.path.abspath(os.path.join(os.path.dirname(__file__),"! "+ NameCustomAvatarDir + "Patcher.py"))

        print("OriginalScript:", OriginalScript)
        print("destination_path:", destination_path)

        shutil.copy(OriginalScript, destination_path)

        #modifying the script with the input files (makes sure it's using the right formating)
        modify_python_patcher_script(
            get_assets_relative_path(OriginalFBX).replace("\\", "/"),
            get_assets_relative_path(OriginalFBX+".meta").replace("\\", "/"),
            NameFBXDiffFile,
            NameMetaDiffFile,
            os.path.basename(FaceTrackedFBX).replace("\\", "/"),
            NameCustomAvatarDir
        )


        #build the patcher
        buildDestination = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher'))
        PyInstaller.__main__.run([
        "! "+NameCustomAvatarDir+"Patcher.py",
        '--onedir',
        '--distpath', buildDestination,
        '--icon='+ os.path.abspath(os.path.join(os.path.dirname(__file__), 'YourCoolIcon.ico'))
        ])


        #moving patcher files over
        source_dir = os.path.join(buildDestination, "! "+NameCustomAvatarDir+"Patcher")
        file_names = os.listdir(source_dir)
        
        for file_name in file_names:
            shutil.move(os.path.join(source_dir, file_name), buildDestination)


        #moving evrything that can be under the data folder
        files_to_exclude = [
            "! "+NameCustomAvatarDir+"Patcher.exe",
            "base_library.zip",
            NameFBXDiffFile+".hdiff",
            NameMetaDiffFile+".hdiff"
        ]

        dll_names_to_exclude = ["python"]

        for root, _, files in os.walk(buildDestination):
            for file_name in files:
                source_path = os.path.join(root, file_name)
                destination_path = ""

                if file_name in files_to_exclude:
                    continue
                
                is_dll_with_python = any(name in file_name.lower() for name in dll_names_to_exclude)
                if is_dll_with_python:
                    continue
                else:
                    destination_path = os.path.join(buildDestination, 'data', file_name)

                shutil.move(source_path, destination_path)

        shutil.copy(hpatchz, os.path.join(buildDestination, 'data', 'hpatchz.exe'))
        shutil.copy(os.path.join(hpatchz, '..', 'License.txt'), os.path.join(buildDestination, 'data', 'License.txt'))
        os.rmdir(source_dir)
        print("Files moved to the 'data' folder.")

        if DescriptionDir != '':
            #remplacing descriptions and readme placeholders
            DirPatcher = os.path.join('Assets', NameCustomDir, NameCustomAvatarDir, 'patcher')
            DirPrefab = os.path.join('Assets', NameCustomDir, NameCustomAvatarDir, 'prefab')

            replace_placeholders_in_files_in_directory(
                DescriptionDir,
                NameCustomAvatarDir,
                CreatorName,
                BoothPage,
                PackageName,
                DirPatcher,
                DirPrefab,
            )

            
        #cleaning up
        delete_files_in_directory(os.path.abspath(os.path.join(os.path.dirname(__file__), 'build')))

        if os.path.isfile(os.path.abspath(os.path.join(os.path.dirname(__file__), "! " + NameCustomAvatarDir + "Patcher.py"))):
            os.remove(os.path.abspath(os.path.join(os.path.dirname(__file__), "! " + NameCustomAvatarDir + "Patcher.py")))

        if os.path.isfile(os.path.abspath(os.path.join(os.path.dirname(__file__), "! " + NameCustomAvatarDir + "Patcher.spec"))):
            os.remove(os.path.abspath(os.path.join(os.path.dirname(__file__), "! " + NameCustomAvatarDir + "Patcher.spec")))
        

        #yippie
        print_ascii_Ready()




        
    except Exception as e:
        
        delete_files_in_directory(os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir)))
        
        delete_files_in_directory(os.path.abspath(os.path.join(os.path.dirname(__file__), 'build')))

        if os.path.isfile(os.path.abspath(os.path.join(os.path.dirname(__file__), "! " + NameCustomAvatarDir + "Patcher.py"))):
            os.remove(os.path.abspath(os.path.join(os.path.dirname(__file__), "! " + NameCustomAvatarDir + "Patcher.py")))

        if os.path.isfile(os.path.abspath(os.path.join(os.path.dirname(__file__), "! " + NameCustomAvatarDir + "Patcher.spec"))):
            os.remove(os.path.abspath(os.path.join(os.path.dirname(__file__), "! " + NameCustomAvatarDir + "Patcher.spec")))
        

        ascii_art = r"""

    

    













                 ____  ____  ____   __  ____ 
                (  __)(  _ \(  _ \ /  \(  _ \
                 ) _)  )   / )   /(  O ))   /
                (____)(__\_)(__\_) \__/(__\_)
        """

        print(ascii_art)

        #big_sadge
        print("An error occurred while copying the file:", str(e))


if __name__ == "__main__":
    main()
