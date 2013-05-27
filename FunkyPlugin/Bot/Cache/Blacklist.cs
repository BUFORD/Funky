﻿using System;
using Zeta;
using System.Collections.Generic;
using Zeta.Internals.SNO;
using Zeta.Common;

namespace FunkyTrinity
{
    public partial class Funky
    {
		  internal static readonly HashSet<ActorType> IgnoredActorTypes=new HashSet<ActorType>
				{
					 ActorType.AxeSymbol,
					 ActorType.ClientEffect,
					 ActorType.Critter,
					 ActorType.CustomBrain,
					 ActorType.Invalid
				};

		  ///<summary>
		  ///Determines the type of blacklist an object should recieve. Permanent is entire game, Temporary is 60 seconds long.
		  ///</summary>
		  public enum BlacklistType
		  {
				None,
				Temporary,
				Permanent
		  }

		  // When did we last clear the temporary blacklist?
		  private static DateTime dateSinceTemporaryBlacklistClear=DateTime.Today;
		  // And the full blacklist?
		  // These are blacklists used to blacklist objects/monsters/items either for the rest of the current game, or for a shorter period
		  private static HashSet<int> hashRGUIDTemporaryIgnoreBlacklist=new HashSet<int>();
		  private static HashSet<int> hashRGUIDIgnoreBlacklist=new HashSet<int>();

		

		  // IGNORE LIST / BLACKLIST - for units / monsters / npcs
		  // Special blacklist for things like ravens, templar/scoundrel/enchantress in town, witch-doctor summons, tornado-animations etc. etc. that should never be attacked
		  // Note: This is a MONSTER blacklist - so only stuff that needs to be ignored by the combat-engine. An "object" blacklist is further down!
		  public static readonly HashSet<int> hashActorSNOIgnoreBlacklist=new HashSet<int> { 
            5840, 111456, 5013, 5014, 205756, 205746, 4182, 4183, 4644, 4062, 4538, 52693, 162575, 2928, 51291, 51292, 
            96132, 90958, 90959, 80980, 51292, 51291, 2928, 3546,164195, 129345, 81857, 138428, 81857, 60583, 170038, 174854, 190390, 
            194263, 87189, 90072, 107031, 106584, 186130, 187265, 201426, 201242, 200969, 201423, 
            201438, 201464, 201454, 108012, 103279, 89578, 74004, 84531, 84538, 89579, 190492, 209133, 6318, 107705, 105681, 89934, 
            89933, 182276, 117574, 182271, 182283, 182278, 128895, 81980, 82111, 81226, 81227, 107067, 106749,
            107107, 107112, 106731, 107752, 107829, 90321, 107828, 121327, 249320, 81232, 81231, 81239, 81515, 210433, 195414,
            80758, 80757, 80745, 81229, 81230, 82109, 83024, 83025, 82972, 83959, 249190, 251396, 138472, 118260, 200226, 192654, 245828, 
            215103, 132951, 217508, 199998, 199997, 114527, 245910, 169123, 123885, 169890, 168878, 169891, 169077, 169904, 169907, 
            169906, 169908, 169905, 169909, 179780, 179778, 179772, 179779, 179776, 122305, 110959, 103235, 103215, 105763, 103217, 51353, 80140, 
            4176, 178664, 173827, 133741, 159144, 181748, 159098, 206569, 200706, 5895, 5896, 5897, 5899, 4686, 87037, 85843, 103919, 249338, 
            251416, 249192, 80812, 196899,196900,196903,223333,220636,218951,206559,208543,166133,114304,212231,
            2976, 181563,181857,181858,5215,175482,3901,152126,80447,425,3609,58568, 210087, 164057,220160, 87534, 144405, 181176, 181177,62522,220114,
				113983,
        };

