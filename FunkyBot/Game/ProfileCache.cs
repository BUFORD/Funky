﻿using System;
using System.Linq;
using System.Collections.Generic;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Settings;
using Zeta.CommonBot.Profile.Common;
using FunkyBot.Cache;

namespace FunkyBot.Game
{
	 public class ProfileCache
	 {
		  // A list of "useonceonly" tags that have been triggered this xml profile
		  internal static HashSet<int> hashUseOnceID=new HashSet<int>();
		  internal static Dictionary<int, int> dictUseOnceID=new Dictionary<int, int>();
		  // For the random ID tag
		  internal static Dictionary<int, int> dictRandomID=new Dictionary<int, int>();


		  internal bool PreformingInteractiveBehavior
		  {
			  get
			  {
				  return this.IsRunningOOCBehavior && this.ProfileBehaviorIsOOCInteractive && this.InteractableCachedObject != null;
			  }
		  }

		  internal Dictionary<int, Cache.CacheObject> InteractableObjectCache = new Dictionary<int, Cache.CacheObject>();
		  private bool profileBehaviorIsOOCInteractive=false;
		  internal bool ProfileBehaviorIsOOCInteractive
		  {
				get { return profileBehaviorIsOOCInteractive; }
				set { profileBehaviorIsOOCInteractive=value; }
		  }

		  internal CacheObject InteractableCachedObject = null;

		  private ProfileBehavior currentProfileBehavior;
		  internal ProfileBehavior CurrentProfileBehavior
		  {
				get { return currentProfileBehavior; }
		  }

		  private DateTime LastProfileBehaviorCheck=DateTime.Today;
		  ///<summary>
		  ///Tracks Current Profile Behavior and sets IsRunningOOCBehavior depending on the current Type of behavior.
		  ///</summary>
		  internal void CheckCurrentProfileBehavior()
		  {
				if (DateTime.Now.Subtract(LastProfileBehaviorCheck).TotalMilliseconds>450)
				{
					 LastProfileBehaviorCheck=DateTime.Now;

					 if (currentProfileBehavior==null
						  ||ProfileManager.CurrentProfileBehavior!=null
						  &&ProfileManager.CurrentProfileBehavior.Behavior!=null
						  &&currentProfileBehavior.Behavior.Guid!=ProfileManager.CurrentProfileBehavior.Behavior.Guid)
					 {
						  currentProfileBehavior=ProfileManager.CurrentProfileBehavior;
						  if (Bot.Settings.Debug.FunkyLogFlags.HasFlag(LogLevel.Event))
							  Logger.Write(LogLevel.Event, "Profile Behavior Changed To {0}", currentProfileBehavior.GetType().ToString());

						  Type profileTagType=currentProfileBehavior.GetType();
						  if (oocDBTags.Contains(profileTagType))
						  {
							  if (InteractiveTags.Contains(profileTagType))
							  {
								  ProfileBehaviorIsOOCInteractive = true;
								  Logging.WriteDiagnostic("Interactable Profile Tag!");

								  InteractableCachedObject = ProfileCache.GetInteractiveCachedObject(currentProfileBehavior);
								  if (InteractableCachedObject != null)
									  Logging.WriteDiagnostic("Found Cached Interactable Server Object");
								  
							  }
							  else
							  {
								  ProfileBehaviorIsOOCInteractive = false;
								  InteractableCachedObject = null;
							  }

							  Logging.WriteDiagnostic("Current Profile Behavior has enabled OOC Behavior.");
							  IsRunningOOCBehavior = true;
						  }
						  else
						  {
							  ProfileBehaviorIsOOCInteractive = false;
							  InteractableCachedObject = null;
							  IsRunningOOCBehavior = false;
						  }
					 }

				}
		  }
		 internal bool IsRunningOOCBehavior { get; set; }

		 internal static CacheObject GetInteractiveCachedObject(ProfileBehavior tag)
		 {
			 Type TagType = tag.GetType();
			 if (InteractiveTags.Contains(TagType))
			 {
				 if (TagType == typeof(UseWaypointTag))
				 {
					 UseWaypointTag tagWP = (UseWaypointTag)tag;
					 var WaypointObjects=Bot.Game.Profile.InteractableObjectCache.Values.Where(obj => obj.SNOID==6442);
					 foreach (CacheObject item in WaypointObjects)
					 {
						 if (item.Position.Distance(tagWP.Position)<100f)
						 {
							 //Found matching waypoint object!
							 return item;
						 }
					 }
				 }
				 else if (TagType == typeof(UseObjectTag))
				 {
					 UseObjectTag tagUseObj = (UseObjectTag)tag;
					 if (tagUseObj.ActorId>0)
					 {//Using SNOID..
						 var Objects = Bot.Game.Profile.InteractableObjectCache.Values.Where(obj => obj.SNOID == tagUseObj.ActorId);
						 foreach (CacheObject item in Objects.OrderBy(obj => obj.Position.Distance(Bot.Character.Position)))
						 {
							 //Found matching object!
							 return item;
						 }

					 }else
					 {//use position to match object
						 Vector3 tagPosition=tagUseObj.Position;
						 var Objects = Bot.Game.Profile.InteractableObjectCache.Values.Where(obj => obj.Position.Distance(tagPosition)<=100f);
						 foreach (CacheObject item in Objects)
						 {
							 //Found matching object!
							 return item;
						 }
					 }
				 }
				 else if (TagType == typeof(UsePortalTag))
				 {
					  UsePortalTag tagUsePortal = (UsePortalTag)tag;
					  if (tagUsePortal.ActorId > 0)
					  {//Using SNOID..
						  var Objects = Bot.Game.Profile.InteractableObjectCache.Values.Where(obj => obj.SNOID == tagUsePortal.ActorId);
						  foreach (CacheObject item in Objects.OrderBy(obj => obj.Position.Distance(Bot.Character.Position)))
						  {
							  //Found matching object!
							  return item;
						  }

					  }
					  else
					  {//use position to match object
						  Vector3 tagPosition = tagUsePortal.Position;
						  var Objects = Bot.Game.Profile.InteractableObjectCache.Values.Where(obj => obj.Position.Distance(tagPosition) <= 100f);
						  foreach (CacheObject item in Objects.OrderBy(obj => obj.Position.Distance(Bot.Character.Position)))
						  {
							  //Found matching object!
							  return item;
						  }
					  }
				 }
			 }

			 return null;
		 }

		 //Common Used Profile Tags that should be considered Out-Of-Combat Behavior.
		 private static readonly HashSet<Type> oocDBTags = new HashSet<Type> 
																	{ 
																	  typeof(Zeta.CommonBot.Profile.Common.UseWaypointTag), 
																	  typeof(Zeta.CommonBot.Profile.Common.UseObjectTag),
																	  typeof(Zeta.CommonBot.Profile.Common.UseTownPortalTag),
																	  //typeof(Zeta.CommonBot.Profile.Common.WaitTimerTag),
																	  typeof (FunkyBot.XMLTags.TrinityTownPortal),
																	};

		 //Common Used Profile Tags that requires backtracking during combat sessions.
		 private static readonly HashSet<Type> InteractiveTags = new HashSet<Type> 
																	{ 
																	  typeof(Zeta.CommonBot.Profile.Common.UseWaypointTag), 
																	  typeof(Zeta.CommonBot.Profile.Common.UseObjectTag),
																	  //typeof(Zeta.CommonBot.Profile.Common.UseTownPortalTag),
																	  typeof(Zeta.CommonBot.Profile.Common.UsePortalTag),
																	};
	 }
}

