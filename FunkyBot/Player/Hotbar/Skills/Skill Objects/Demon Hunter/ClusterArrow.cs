﻿using FunkyBot.Movement.Clustering;
using System;
using FunkyBot.Player.HotBar.Skills.Conditions;
using Zeta.Internals.Actors;

namespace FunkyBot.Player.HotBar.Skills.DemonHunter
{
	 public class ClusterArrow : Skill
	 {
		 public override void Initialize()
		  {
				Cooldown=150;
				ExecutionType=AbilityExecuteFlags.Location|AbilityExecuteFlags.ClusterLocation;
				WaitVars=new WaitLoops(1, 1, true);
				Cost=50;
				Range=50;
				IsRanged=true;
				IsProjectile=true;
				UseageType=AbilityUseage.Combat;
				Priority=AbilityPriority.Low;
				PreCast=new SkillPreCast((AbilityPreCastFlags.CheckPlayerIncapacitated|AbilityPreCastFlags.CheckEnergy|
				                          AbilityPreCastFlags.CheckRecastTimer));

				UnitsWithinRangeConditions=new Tuple<RangeIntervals, int>(RangeIntervals.Range_50, 3);
				ElitesWithinRangeConditions=new Tuple<RangeIntervals, int>(RangeIntervals.Range_50, 1);
				//SingleUnitCondition=new UnitTargetConditions(TargetProperties.IsSpecial,69),
				ClusterConditions=new ClusterConditions(4d, 45, 2, true);
		  }

		  #region IAbility

		  public override int RuneIndex
		  {
				get { return Bot.Character.Class.HotBar.RuneIndexCache.ContainsKey(Power)?Bot.Character.Class.HotBar.RuneIndexCache[Power]:-1; }
		  }

		  public override int GetHashCode()
		  {
				return (int)Power;
		  }

		  public override bool Equals(object obj)
		  {
				//Check for null and compare run-time types. 
				if (obj==null||GetType()!=obj.GetType())
				{
					 return false;
				}
				else
				{
					 Skill p=(Skill)obj;
					 return Power==p.Power;
				}
		  }

		  #endregion

		  public override SNOPower Power
		  {
				get { return SNOPower.DemonHunter_ClusterArrow; }
		  }
	 }
}
