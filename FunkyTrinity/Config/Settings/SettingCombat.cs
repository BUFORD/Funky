﻿using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using FunkyTrinity.Enums;
namespace FunkyTrinity.Settings
{
	public class SettingCombat
	{
		 public double GlobeHealthPercent { get; set; }
		 public double PotionHealthPercent { get; set; }
		 public double HealthWellHealthPercent { get; set; }

		 public SettingCombat()
		 {
			  GlobeHealthPercent=0.6d;
			  PotionHealthPercent=0.5d;
			  HealthWellHealthPercent=0.75d;
		 }

		 private static string DefaultFilePath=Path.Combine(Funky.FolderPaths.sTrinityPluginPath, "Config", "Defaults", "Combat_Default.xml");
		 public static SettingCombat DeserializeFromXML()
		 {
			  XmlSerializer deserializer=new XmlSerializer(typeof(SettingCombat));
			  TextReader textReader=new StreamReader(DefaultFilePath);
			  SettingCombat settings;
			  settings=(SettingCombat)deserializer.Deserialize(textReader);
			  textReader.Close();
			  return settings;
		 }
		 public static SettingCombat DeserializeFromXML(string Path)
		 {
			  XmlSerializer deserializer=new XmlSerializer(typeof(SettingCombat));
			  TextReader textReader=new StreamReader(Path);
			  SettingCombat settings;
			  settings=(SettingCombat)deserializer.Deserialize(textReader);
			  textReader.Close();
			  return settings;
		 }
	}
}
