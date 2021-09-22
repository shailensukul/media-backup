# Media Backup

Media Backup allows media files to be read from a source and transferred to a destination with pluggable destination adapters.
The default installation ships with a FileSystemProcessor, which copies file to a file-based destination.

The program works out the creation date of the source media and copies the media to a folder arranged as per the following hierarchy.
```
yyyy
	mm
		dd
```

Before the file is copied, its hash is compared to all the hashes of the files in the destination folder.
If the hash matches, meaning that the file exists, then it is skipped.

## Examples

Note: For the true or false parameters, skip the parameter if it is false. 

If you provide a parameter, it will be intepreted as true, regardless of its value.

Copy files from a source folder to a destination folder, and keep the source files
```
media-backup.exe -s C:\temp\Pictures\Source -d D:\Temp\Destination -i -v 
```

Copy files from a source folder to a destination folder, and delete the source files

```
media-backup.exe -s C:\temp\Pictures\Source -d D:\Temp\Destination -i -v -x
```

Simulate a file copy but do not actually copy files. 

This is useful so you view any error reports about problems with file metadata (see the Output section below)
```
media-backup.exe -s C:\temp\Pictures\Source -d D:\Temp\Destination -i -v -w
```

## Output
Every execution produces two files:

* Log output - `dd-MM-yyyy-HH-mm-ss-Log.log` - contains a dump of all the log statements you see on the console

* Run report - `dd-MM-yyyy-HH-mm-ss-Processing-Result.csv` - a csv file which contains the filename, result (Success or Error) and reason for the error

## Arguments
| Argument        | Description |
| ------------- |:-------------:|
| `-s`          | source folder  |
| `-d`          | destination folder |
| `-i`          | copy images? true or false. Omit if false |
| `-v`          | copy videos? true or false. Omit if false |
| `-x`		    | delete files after copying? true or false. Omit if false |
| `-w`			| do not save file. Run a hypothetical scenario only. true or false. Omit if false |

## Roadmap

* Add an option to strip identifying metadata from media files
* ---Add an option to de-duplicate files based on a unique hashcode generated from the file contents (minus the metadata)---
* ---Use multithreaded file processing---

Disclaimer: Use at your own risk.