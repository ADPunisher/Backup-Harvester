# Backup-Harvester

SAM and SYSTEM registry hive dumper based on the SeBackupPrivilege.

## Code Logic
The application first checks if the user has admin privileges to run. If the user is not an administrator, the application displays an error message and exits.

* Check if the user has admin privileges.
* Enable the backup privilege by calling the EnablePrivilege function with the privilege name "SeBackupPrivilege".
* Harvest the sam and system by calling the HarvestBackup function.
* The HarvestBackup function sets the destination directory to "C:\Users\Public"
* If the directory doesn't exist, it creates it.
* It saves the registry hives to the specified directory.

Once harvesting complete, the program ends and displays a message indicating that it has finished.

## Usage
* Clone or download the repository.
* Open the project in Visual Studio.
* Build the project.
* Open the command prompt or terminal and navigate to the location of the executable file.
* Execute the application.
* BackupHarvester.exe

## Video Proof-of-Concept

https://youtu.be/-nv5OJhhHh0

## Sam System Data extraction Proof-of-concept

![Screenshot (852)](https://user-images.githubusercontent.com/66905930/233942622-b7398e69-befb-4851-b70a-3211385c81cf.png)
