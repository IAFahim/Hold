import os

def rename_files_in_directory(directory_path):
    # List all files in the directory
    for filename in os.listdir(directory_path):
        # Check if the filename contains 'kol 1'
        if 'kol 1' in filename:
            # Create the new filename by replacing 'kol 1' with 'kol 0'
            new_filename = filename.replace('kol 1', 'kol 0')

            # Create full file paths
            old_file = os.path.join(directory_path, filename)
            new_file = os.path.join(directory_path, new_filename)

            # Rename the file
            os.rename(old_file, new_file)
            print(f'Renamed: {filename} to {new_filename}')

# Specify the directory path where your files are located
directory_path = 'path/to/your/directory'

# Call the function to rename the files
rename_files_in_directory(directory_path)
