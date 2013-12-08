﻿using FunkyBot.Movement.Clustering;
using Zeta.Internals.Actors;

namespace FunkyBot.Player.HotBar.Skills.Barb
{
	 public class Overpower : Skill
	 {
		 public override SNOPower Power
		  {
				get { return SNOPower.Barbarian_Overpower; }
		  }

		  public override int RuneIndex { get { return Bot.Character.Class.HotBar.RuneIndexCache.ContainsKey(Power)?Bot.Character.Class.HotBar.RuneIndexCache[Power]:-1; } }

		  public override void Initialize()
		  {
				Cooldown=200;
				ExecutionType=AbilityExecuteFlags.Self;
				WaitVars=new WaitLoops(4, 4, true);
				Cost=0;
				UseageType=AbilityUseage.Anywhere;
				Priority=AbilityPriority.Low;
				PreCastFlags=(AbilityPreCastFlags.CheckRecastTimer|AbilityPreCastFlags.CheckEnergy|
											AbilityPreCastFlags.CheckCanCast|AbilityPreCastFlags.CheckPlayerIncapacitated);
				TargetUnitConditionFlags=new UnitTargetConditions(TargetProperties.None, 10,
					falseConditionalFlags: TargetProperties.Fast);
				ClusterConditions=new ClusterConditions(5d, 7, 2, false);
				FcriteriaCombat=() =>
				{
					// Bot.Targeting.Environment.iAnythingWithinRange[(int)RangeIntervals.Range_6]>=2||(Bot.Character_.Data.dCurrentHealthPct<=0.85&&Bot.Target.CurrentTarget.RadiusDistance<=5f)||
					return true;
				};
		  }

		  #region IAbility
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
	 }
}
