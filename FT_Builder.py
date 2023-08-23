import os
import sys
import shutil
import subprocess
import PyInstaller.__main__

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

def get_assets_relative_path(file_path):
    asset_folder = "Assets"
    index = file_path.rfind(asset_folder)
    if index != -1:
        return file_path[index + len(asset_folder) + 1:]
    else:
        return None

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

    # Get the dropped file path as input
    OriginalFBX = input("Drag and drop the original FBX here (make sure it's in between quotation marks): ")
    OriginalFBX = OriginalFBX.strip('"')
    FaceTrackedFBX = input("Drag and drop the face tracked FBX here (make sure it's in between quotation marks): ")
    FaceTrackedFBX = FaceTrackedFBX.strip('"')
    NameCustomDir = input("Please input the name of your custom directory: ")
    NameCustomAvatarDir = input("Please input the name of the avatar's custom directory: ")
    NameFBXDiffFile = input("Please input the name of your FBX Diff file: ")
    NameMetaDiffFile = input("Please input the name of your import settings Diff file: ")

    

    
    target_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher', 'data', 'DiffFiles'))
    os.makedirs(target_dir, exist_ok=True)



    hdiffz = os.path.join(os.path.dirname(__file__),  'hdiff', 'hdiffz.exe')
    hpatchz = os.path.join(os.path.dirname(__file__),  'hdiff', 'hpatchz.exe')

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


    #build
    buildDestination = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher'))
    PyInstaller.__main__.run([
    "! "+NameCustomAvatarDir+"Patcher.py",
    '--onedir',

    '--distpath', buildDestination,
    '--icon='+ os.path.abspath(os.path.join(os.path.dirname(__file__), 'YourCoolIcon.ico'))
    ])


    #moving files over
    source_dir = os.path.join(buildDestination, "! "+NameCustomAvatarDir+"Patcher")
    file_names = os.listdir(source_dir)
    
    for file_name in file_names:
        shutil.move(os.path.join(source_dir, file_name), buildDestination)

    #moving evrything under the data folder
    shutil.move(os.path.join(buildDestination, '_bz2.pyd'), os.path.join(buildDestination, 'data', '_bz2.pyd'))
    shutil.move(os.path.join(buildDestination, '_hashlib.pyd'), os.path.join(buildDestination, 'data', '_hashlib.pyd'))
    shutil.move(os.path.join(buildDestination, '_lzma.pyd'), os.path.join(buildDestination, 'data', '_lzma.pyd'))
    shutil.move(os.path.join(buildDestination, '_socket.pyd'), os.path.join(buildDestination, 'data', '_socket.pyd'))
    shutil.move(os.path.join(buildDestination, '_ssl.pyd'), os.path.join(buildDestination, 'data', '_ssl.pyd'))
    shutil.move(os.path.join(buildDestination, 'libcrypto-1_1-x64.dll'), os.path.join(buildDestination, 'data', 'libcrypto-1_1-x64.dll'))
    shutil.move(os.path.join(buildDestination, 'libssl-1_1-x64.dll'), os.path.join(buildDestination, 'data', 'libssl-1_1-x64.dll'))
    shutil.move(os.path.join(buildDestination, 'select.pyd'), os.path.join(buildDestination, 'data', 'select.pyd'))
    shutil.move(os.path.join(buildDestination, 'unicodedata.pyd'), os.path.join(buildDestination, 'data', 'unicodedata.pyd'))
    shutil.move(os.path.join(buildDestination, 'VCRUNTIME140.dll'), os.path.join(buildDestination, 'data', 'VCRUNTIME140.dll'))
    shutil.copy(hpatchz, os.path.join(buildDestination, 'data', 'hpatchz.exe'))
    shutil.copy(os.path.join(hpatchz, '..', 'License.txt'), os.path.join(buildDestination, 'data', 'License.txt'))
    os.rmdir(source_dir)


if __name__ == "__main__":
    main()
