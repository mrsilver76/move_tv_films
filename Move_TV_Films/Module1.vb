'
' move_tv_films (version 1.3, 21st August 2019)
' Copyright © 2014-2019 Richard Lawrence
' https://github.com/mrsilver76/move_tv_films
'
' A program which moves all Windows Media Center recordings that are films
' into a different directory. This is useful if you wish to stop your "Recorded TV"
' directory from containing a mixture of films and TV and also if you view and/or
' manage these films in a third party application (such as Media Browser).

' This program is free software; you can redistribute it and/or modify it
' under the terms of the GNU General Public License as published by the
' Free Software Foundation; either version 2 of the License, or (at your
' option) any later version.
'
' This program is distributed in the hope that it will be useful, but
' WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General
' Public License for more details.
'
' ========================================================================
'

Imports System.IO
Imports System.Environment
Imports Toub.MediaCenter.Dvrms.Metadata
Imports System.Text.RegularExpressions

Module Main

    Dim commandLineArgs As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Application.CommandLineArgs
    Dim directoryFrom As String, directoryTo As String
    Dim testMode As Boolean = False, verboseMode As Boolean = False, pauseAfterEnd As Boolean = False
    Dim noDirectory As Boolean = False, noRename As Boolean = False, noYear As Boolean = False
    Dim silent As Boolean = False
    Dim logFileName As String
    Dim filmsFound As Integer = 0, showsFound As Integer = 0
    Public Const VERSION As String = "1.3"

    Enum ReplaceType
        Only_Path = 1
        Only_FileName = 2
        ' Both = 3    ' Not needed, so commented out
    End Enum

    Sub Main()

        If commandLineArgs.Count = 0 Then
            Call Display_Usage()
            End
        End If

        ' Parse command line arguments

        Dim theArg As String
        For Each theArg In commandLineArgs
            Select Case UCase(theArg)
                Case "/?", "-h", "--HELP"
                    ' Display help
                    Call Display_Usage()
                    End
                Case "/R", "-R", "--RECORDEDTV"
                    ' Use "Public Recorded TV" path
                    directoryFrom = Environment.GetEnvironmentVariable("PUBLIC") & "\Recorded TV"
                Case "/T", "-T", "--TEST"
                    ' Don't create folders or move/rename any films
                    testMode = True
                Case "/ND", "-ND", "--NO-DIRECTORY"
                    noDirectory = True
                Case "/NR", "-NR", "--NO-RENAME"
                    noRename = True
                Case "/NY", "-NR", "--NO-YEAR"
                    noYear = True
                Case "/V", "-V", "--VERBOSE"
                    ' Show extra logging information
                    verboseMode = True
                Case "/P", "-P", "--PAUSE"
                    ' Pause (and display "press [ENTER]" prompt) before quitting
                    pauseAfterEnd = True
                Case "/S", "-S", "--SILENT"
                    ' Don't display any output
                    silent = True
                Case Else
                    ' We've been supplied a path to a folder.
                    If directoryFrom = "" Then
                        directoryFrom = Remove_Invalid_Characters(theArg, ReplaceType.Only_Path)
                        If directoryFrom <> theArg Then Display_Usage(theArg & " contains invalid characters")
                    Else
                        directoryTo = Remove_Invalid_Characters(theArg, ReplaceType.Only_Path)
                        If directoryTo <> theArg Then Display_Usage(theArg & " contains invalid characters")
                    End If
            End Select
        Next

        ' Work out if folders are valid
        If directoryFrom = "" Then Call Display_Usage("Missing Recorded TV directory (or use /R for default)")
        If directoryTo = "" Then Call Display_Usage("Missing film directory")
        If System.IO.Directory.Exists(directoryFrom) = False Then Display_Usage(directoryFrom & " is not a valid directory")
        If System.IO.Directory.Exists(directoryTo) = False Then Display_Usage(directoryTo & " is not a valid directory")
        If LCase(directoryFrom) = LCase(directoryTo) Then Display_Usage("TV and film directories cannot be the same")

        Call Prepare_Logging()
        Call Scan_Folders()
        Call Shutdown()
        End

    End Sub

    ' Display_Usage
    ' Display the command line syntax. If errorMsg is supplied then this will be appended at the
    ' bottom of the output.

    Sub Display_Usage(Optional errorMsg As String = "")

        Console.WriteLine("Move_TV_Films (version " & VERSION & ")")
        Console.WriteLine("Moves films recorded in Windows Media Center into a different folder.")
        Console.WriteLine("Copyright (c) 2014-" & Now.Year & " Richard Lawrence.")
        Console.WriteLine("http://www.fourteenminutes.com/code/move_tv_films")
        Console.WriteLine()
        Console.WriteLine("Usage: " & Process.GetCurrentProcess.ProcessName() & ".exe [/T] [/V] [/P] [/ND] [/NR] [/NY] [/S] [/R | <tv dir>] <film dir>")
        Console.WriteLine()
        Console.WriteLine("    No args      Display help. This is the same as typing /?")
        Console.WriteLine("    /?           Display help. This is the same as not typing any options.")
        Console.WriteLine("    /T           Test mode. Do not move any files.")
        Console.WriteLine("    /V           Verbose mode. Log additional information during execution.")
        Console.WriteLine("    /P           Pause after executing and prompt for [ENTER] to be pressed.")
        Console.WriteLine("    /ND          Don't create a specific directory for the film.")
        Console.WriteLine("    /NR          Don't rename the recording, keep original filename.")
        Console.WriteLine("    /NY          Don't append the year to the directory and/or filename.")
        Console.WriteLine("    /S           Silent. Don't display any progress details.")
        Console.WriteLine("    /R           Look at recordings in the 'Recorded TV' directory.")
        Console.WriteLine("    <tv dir>     The dir to Recorded TV files. Required unless /R is used.")
        Console.WriteLine("    <film dir>   The dir where films should be moved to. Required.")
        Console.WriteLine()
        Console.WriteLine(" Log files can be found in: " & GetFolderPath(SpecialFolder.CommonApplicationData) & "\Move_TV_Films\Logs")
        Console.WriteLine()
        If errorMsg <> "" Then
            Console.WriteLine("Error: " & errorMsg)
        End If

        End

    End Sub

    ' Prepare_Logging
    ' Set up the location for the log files, display about and licence and delete any old log files.

    Sub Prepare_Logging()

        ' Create the appropriate folders for logs
        Dim appData As String = GetFolderPath(SpecialFolder.CommonApplicationData) & "\Move_TV_Films"
        If System.IO.Directory.Exists(appData) = False Then System.IO.Directory.CreateDirectory(appData)
        If System.IO.Directory.Exists(appData & "\Logs") = False Then System.IO.Directory.CreateDirectory(appData & "\Logs")
        logFileName = appData & "\Logs\Log " & Format(Now(), "d").Replace("/", "-") & ".txt"

        If silent = False Then
            Console.WriteLine("Move_TV_Films (version " & VERSION & ")")
            Console.WriteLine("Copyright (c) 2014-" & Now.Year & " Richard Lawrence.")
            Console.WriteLine("https://github.com/mrsilver76/move_tv_films")
            Console.WriteLine()
            Console.WriteLine("A program which moves Windows Media Center recordings that are films")
            Console.WriteLine("into a different folder. This is useful if you wish to view and manage")
            Console.WriteLine("these films in a third party application (such as Media Browser).")
            Console.WriteLine()
            Console.WriteLine("This program is free software; you can redistribute it and/or modify it")
            Console.WriteLine("under the terms of the GNU General Public License as published by the")
            Console.WriteLine("Free Software Foundation; either version 2 of the License, or (at your")
            Console.WriteLine("option) any later version.")
            Console.WriteLine()
            Console.WriteLine("This program is distributed in the hope that it will be useful, but")
            Console.WriteLine("WITHOUT ANY WARRANTY; without even the implied warranty of")
            Console.WriteLine("MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General")
            Console.WriteLine("Public License for more details. ")
            Console.WriteLine()
        End If


        Call Log("Move_TV_Films (version " & VERSION & ") starting...", False)
        ' Log variables used
        Call Log("Configuration: folderFrom = " & directoryFrom, True)
        Call Log("Configuration: folderTo = " & directoryTo, True)
        Call Log("Configuration: testMode = " & testMode & ", pauseAfterEnd = " & pauseAfterEnd, True)
        Call Log("Configuration: noDirectory = " & noDirectory & ", noRename = " & noRename, True)
        Call Log("Configuration: noYear = " & noYear & ", silent = " & silent, True)

        ' If /ND (no directory) and /NR (no rename) are used then no years will be appended anyway.
        ' As a result, warn if /NY is included since it's redundant.
        If noRename = True And noDirectory = True And noYear = True Then Call Log("Note: /NY redundant as both /NR and /ND are specified", False)

        ' Clean up old log files
        Dim logFile As String, logAge As Long
        For Each logFile In Directory.GetFiles(appData & "\Logs")
            If Left(Path.GetFileName(logFile), 4) = "Log " Then
                logAge = DateDiff("d", File.GetLastWriteTime(logFile), Now())
                Call Log("Examining log file: " & Path.GetFileName(logFile) & " (age: " & logAge & " days)", True)
                If logAge > 7 Then
                    Call Log("Deleting old log file: " & Path.GetFileName(logFile), True)
                    System.IO.File.Delete(logFile)
                End If
            End If
        Next

        ' Warn about test mode
        If testMode = True Then Call Log("TEST MODE ENABLED. NO FILES WILL BE MOVED.", False)

    End Sub

    ' Scan_Folders
    ' Look through all the files in the "folderFrom" folder, trying to work out which ones are films.
    ' If one is found, then it will be processed.

    Sub Scan_Folders()

        Call Log("Looking for films in " & directoryFrom, False)

        Dim theFile As String, theExt As String, newTitle As String
        Dim isMovie As Boolean, mediaIsMovie As String

        For Each theFile In Directory.GetFiles(directoryFrom)
            theExt = LCase(Path.GetExtension(theFile))
            If theExt = ".wtv" Or theExt = ".dvrms" Or theExt = ".dvr-ms" Then

                ' Get the meta-data for this film
                On Error Resume Next
                Dim metaData As New Toub.MediaCenter.Dvrms.Metadata.DvrmsMetadataEditor(theFile)
                If Err.Number = 0 Then
                    ' Is this a movie?
                    mediaIsMovie = Get_Meta(metaData, "WM/MediaIsMovie")
                    Select Case mediaIsMovie.ToLower
                        Case "true"
                            isMovie = True
                        Case "false"
                            isMovie = False
                        Case ""
                            isMovie = False
                            Call Log("WM/MediaIsMovie is empty, assuming false", True)
                        Case Else
                            Call Log("Unexpected value from WM/MediaIsMovie (" & mediaIsMovie & "), assuming false", True)
                            isMovie = False
                    End Select
                Else
                    ' We have a problem
                    Call Log("Couldn't get metadata for " & theFile & ", assuming TV show", True)
                    isMovie = False
                End If

                If isMovie = False Then
                    ' This is a TV show, so will be ignored
                    Call Log("Found show: " & Path.GetFileName(theFile), True)
                    showsFound += 1
                Else
                    ' We have a film, lets rename and move it.
                    Call Log("Found film: " & Path.GetFileName(theFile), False)
                    newTitle = Get_New_Title(metaData)
                    Call Log("New format: " & newTitle, False)
                    Call Move_File(theFile, newTitle)
                End If
            End If
        Next

    End Sub

    ' Plural_S
    ' Given a number, returns an "s" if it is not a one. This avoids having to write "film(s)" because
    ' you're too lazy to work out whether or not "film" should be plural or not.

    Function Plural_S(number As Integer) As String

        If number = 1 Then
            Plural_S = ""
        Else
            Plural_S = "s"
        End If

    End Function

    ' Shutdown
    ' Report final usage and pause depending if the user asked for that.

    Sub Shutdown()

        Call Log("Ignored " & showsFound & " TV show" & Plural_S(showsFound) & " and moved " & filmsFound & " film" & Plural_S(filmsFound) & ".", False)
        Call Log("Move_TV_Films finished.", False)
        Call Log("", False)

        If pauseAfterEnd = True Then
            Console.WriteLine()
            Console.WriteLine("Press [ENTER] or [RETURN] to end this program.")
            Console.ReadLine()
        End If

        End

    End Sub

    ' Move_File
    ' Given the name of a file and it's new title (with or without year), move it into a folder
    ' in the format:  <folderTo>\<title>\<title>.ext
    ' NOTE: noDirectory and noRename will override this path!

    Sub Move_File(file As String, title As String)

        Dim newFolder As String, newFile As String

        ' Do we want to create a directory for this?
        If noDirectory = True Then
            newFolder = directoryTo
        Else
            newFolder = directoryTo & "\" & title
        End If

        ' Do we want to rename the file?
        If noRename = True Then
            newFile = IO.Path.GetFileName(file)
        Else
            newFile = title & IO.Path.GetExtension(file)
        End If

        ' If we are going to create a folder, check if it already exists

        If noDirectory = False Then
            If System.IO.Directory.Exists(newFolder) = True Then
                Call Log(newFolder & " already exists, continuing anyway", False)
            Else
                Call Log("Creating " & newFolder, True)
                Try
                    If testMode = True Then
                        Call Log("TEST MODE. Directory not created.", False)
                    Else
                        System.IO.Directory.CreateDirectory(newFolder)
                    End If
                Catch ex As Exception
                    Call Log("Unable to create directory: " & ex.Message, False)
                    Exit Sub
                End Try
            End If
        End If

        ' Check if the file exists

        If System.IO.File.Exists(newFolder & "\" & newFile) = True Then
            Call Log(newFolder & "\" & newFile & " already exists, not moving this", False)
            ' TO DO: Replace the file if it is bigger and doesn't have DRM
            Exit Sub
        End If

        ' Move the file

        Call Log("Moving file to " & newFolder & "\" & newFile, False)
        Try
            If testMode = True Then
                Call Log("TEST MODE. Move did not happen.", False)
            Else
                System.IO.File.Move(file, newFolder & "\" & newFile)
                filmsFound += 1
            End If
        Catch ex As Exception
            Call Log("Unable to move file: " & ex.Message, False)
            Exit Sub
        End Try

    End Sub

    ' Get_New_Title
    ' Given the pointer to the metadata of the recording we are examining, try to work out what
    ' the actual title of the film is and the year that it was filmed. 

    Function Get_New_Title(md As Toub.MediaCenter.Dvrms.Metadata.DvrmsMetadataEditor) As String

        Dim temp As Integer

        Get_New_Title = Get_Meta(md, "Title")
        Call Log("Film title is: " & Get_New_Title, True)

        If noYear = False Then
            ' Try to get the film's year. This will be in one of the following places in this order:
            ' Year -> WM/OriginalReleaseTime -> WM/SubTitleDescription

            temp = Get_Year(Get_Meta(md, "Year"))  ' Probably overkill, but just in case year is oddly formatted
            If temp = -1 Then
                Call Log("Year is invalid, trying WM/OriginalReleaseTime", True)
                temp = Get_Year(Get_Meta(md, "WM/OriginalReleaseTime"))
                If temp = -1 Then
                    Call Log("WM/OriginalReleaseTime is invalid, trying WM/SubTitleDescription for year", True)
                    temp = Get_Year(Get_Meta(md, "WM/SubTitleDescription"))
                End If
            End If

            If temp = -1 Then
                Call Log("Film year is: (unknown)", True)
            Else
                Call Log("Film year is: " & temp, True)
                Get_New_Title &= " (" & temp & ")"
            End If
        End If

        ' Now we need to remove all the invalid characters
        Get_New_Title = Remove_Invalid_Characters(Get_New_Title, ReplaceType.Only_FileName)

    End Function

    ' Remove_Invalid_Characters
    ' Remove any characters that aren't allowed in a path and filename. toReplace indicates
    ' whether or not we are cleaning up a path, a filename or both.

    Function Remove_Invalid_Characters(file As String, toReplace As ReplaceType) As String

        Remove_Invalid_Characters = file
        Dim invalidChar As String

        ' TO DO: Find out how to create one char() and then copy only Path.GetInvalidFileNameChars()
        ' or Path.GetInvalidPathChars(), rather than having code repetition.

        If toReplace = ReplaceType.Only_FileName Then
            For Each invalidChar In Path.GetInvalidFileNameChars()
                Remove_Invalid_Characters = Remove_Invalid_Characters.Replace(invalidChar, "")
            Next
        End If

        If toReplace = ReplaceType.Only_Path Then
            For Each invalidChar In Path.GetInvalidPathChars()
                Remove_Invalid_Characters = Remove_Invalid_Characters.Replace(invalidChar, "")
            Next
        End If

    End Function

    ' Get_Year
    ' Given a string, see if a four digit year can be extracted from it. If it cannot
    ' then -1 is returned.

    Function Get_Year(text As String) As Integer

        Get_Year = -1
        If text = "" Then Exit Function

        Dim thisYear As Integer

        thisYear = Now.Year
        Dim match As Match = Regex.Match(text, "\d\d\d\d")

        If match.Success = False Then
            Call Log("Couldn't find any year in: " & text, True)
            Exit Function
        End If

        While match.Success = True
            If match.Value <> "" Then
                ' If the year found is later than today then it is highly unlikely this
                ' was the year the film was produced!
                If CDbl(match.Value) <= thisYear Then Get_Year = CInt(match.Value)
            End If
            match = match.NextMatch
        End While

    End Function

    ' Log
    ' Given some text output it to the display and also log it. isVerbose is used to
    ' indicate whether or not this is a verbose log entry. If the program has not been
    ' run with /V then this will not be set and so these log entries will be ignored.

    Sub Log(text As String, isVerbose As Boolean)

        Dim newLog As Boolean = False

        ' Don't log anything if there is no log file defined or this is a
        ' verbose log entry and the user has not asked for that.
        If logFileName = "" Then Exit Sub
        If isVerbose = True And verboseMode = False Then Exit Sub

        ' Format the log entry
        Dim logEntry As String = "[" & Format(Now(), "T") & "] " & text

        ' Are we creating a new log file?
        If System.IO.File.Exists(logFileName) = False Then newLog = True

        Dim logStream As New System.IO.StreamWriter(logFileName, True)

        ' If an empty line is passed to the log file then write just that empty line
        If text <> "" Then
            logStream.WriteLine(logEntry)
            If silent = False Then Console.WriteLine(logEntry)
        Else
            logStream.WriteLine()
        End If

        logStream.Close()

    End Sub

    ' Get_Meta
    ' Given the key of a WTV/DVRMS file, return its value. Although not strictly necessary, it makes the
    ' code cleaner as this line is quite verbose.

    Function Get_Meta(ed As Toub.MediaCenter.Dvrms.Metadata.DvrmsMetadataEditor, sKey As String) As String

        Try
            Get_Meta = Toub.MediaCenter.Dvrms.Metadata.DvrmsMetadataEditor.GetMetadataItemAsString(ed.GetAttributes(), sKey)
        Catch ex As Exception
            Call Log("Exception trying to get " & sKey & ", ignoring", True)
            Get_Meta = ""
        End Try

    End Function

End Module