		  // IGNORE LIST / BLACKLIST - for world objects
		  // World objects that should always be ignored - eg certain destructables, certain containers, etc. - anything handled as a "world object" rather than a monster
		  private static readonly HashSet<int> hashSNOIgnoreBlacklist=new HashSet<int> { 
            163449, 78030, 2909, 58283, 58299, 58309, 58321, 87809, 88005, 90150, 91600, 97023, 97350, 97381, 72689, 121327, 54952, 54515, 3340, 122076, 123640, 
            60665, 60844, 78554, 86400, 86428, 81699, 86266, 86400, 110769, 211456, 6190, 80002, 104596, 58836, 104827, 74909, 6155, 6156, 6158, 6159, 75132,
            181504, 91688, 3007, 3011, 3014, 130858, 131573, 214396, 182730, 226087, 141639, 206569, 15119, 54413, 54926, 2979, 56416, 53802, 5776, 3949, 
            108490, 52833, 3341, 4482, 188129, 188127, 55259, 54693, 3689, 131494, 3609, 225589, 171635, 3948,5739, 185949, 182697,
         };

		  internal static void AddObjectToBlacklist(int RAGUID, BlacklistType blacklist)
		  {
				if (blacklist==BlacklistType.Permanent)
				{
					 if (!hashRGUIDIgnoreBlacklist.Contains(RAGUID))
						  hashRGUIDIgnoreBlacklist.Add(RAGUID);

				}
				else if (blacklist==BlacklistType.Temporary)
				{
					 if (!hashRGUIDTemporaryIgnoreBlacklist.Contains(RAGUID))
					 {
						  hashRGUIDTemporaryIgnoreBlacklist.Add(RAGUID);
						  dateSinceTemporaryBlacklistClear=DateTime.Now;
					 }
				}
		  }

		  //Updates RActorGUID and returns if we should continue Cache Process by checking blacklists.
		  internal static bool IsObjectBlacklisted(int ractorGUID)
		  {
				// See if it's on our temporary blacklist (from being stuck targeting it), as long as it's distance is not extremely close
				if (hashRGUIDTemporaryIgnoreBlacklist.Contains(ractorGUID))
				{
					 return false;
				}
				// Or on our more permanent "per-game" blacklist
				if (hashRGUIDIgnoreBlacklist.Contains(ractorGUID))
				{
					 return false;
				}

				return true;
		  }

		  internal static void IgnoreThisObject(CachedSNOEntry snoObj, int RAGUID, bool removal=true, bool blacklistSNOID=true)
		  {
				Logging.WriteVerbose("[Blacklist] -- RAGUID {0} SNOID {1} ({2})", snoObj.SNOID, RAGUID, snoObj.InternalName);

				int sno, raguid;
				sno=snoObj.SNOID;
				raguid=RAGUID;

				//Add to our blacklist so we don't create it again..
				hashRGUIDIgnoreBlacklist.Add(raguid);

				if (blacklistSNOID)
					 //Blacklist SNO so we don't create it ever again!
					 hashActorSNOIgnoreBlacklist.Add(sno);

				if (removal)
				{
					 //Clear SNO cache entries..
					 ObjectCache.cacheSnoCollection.Remove(snoObj.SNOID);
					 //Clear previous cache entries..
					 ObjectCache.Objects.Remove(RAGUID);
				}


		  }
		  internal static void IgnoreThisObject(CacheObject thisobj, bool removal=true, bool blacklistSNOID=true)
		  {
				Logging.WriteVerbose("[Blacklist] Obj RAGUID {0} SNOID {1} ({2})", thisobj.RAGUID, thisobj.SNOID, thisobj.InternalName);

				int sno, raguid;
				sno=thisobj.SNOID;
				raguid=thisobj.RAGUID;

				//Add to our blacklist so we don't create it again..
				hashRGUIDIgnoreBlacklist.Add(raguid);

				if (blacklistSNOID)
					 //Blacklist SNO so we don't create it ever again!
					 hashActorSNOIgnoreBlacklist.Add(sno);

				if (removal)
				{
					 //Clear SNO cache entries..
					 ObjectCache.cacheSnoCollection.Remove(sno);
					 //Clear previous cache entries..
					 ObjectCache.Objects.Remove(raguid);


				}
		  }
    }
}