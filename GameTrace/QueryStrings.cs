using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameTrace
{
    public static class QueryStrings
    {
        // GAMETRACE DATABASE
        public static string connstr = "Data Source= XE;User Id=GameTrace;Password=ftn;";
        public static string selectGames = @"select GAMEID, GAMENAME,GAMEDR from GAME";
        public static string updateGameUserPlaying1 = "UPDATE GAME_USER SET PLAYING_GAME_ID = ";
        public static string updateGameUserPlaying2 = ", PLAYING= 1 WHERE UNAME= '";
        public static string playsInsert1 = @"INSERT INTO PLAYS VALUES ('";
        public static string playsInsert2 = ", DEFAULT, DEFAULT)";
        public static string updateGameUserNotPlaying = "UPDATE GAME_USER SET PLAYING_GAME_ID = NULL, PLAYING= 0 WHERE UNAME= '";
        public static string selectUserUname = @"select COUNT(*) from GT_USER WHERE UNAME = '";
        public static string selectUserPword = "' AND PWORD = '";
        public static string selectUserUtype = "' AND USERUT=1";

        // PROCESS QUERY
        public static string selectCMDWhereProcessId = "SELECT CommandLine FROM Win32_Process WHERE ProcessId = ";
        public static string selectExecPathWhereProcessId = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = ";

        public static string instanceCreation = "Select * From __InstanceCreationEvent Within 1 Where TargetInstance ISA 'Win32_Process' ";
        public static string instanceDeletion = "Select * From __InstanceDeletionEvent Within 1 Where TargetInstance ISA 'Win32_Process' ";
    }
}
