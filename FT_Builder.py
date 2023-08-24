import os
from pickle import TRUE
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
                 Made By Hash - v1.0


    """
    print(ascii_art)
    print(credits)


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
    with open('PythonPatcher.py', 'r') as file:
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

    with open("! "+NameCustomScript+"Patcher.py", 'w') as file:
        file.write(script_content)

    print("Modified script and saved it as '! " + NameCustomScript + "Patcher.py'")



def main():

    print_ascii_art()

    # Get the required info to proceed

#   OriginalFBX = input("Drag and drop the original FBX here: ")                Until we find a way to build the builder
    OriginalFBX = input("Please input the path to the original FBX: ")
    if OriginalFBX.startswith('"') and OriginalFBX.endswith('"'):
        OriginalFBX = OriginalFBX.strip('"')


#   FaceTrackedFBX = input("Drag and drop the face tracked FBX here: ")
    FaceTrackedFBX = input("Please input the path to the Face tracked FBX: ")
    if FaceTrackedFBX.startswith('"') and FaceTrackedFBX.endswith('"'):
        FaceTrackedFBX = FaceTrackedFBX.strip('"')


    NameCustomDir = input("Please input the name of your custom directory: ")
    NameCustomAvatarDir = input("Please input the name of the avatar's custom directory: ")
    NameFBXDiffFile = NameCustomAvatarDir + 'Diff'
    NameMetaDiffFile = NameCustomAvatarDir + 'Meta' + 'Diff'
    DescriptionDir = input("Please input the directory of your descriptions and readme files (will skip if left empty): ")
    
    
    if DescriptionDir.startswith('"') and DescriptionDir.endswith('"'):
        DescriptionDir = DescriptionDir.strip('"')
    if DescriptionDir != '':
        CreatorName = input("Please input the name of the creator: ")
        BoothPage = input("Please input the page of the avatar: ")
        PackageName = input("Please input the name of the package that users will have: ")


    

    

    #creates the funky subfolders
    target_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher', 'data', 'DiffFiles'))
    os.makedirs(target_dir, exist_ok=True)


    #location of the hdiff builds
    hdiffz = os.path.join(os.path.dirname(__file__), 'hdiff', 'hdiffz.exe')
    hpatchz = os.path.join(os.path.dirname(__file__), 'hdiff', 'hpatchz.exe')

    #location of the diff files
    FBXDiffFile = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher', 'data', 'DiffFiles', NameFBXDiffFile+".hdiff"))
    MetaDiffFile = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher', 'data', 'DiffFiles', NameMetaDiffFile+".hdiff"))

    #creates the FBXDiffFile
    if subprocess.run([hdiffz, '-f', OriginalFBX, FaceTrackedFBX, FBXDiffFile]).returncode != 0:
        print("Error occurred during creation of the diff file.")
        input("Press Enter to continue...")

    #creates the MetaFBXDiffFile
    if subprocess.run([hdiffz, '-f', OriginalFBX+".meta", FaceTrackedFBX+".meta", MetaDiffFile]).returncode != 0:
        print("Error occurred during creation of the meta diff file.")
        input("Press Enter to continue...")
    
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
        replace_placeholders_in_files_in_directory(
            DescriptionDir,
            NameCustomAvatarDir,
            CreatorName,
            BoothPage,
            PackageName,
            'Assets' + NameCustomDir + NameCustomAvatarDir + 'patcher',
            'Assets' + NameCustomDir + NameCustomAvatarDir + 'prefab',
        )


if __name__ == "__main__":
    main()
