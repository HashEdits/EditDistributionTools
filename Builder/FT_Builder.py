import os
import sys
import shutil
import subprocess
import PyInstaller.__main__

def get_assets_relative_path(file_path):
    asset_folder = "Assets"
    index = file_path.rfind(asset_folder)
    if index != -1:
        return file_path[index + len(asset_folder) + 1:]
    else:
        return None

def modify_python_patcher_script(original_model_path, original_meta_file_path, diff_file_name, meta_diff_file_name, output_name, NameCustomScript):
    with open('../PythonPatcher.py', 'r') as file:
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
        f'diff_file_path="data/DiffFiles/{diff_file_name}"'
    )
    script_content = script_content.replace(
        'meta_diff_file_path="data/DiffFiles/NameOfYourMetaDiffFile.hdiff"',
        f'meta_diff_file_path="data/DiffFiles/{meta_diff_file_name}"'
    )
    script_content = script_content.replace(
        'output_name = "MyCoolAvatar_FT"',
        f'output_name = "{output_name}"'
    )

    with open("! "+NameCustomScript+"Patcher.py", 'w') as file:
        file.write(script_content)

    print("Modified script created as '! " + NameCustomScript + "Patcher.py'")



def main():
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



    hdiffz = os.path.join(os.path.dirname(__file__), '..', 'diff file generator', 'hdiffz.exe')

    FBXDiffFile = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher', 'data', 'DiffFiles', NameFBXDiffFile+".hdiff"))
    MetaDiffFile = os.path.abspath(os.path.join(os.path.dirname(__file__), NameCustomDir, NameCustomAvatarDir, 'patcher', 'data', 'DiffFiles', NameMetaDiffFile+".hdiff"))


    if subprocess.run([hdiffz, '-f', OriginalFBX, FaceTrackedFBX, FBXDiffFile]).returncode != 0:
        print("Error occurred during creation of the diff file.")
        input("Press Enter to continue...")

    if subprocess.run([hdiffz, '-f', OriginalFBX+".meta", FaceTrackedFBX+".meta", MetaDiffFile]).returncode != 0:
        print("Error occurred during creation of the meta diff file.")
        input("Press Enter to continue...")
    
    OriginalScript = os.path.abspath(os.path.join(__file__,'..', '..', 'PythonPatcher.py'))
    destination_path = os.path.abspath(os.path.join(os.path.dirname(__file__),"! "+ NameCustomAvatarDir + "Patcher.py"))

    print("OriginalScript:", OriginalScript)
    print("destination_path:", destination_path)

    shutil.copy(OriginalScript, destination_path)


    modify_python_patcher_script(
        get_assets_relative_path(OriginalFBX).replace("\\", "/"),
        get_assets_relative_path(OriginalFBX+".meta").replace("\\", "/"),
        NameFBXDiffFile,
        NameMetaDiffFile,
        os.path.basename(FaceTrackedFBX).replace("\\", "/"),
        NameCustomAvatarDir
    )



if __name__ == "__main__":
    main()
