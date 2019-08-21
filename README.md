# move_tv_films

Moves all Windows Media Center recordings that are films into a different directory. This is useful if you wish to stop your "Recorded TV" directory from containing a mixture of films and TV and also if you view and/or manage these films in a third party application (such as [Emby](https://emby.media/), [Kodi](https://kodi.tv/) or [Plex](https://www.plex.tv/)).

## Features

This program has the following features:

1.  Highly configurable command line based program for running as a one off or as a scheduled task.
2.  Looks at WTV or DVR-MS files (with extensions `wtv`, `dvr-ms` or `dvrms`).
3.  Can automatically find the "Public Recorded TV" path or let you use any other location.
4.  Extracts the correct film name from the meta-data.
5.  Extracts the correct year from the meta-data. Will try several places within the meta-data (including the description) if the year is missing from the standard "year" field.
6.  Creates any missing directories and renames/moves the file using the standard XBMC/Plex/Media Browser naming format.
7.  Can be configured not to create directories and/or not to rename recordings.
8.  Test mode which doesn't move files or create directories.
9.  Seven days worth of logs kept.
10.  Verbose logging mode, useful to see what is going on.

## Limitations and known issues

1.  If there already is a film with that file name, the move will not occurr.
2.  It's not possible to prevent it from running if something is being recorded.
3.  The program relies on the EPG data provider tagging films correctly. If they don't (which is rare), there are no additional sanity checks.

## Requirements

In order to use this program you need the following:

*   A computer running Microsoft Windows and Windows Media Center.
*   Some recorded television shows in WTV or DVR-MS format.
*   Knowledge of running command line applications.

This program is not recommended for people who are not comfortable with the workings of Microsoft Windows, command line applications and the scheduling of tasks.

## Installation and usage

1.  Copy the program `move_tv_films.exe` and the `Toub.MediaCenter.Dvrms.dll` file into any directory on your computer. One possible option is the Documents directory. The program will not run without the accompanying DLL.
2.  Double clicking on the program will pop up a message displaying the command line options.
3.  To run the program from the command line, you should enter the following command from within a DOS window: `move_tv_films.exe [options]`where `[options]` are the possible options detailed in the next section.
4.  All logs are stored within the ProgramData directory. The easiest way to access is to enter the following either in the start menu search, from the Run command or in the address of the Explorer window: `%programdata%\move_tv_films` This will open the browser window at the location of the logs.
5.  You can call this program automatically using Windows Task Scheduler. For more details, see the section later.
6.  You can call this program by double-clicking on an icon. To do this you need to right-click on the program and select "Create Shortcut". When the shortcut appears, right-click on that, select "Properties" and add the command line options to the end of the section entitled "Target".

## Command line options overview

    Move_TV_Films.exe [/T] [/V] [/P] [/ND] [/NR] [/NY] [/S] [/R | <tv dir>] <film dir>
    
        No args      Display help. This is the same as typing /?
        /?           Display help. This is the same as not typing any options.
        /T           Test mode. Do not move any files.
        /V           Verbose mode. Log additional information during execution.
        /P           Pause after executing and prompt for [ENTER] to be pressed.
        /ND          Don't create a specific directory for the film.
        /NR          Don't rename the recording, keep original filename.
        /NY          Don't append the year to the directory and/or filename.
        /S           Silent. Don't display any progress details.
        /R           Look at recordings in the 'Recorded TV' directory.
        <tv dir>     The dir to Recorded TV files. Required unless /R is used.
        <film dir>   The dir where films should be moved to. Required.

**Please note!** The `[` and `]` denote optional arguments and the `<` and `>` denote mandatory arguments, both of which should <u>not</u> be used as part of the command line. If you are unsure, please refer to the examples further down this README.**

## Command line options details

1.  Options are prefixed with a `/`. If you prefer, you can use `-` (for example, `-R` and `/R` are the same).
2.  If your choice of command line options means that you have to supply two paths, then the path to films directory where you want to move the recordings to _**always comes after**_ the Recorded TV path.
3.  When supplying paths, if there are any spaces in the path then it _**must**_ be enclosed in speech marks. To be safe, it is recommended to always use speech marks.
4.  If you try and supply the same directory name for both the location of recordings and the films directory then the program will warn you and exit.

### Test mode. Do not move any files (/T)

Runs the program as normal but does not create any directories, rename any recordings or move them. Useful if you want to see what will happen before you run the program properly. You may find that you need to also use `/V` to fully understand what is happening.

### Verbose mode. Log additional information during execution (/V)

Logs (and displayed to the screen) even more information about what the program is doing during running. Useful for debugging and the curious. Be warned that the extra debugging information will slow down the running of the program significantly and generate much larger logs.

If you plan to send these log files by email, then using zip to compress them is highly recommended.

### Pause after running (/P)

If you use this command then after the program has finished running, you will be presented with a prompt to press Enter before it finishes. This is not recommended if you plan to run the command from a batch file or from a scheduled task.

**Note:** Use of both `/P` and `/S` will not cause the prompt to be hidden.

### Don't create a specific directory for the film (/ND)

If you use this command then the program will not create a specific directory for each film (which is the title and year of the film) and, instead, just move the recording into the directory specified by `<film dir>`.

**Note:** The majority of people will not need to use this option.

### Don't rename the recording, keep original filename (/NR)

If you use this command then the program will not rename the recording (to the title and year of the film) and, instead, just keep the original filename that Windows Media Center used. For example, `Double_Jeopardy_Film4_2014_10_21_20_58_00.wtv`.

**Note:** The majority of people will not need to use this option.

### Don't append the year to the directory and/or filename (/NY)

If you use this command then the program will not include the year of the film in either the name of the file or the name of the directory.

**Note:** The majority of people will not need to use this option.

### Silent. Don't display any progress details (/S)

If you use this command then the program will not display any output. Logs will still continue to be written so you can read those to find out the outcome.

**Note:** If you use `/S` and `/P` together, then the message asking you to press Enter will be displayed to the screen.

### Look at shows in the Public 'Recorded TV' location (/R)

Tells the program to assume that recorded television shows are located in the public 'Recorded TV' directory. This varies depending on how Windows was installed and the program is clever enough to cope if you have this in a non-standard location or drive (for most people running Windows 7 it is `C:\Users\Public\Recorded TV`). If you use this command then you do not need to supply a path to the 'Recorded TV' directory.

**Note:** If you use `/R` and supply two directories, then the directory found by `/R` will take priority and the other one will be ignored.

### \<tv dir\>

This is the directory where your 'Recorded TV' files are kept. If you don't know where this is then you can use `/R` instead and the program will attempt to work this out for you.

### \<film dir\>

This is the directory where you would like the recordings to be moved to. Films found by the program will be added to this directory. As long as you haven't used the `/ND` and/or `/NR` options then each film will have their own directory created and the recording placed **_inside_** that directory with the same name as the directory.

For example, if you specify `D:\TV Movies` as the film directory and you record [Gladiator](http://www.imdb.com/title/tt0172495/) on television then a directory `D:\TV Movies\Gladiator (2000)` will be created and the film placed inside that directory.

Using the example above, the final location of the film will be `D:\TV Movies\Gladiator (2000)\Gladiator (2000).wtv`.

**Note:** Use of the `/ND`, `/NR` and `/NY` options will change the final location of the film. For more details, refer to the following table:

| **Additional options** | **Final path to film** | 
|---|---|
| (none) | `D:\TV Movies\Gladiator (2000)\Gladiator (2000).wtv` (recommended) | 
| `/ND` | `D:\TV Movies\Gladiator (2000).wtv` | 
| `/NR` | `D:\TV Movies\Gladiator (2000)\Gladiator_BBC1_2013_00_20_21_30_00.wtv` | 
| `/NY` | `D:\TV Movies\Gladiator\Gladiator.wtv` | 
| `/ND /NR` | `D:\TV Movies\Gladiator_BBC1_2013_00_20_21_30_00.wtv` | 
| `/ND /NY` | `D:\TV Movies\Gladiator.wtv` | 
| `/NR /NY` | `D:\TV Movies\Gladiator\Gladiator_BBC1_2013_00_20_21_30_00.wtv` | 
| `/ND /NR /NY` | `D:\TV Movies\Gladiator_BBC1_2013_00_20_21_30_00.wtv` (Note: `/NY` is redundant.) | 

## Example usage

Below are a couple of recommended command line uses:

    move_tv_films.exe /R "D:\Movies"

Move films from the public 'Recorded TV' directory (`/R`) into `D:\Movies`.

    move_tv_films.exe /V "D:\Recorded TV" "E:\My Movies"

Move films from the `D:\Recorded TV` directory into `E:\My Movies`. Display additional information to aid in debugging (`/V`)

    move_tv_films.exe /T /P /R "C:\Users\Public\Movies"

Pretend to move (`/T`) films from public 'Recorded TV' directory (`/R`) into `"C:\Users\Public\Movies`. Display a full set of logs (`/V`) and pause before exiting (`/P`).

## Scheduled task

The recommended method to run this program is via a scheduled task. [This Microsoft website](http://windows.microsoft.com/en-US/windows7/schedule-a-task) explains how to create one in Windows 7\. Some key points to know:

1.  Using "Create Task..." instead of "Create Basic Task..." will give you more flexibility.
2.  The trigger could be one or more times a day (based around your viewing habits) or when the computer turns on.
3.  In Actions: The "Program/Script" should point to `move_tv_movies.exe`. The "Add arguments" should be all the command line options. You do not need to fill out "Start in"

## Recommended setup

There is no recommended setup. If you are interested, my HTPC runs a batch file twice a day (at midday and 5pm) which removes duplicate recordings (see [wmc-dedupe](http://www.fourteenminutes.com/code/wmc-dedupe)) and then the following:

    move_tv_films.exe /R "D:\TV Movies"

This moves all films from the 'Recorded TV' directory (on my SSD) into a new directory on an external HDD. I then use [Plex](https://www.plex.tv) to navigate, manage and view these films.

## Showing only films in the Windows Media Center "Movie Library"

It is not possible to display films organised this way in Windows Media Center. It is recommended that you look at media library software such as [Emby](https://emby.media/), [Kodi](https://kodi.tv/) or [Plex](https://www.plex.tv/).
