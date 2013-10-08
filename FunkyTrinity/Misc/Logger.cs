﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using Zeta.Common;

namespace FunkyBot
{
	 [Flags]
	 public enum LogLevel
	 {
		  None=0,
		  User=1,
		  Execption=2, //doh?
		  Cluster=4,
		  Grouping=8,
		  Movement=16,
		  Ability=32,
		  Target=64,
		  Items=128,
		  OutOfGame=256,
		  OutOfCombat=512,

		  All=User|Execption|Cluster|Grouping|Movement|Ability|Target|Items|OutOfGame|OutOfCombat,
	 }
	 public static class Logger
	 {

		  internal static void CleanLogs()
		  {
				List<string> deleteList=new List<string>();
				if (string.IsNullOrEmpty(FolderPaths.sDemonBuddyPath))
				{
					 Logging.Write("Failure to reconigze demon buddy path!");

				}
				else
				{
					 foreach (string file in System.IO.Directory.GetFiles(FolderPaths.sDemonBuddyPath+@"\Logs\"))
					 {
						  DateTime curFileCreated=System.IO.Directory.GetCreationTime(file);
						  if (DateTime.Now.Subtract(curFileCreated).TotalHours>=24)
						  {
								deleteList.Add(file);
						  }
					 }

					 if (deleteList.Count>0)
					 {
						  foreach (string item in deleteList)
						  {
								System.IO.File.Delete(item);
						  }
						  Logging.WriteDiagnostic("Total DB logs deleted "+deleteList.Count);
					 }
				}

				string ItemRulesPath=@"\Plugins\FunkyTrinity\ItemRules\Log\Archive\";
				deleteList=new List<string>();
				try
				{
					 foreach (string file in System.IO.Directory.GetFiles(FolderPaths.sDemonBuddyPath+ItemRulesPath))
					 {
						  DateTime curFileCreated=System.IO.Directory.GetCreationTime(file);
						  if (DateTime.Now.Subtract(curFileCreated).TotalHours>=24)
						  {
								deleteList.Add(file);
						  }
					 }

					 if (deleteList.Count>0)
					 {
						  foreach (string item in deleteList)
						  {
								System.IO.File.Delete(item);
						  }
						  Logging.WriteDiagnostic("Total item rule logs deleted "+deleteList.Count);
					 }

				} catch { Logging.WriteDiagnostic("Failure to clean log files @ path: "+ItemRulesPath); }

				string ProfileLogs=@"\Plugins\FunkyTrinity\Log\ProfileStats\";
				deleteList=new List<string>();
				try
				{
					 foreach (string file in System.IO.Directory.GetFiles(FolderPaths.sDemonBuddyPath+ProfileLogs))
					 {
						  DateTime curFileCreated=System.IO.Directory.GetCreationTime(file);
						  if (DateTime.Now.Subtract(curFileCreated).TotalDays>=1)
						  {
								deleteList.Add(file);
						  }
					 }

					 if (deleteList.Count>0)
					 {
						  foreach (string item in deleteList)
						  {
								System.IO.File.Delete(item);
						  }
						  Logging.WriteDiagnostic("Total item rule logs deleted "+deleteList.Count);
					 }

				} catch { Logging.WriteDiagnostic("Failure to clean log files @ path: "+ProfileLogs); }

		  }

		  private static string dbLogFile;
		  internal static string DBLogFile
		  {
				get { return Logger.dbLogFile; }
				set { Logger.dbLogFile=value; Init(); }
		  }

		  internal delegate string GetLogLevelName(object obj);

		  private static string[] FilePath;

		  internal static readonly string FileNamePrefix="FunkyLog - ";

		  private static string filename;
		  public static string FunkyLogFilename
		  {
				get { return filename; }
				set { filename=value; }
		  }

		  public static void Init()
		  {
				FilePath=Path.GetFileName(DBLogFile).Split(Char.Parse(" "));
				filename=Path.Combine(FolderPaths.sDemonBuddyPath, "Logs", FileNamePrefix+FilePath[1]+" "+FilePath[2]);
		  }
		  public static void Write(LogLevel level, string Message,bool WriteToMainLog, params object[] args)
		  {
				string prefix="["+DateTime.Now.ToString("hh:mm:ss.fff")+" "+level.ToString()+"]";
				string message=String.Format(Message, args);
				WriteLine(String.Format("{0} {1}", prefix, message), true);
				if (WriteToMainLog)
					 Zeta.Common.Logging.Write("{0} {1}","[Funky]", message);
		  }
		  public static void Write(LogLevel level, string Message, params object[] args)
		  {
				string prefix="["+DateTime.Now.ToString("hh:mm:ss.fff")+" "+level.ToString()+"]";
				string message=String.Format(Message, args);
				WriteLine(String.Format("{0} {1}", prefix, message), true);
		  }
		  private static void WriteLine(string text, bool append)
		  {
				// open file
				// If an error occurs throw it to the caller.
				try
				{

					 StreamWriter Writer=new StreamWriter(FunkyLogFilename, append, Encoding.UTF8);
					 if (!String.IsNullOrEmpty(text)) Writer.WriteLine(text);
					 Writer.Flush();
					 Writer.Close();
				} catch
				{
					 throw;
				}
		  }
	 }



	 public static class FolderPaths
	 {
		  internal static string sDemonBuddyPath=Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		  internal static string sTrinityPluginPath=sDemonBuddyPath+@"\Plugins\FunkyTrinity\";
		  internal static string sTrinityLogPath=sDemonBuddyPath+@"\Plugins\FunkyTrinity\Log\";
		  internal static string SettingsDefaultPath=Path.Combine(FolderPaths.sTrinityPluginPath, "Config", "Defaults");


		  internal static string sFunkySettingsPath
		  {
				get
				{
					 if (Bot.CurrentAccountName==null)
						  Bot.UpdateCurrentAccountDetails();

					 string sFunkyCharacterFolder=Path.Combine(FolderPaths.sDemonBuddyPath, "Settings", "FunkyTrinity", Bot.CurrentAccountName);
					 if (!System.IO.Directory.Exists(sFunkyCharacterFolder))
					 {
						  Logging.WriteDiagnostic("Creating Funky Settings Folder @ {0}", sFunkyCharacterFolder);
						  System.IO.Directory.CreateDirectory(sFunkyCharacterFolder);
					 }

					 return sFunkyCharacterFolder;
				}
		  }
		  internal static string sFunkySettingsCurrentPath
		  {
				get
				{
					 if (Bot.CurrentHeroName==null)
						  Bot.UpdateCurrentAccountDetails();

					 return Path.Combine(sFunkySettingsPath, Bot.CurrentHeroName+".xml");

				}
		  }
	 }
}
