# Media Backup

This is a .Net Standard library and sample program, which allows media files to be read from a source and transferred to a destination.
The default instllation ships with a FileSystemProcessor, which copies file to a folder-based destination.

The pogram is laid out into the following projects:

* media_backup.shared - contains shared interface and image helper functions
* media_backup - this is a sample .Net Core console program
* media_backup.filesystemprocessor - is an implementation of the `IMediaProcessor` interface, which copies media files to a filesystembased destination

## How to run the console program
```
dotnet run media-backup.dll -s C:\temp\Pictures -d D:\Temp\Copytest -i true -v false
```

## Arguments
| Argument        | Description |
| ------------- |:-------------:|
| `-s`          | source folder  |
| `-d`          | destination folder |
| `-i`          | copy images? true or false |
| `-v`          | copy videos? true or false |
