import os
import re

def rename_files():
    # Get current working directory
    directory = os.getcwd()

    # Pattern to match files with 'kol 1 Mission Name' followed by a number and optional .asset/.meta
    pattern = re.compile(r'^(kol 1 Station )(\d+)(\.asset(?:\.meta)?)$')

    # Get list of files in directory
    files = os.listdir(directory)

    # Find the highest number
    max_number = -1
    for file in files:
        match = pattern.match(file)
        if match:
            number = int(match.group(2))
            if number > max_number:
                max_number = number

    # If no matching files found, return
    if max_number == -1:
        print("No matching files found.")
        return

    # Rename the file(s) with the highest number
    for file in files:
        match = pattern.match(file)
        if match and int(match.group(2)) == max_number:
            old_name = file
            new_number = max_number + 1
            new_name = f"{match.group(1)}{new_number}{match.group(3)}"
            os.rename(
                os.path.join(directory, old_name),
                os.path.join(directory, new_name)
            )
            print(f"Renamed: {old_name} -> {new_name}")

if __name__ == "__main__":
    rename_files()
