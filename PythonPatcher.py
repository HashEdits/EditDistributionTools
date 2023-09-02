import os
import sys
import shutil
import subprocess

        


def print_ascii_art():
    ascii_art = r"""
 ____  __    ___  ____    ____  ____   __    ___  __ _  __  __ _   ___    ____   __  ____  ___  _  _  ____  ____ 
(  __)/ _\  / __)(  __)  (_  _)(  _ \ / _\  / __)(  / )(  )(  ( \ / __)  (  _ \ / _\(_  _)/ __)/ )( \(  __)(  _ \
 ) _)/    \( (__  ) _)     )(   )   //    \( (__  )  (  )( /    /( (_ \   ) __//    \ )( ( (__ ) __ ( ) _)  )   /
(__) \_/\_/ \___)(____)   (__) (__\_)\_/\_/ \___)(__\_)(__)\_)__) \___/  (__)  \_/\_/(__) \___)\_)(_/(____)(__\_)
    """
    credits = r"""
                                        Made By Hash - v1.0


    """
    print(ascii_art)
    print(credits)

def resource_path(relative_path):
    """ Get absolute path to resource, works for dev and for PyInstaller """
    try:
        # PyInstaller creates a temp folder and stores path in _MEIPASS
        base_path = sys._MEIPASS
    except Exception:
        base_path = os.path.abspath(".")

    return os.path.join(base_path, relative_path)

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
            print(f"OSC Config cleared.")
        else:
            print(f"no OSC config found")
    except Exception as e:
        print(f"An error occurred while clearing OSC conifg: {e}")


def patch_model(original_model_path, original_meta_file_path, diff_file_path, meta_diff_file_path, output_name):
    script_directory = resource_path(os.path.dirname(os.path.abspath(__file__)))
    original_model = resource_path(original_model_path)
    original_meta_file = resource_path(original_meta_file_path)
    diff_file = resource_path(diff_file_path)
    meta_diff_file = resource_path(meta_diff_file_path)

    if not os.path.exists(original_model):
        error_msg = f"Please make sure you have the original .fbx at:\n{original_model}"
        print(error_msg)
        input("Press Enter to continue...")
        return

    if not os.path.exists(diff_file):
        print("Missing diff file")
        input("Press Enter to continue...")
        return

    backup_dir = os.path.abspath(os.path.join(__file__, '..', '..', 'fbx'))
    os.makedirs(backup_dir, exist_ok=True)

    shutil.copy(original_model, original_model + '.bak')
    shutil.copy(original_meta_file, original_meta_file + '.bak')
    shutil.copy(original_model, os.path.join(backup_dir, f'{output_name}'))
    shutil.copy(original_meta_file, os.path.join(backup_dir, f'{output_name}.meta'))

    patcher_exe = os.path.join(os.path.dirname(__file__), 'data', 'hpatchz.exe')

    patched_model = os.path.join(backup_dir, f'{output_name}')
    patched_meta_file = os.path.join(backup_dir, f'{output_name}.meta')

    if subprocess.run([patcher_exe, '-f', patched_model, diff_file, patched_model]).returncode != 0:
        print("Error occurred during patching process for model.")
        input("Press Enter to continue...")
        return

    if subprocess.run([patcher_exe, '-f', patched_meta_file, meta_diff_file, patched_meta_file]).returncode != 0:
        print("Error occurred during patching process for meta file.")
        input("Press Enter to continue...")
        return


def main():
    print_ascii_art()

    patch_model(
        original_model_path="../../../ Location of your FBX relative to Assets",           # Change this to the relative directory to your FBX
        original_meta_file_path="../../../ Location of your FBX.meta relative to Assets",  # Change this to the relative directory to your FBX's import settings
        diff_file_path="data/DiffFiles/NameOfYourDiffFile.hdiff",                          # Change the name to the name of the Hdiff file you generated
        meta_diff_file_path="data/DiffFiles/NameOfYourMetaDiffFile.hdiff",                 # Change the name to the name of the MetaHdiff file you generated
        output_name = "MyCoolAvatar_FT"                                                    # Change this to your desired output name                                                   # Change this to your desired output name
    )

    delete_files_in_directory(os.path.join(os.path.expanduser("~"), "AppData", "LocalLow", "VRChat", "VRChat", "OSC"))
    
    print("Patch complete! Please read the documentation and instructions.")
    input("Press Enter to exit...")

if __name__ == "__main__":
    main()