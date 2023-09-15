import os
from pickle import TRUE
import tkinter as tk
from tkinter import filedialog
import sys
import shutil

def print_ascii_yippie():
    ascii_art = r"""
    

    








    




 
         _  _  __  ____  ____  __  ____ 
        ( \/ )(  )(  _ \(  _ \(  )(  __)
         )  /  )(  ) __/ ) __/ )(  ) _) 
        (__/  (__)(__)  (__)  (__)(____)
    """

    print(ascii_art)

def custom_ignore(dir, contents):
    exclude_files = ["ReleaseBundler.py" ,"FT_Builder.py", "PythonPatcher.py", "README.md", ".gitattributes"]
    exclude_folders = [".github", ".git", "temp"]  # List of folder names to exclude
    ignored = set()

    for item in contents:
        item_path = os.path.join(dir, item)

        # Check if it's a directory and if it's one of the folders to exclude
        if os.path.isdir(item_path):
            if item in exclude_folders:
                ignored.add(item)
        else:  # It's a file
            if item in exclude_files:
                ignored.add(item)

    return ignored

def copy_directory(source_dir, destination_dir):
    try:
        for root, dirs, files in os.walk(source_dir):
            # Use the custom_ignore function to exclude files and folders
            ignored = custom_ignore(root, dirs + files)
            for item in ignored:
                if os.path.isdir(os.path.join(root, item)):
                    dirs.remove(item)
                else:
                    files.remove(item)

            for file in files:
                src_file = os.path.join(root, file)
                dest_file = os.path.join(destination_dir, os.path.relpath(src_file, source_dir))
                os.makedirs(os.path.dirname(dest_file), exist_ok=True)
                shutil.copy2(src_file, dest_file)

        print(f"Successfully copied from '{source_dir}' to '{destination_dir}'")
    except Exception as e:
        print(f"An error occurred: {str(e)}")

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


#fonction that will take care of modifying the right lines of code in the patcher
def ModifyVersionNumber(VersionNumber, RepoDir, DestDir, fileName):
    with open(os.path.join(RepoDir, fileName), 'r', encoding='utf-8') as file:
        script_content = file.read()

    script_content = script_content.replace(
        'VersionNumber',
        f'{VersionNumber}'  # Remove the surrounding double quotes
    )

    with open(os.path.join(DestDir, fileName), 'wb') as file:
        encoded_content = script_content.encode('utf-8')
        file.write(encoded_content)

    print("replaced version number")



def zip_folder(source_dir, zip_filename):
    try:
        shutil.make_archive(zip_filename, 'zip', source_dir)
        print(f"Successfully zipped '{source_dir}' to '{zip_filename}.zip'")
    except Exception as e:
        print(f"An error occurred: {str(e)}")



def get_directory_path(prompt):
    root = tk.Tk()
    root.withdraw()  # Hide the main window

    directory_path = filedialog.askdirectory(title=prompt)

    return directory_path

def main():
    os.environ['PYTHONIOENCODING'] = 'utf-8'

    #RepoDir =  get_directory_path("please select where you have your repo")
    RepoDir = os.path.dirname(__file__)

    DestDir =  get_directory_path("please select where you want your release to be built")
    
    Version_Number = input("Please input the version of the release: ")
    
    TempDir = os.path.abspath(os.path.join(os.path.dirname(__file__), "temp"))
    os.makedirs(TempDir, exist_ok=True)
    
    copy_directory(RepoDir, TempDir)
    ModifyVersionNumber(Version_Number, RepoDir, TempDir,"FT_Builder.py")
    ModifyVersionNumber(Version_Number, RepoDir, TempDir,"PythonPatcher.py")
    zip_folder(TempDir, os.path.join(DestDir, "Face-Tracking-Patcher-V"+Version_Number))
    delete_files_in_directory(TempDir)
    print_ascii_yippie()




if __name__ == "__main__":
    main()
