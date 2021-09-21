# Media Backup

This is a .Net Standard library and sample program, which allows media files to be read from a source and transferred to a destination.
The default instllation ships with a FileSystemProcessor, which copies file to a folder-based destination.
The destination files are arranged by yyyy/mm/dd

The pogram is laid out into the following projects:

* media_backup.shared - contains shared interface and image helper functions
* media_backup - this is a sample .Net Core console program
* media_backup.filesystemprocessor - is an implementation of the `IMediaProcessor` interface, which copies media files to a filesystembased destination

## How to run the console program

Note: For the true or false parameters, skip the parameter if it is false. If you provide a parameter, it will be intepreted as true.

To copy files from source folder to destination folder
```
dotnet media-backup.dll -s C:\temp\Pictures\Source -d D:\Temp\Destination -i -v 
```

To copy files from source folder to destination folder and delete source files are copying.
```
dotnet media-backup.dll -s C:\temp\Pictures\Source -d D:\Temp\Destination -i -v -x
```

Simulate a file copy but do not actually copy files. This is useful to produce any error reports about problems with file metadata.
```
dotnet media-backup.dll -s C:\temp\Pictures\Source -d D:\Temp\Destination -i -v -w
```

## Arguments
| Argument        | Description |
| ------------- |:-------------:|
| `-s`          | source folder  |
| `-d`          | destination folder |
| `-i`          | copy images? true or false. Omit if false |
| `-v`          | copy videos? true or false. Omit if false |
| `-x`		    | delete files after copying? true or false. Omit if false |
| `-w`			| do not save file. Run a hypothetical scenario only. true or false. Omit if  |

## Output
Two files are produced:

* `dd-MM-yyyy-HH-mm-ss-Log.log` - contain a dump of all the log statements you see on the console

* `dd-MM-yyyy-HH-mm-ss-Processing-Result.csv` - a csv file contain the filename, result (Success or Error) and reason (of error)

## Roadmap

* Add an option to strip identifying metadata from media files
* Add an option to de-duplicate files based on a unique hashcode generated from the file contents (minus the metadata)
* Use multithreaded file processing