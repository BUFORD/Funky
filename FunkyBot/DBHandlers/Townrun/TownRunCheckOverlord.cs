﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using System.Collections.Generic;

namespace FunkyBot.DBHandlers
{

	internal static partial class TownRunManager
	{
		private static bool PotionCheck = false;

		private static bool bPreStashPauseDone;
		private static double iPreStashLoops;

		private static int[] LastStashPoint = { -1, -1 };
		private static int LastStashPage = -1;

		private static bool FoundTownPortal;
		private static DiaGizmo TownPortalObj;
		private static Vector3 TownportalMovementVector3 = Vector3.Zero;

		// The distance last loop, so we can compare to current distance to work out if we moved
		private static float iLastDistance;
		// This dictionary stores attempted stash counts on items, to help detect any stash stucks on the same item etc.
		internal static Dictionary<int, int> _dictItemStashAttempted = new Dictionary<int, int>();
		// Random variables used during item handling and town-runs
		private static int iItemDelayLoopLimit;
		private static int iCurrentItemLoops;
		internal static bool bLoggedAnythingThisStash = false;
		private static bool bUpdatedStashMap;
		private static bool bCheckUnidItems;
		private static bool bCheckedItemDurability;

		internal static bool bLoggedJunkThisStash = false;
		internal static string sValueItemStatString = "";
		internal static string sJunkItemStatString = "";
		// Whether to try forcing a vendor-run for custom reasons
		internal static bool bWantToTownRun = false;
		internal static bool bFailedToLootLastItem = false;
		private static bool bLastTownRunCheckResult;
		private static bool bReachedSafety;
		// DateTime check to prevent inventory-check spam when looking for repairs being needed
		private static DateTime TimeLastCheckedForTownRun = DateTime.Today;
		private static bool bCurrentlyMoving;
		private static bool bNeedsEquipmentRepairs;
		// Stash mapper - it's an array representing every slot in your stash, true or false dictating if the slot is free or not
		private static bool[,] GilesStashSlotBlocked = new bool[7, 30];
		internal static bool TownrunStartedInTown = true;
        public static bool TownRunFromInactivity = false;

		// **********************************************************************************************
		// *****         TownRunCheckOverlord - determine if we should do a town-run or not         *****
		// **********************************************************************************************
		internal static bool GilesTownRunCheckOverlord(object ret)
		{
			bWantToTownRun = false;

			// Check if we should be forcing a town-run
			if (Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
			{
				bWantToTownRun = true;
			}
			else
			{
				int recheckDelay = Bot.Character.Data.bIsInTown ? 1 : 6;
				if (DateTime.Now.Subtract(TimeLastCheckedForTownRun).TotalSeconds > recheckDelay)
				{
					TimeLastCheckedForTownRun = DateTime.Now;
					if (Zeta.CommonBot.Logic.BrainBehavior.ShouldVendor || Bot.Character.Data.BackPack.ShouldRepairItems())
					{
						bCheckedItemDurability = false;
						bCheckUnidItems = true;
						bWantToTownRun = true;
					}
				}
			}

			bLastTownRunCheckResult = bWantToTownRun;

			//Precheck prior to casting TP..
			if (bLastTownRunCheckResult)
			{
				if (!ZetaDia.Me.IsInTown)
				{
					bPreStashPauseDone = false;
					Bot.NavigationCache.LOSmovementObject = null;
					Bot.NavigationCache.LOSVector = Vector3.Zero;
					return TownPortalBehavior.SafetyCheckForTownRun();
				}
			}

			return bWantToTownRun;
		}

	}

}