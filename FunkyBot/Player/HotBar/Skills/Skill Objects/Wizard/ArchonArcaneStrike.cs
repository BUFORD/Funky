﻿using FunkyBot.Player.HotBar.Skills.Conditions;
using Zeta.Game.Internals.Actors;

namespace FunkyBot.Player.HotBar.Skills.Wizard
{
	 public class ArchonArcaneStrike : Skill
	 {
		 public override void Initialize()
		  {
				Cooldown=200;
				ExecutionType=AbilityExecuteFlags.Target|AbilityExecuteFlags.ClusterTargetNearest;
				WaitVars=new WaitLoops(1, 1, true);
				Range=15;
				UseageType=AbilityUseage.Combat;
				Priority=AbilityPriority.Low;
				PreCast=new SkillPreCast((AbilityPreCastFlags.CheckPlayerIncapacitated));
				ClusterConditions=new SkillClusterConditions(6d, 10f, 2, true);
				SingleUnitCondition=new UnitTargetConditions(TargetProperties.None, 8);
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
			  Skill p=(Skill)obj;
			  return Power==p.Power;
		  }

		  #endregion

		  public override SNOPower Power
		  {
				get { return SNOPower.Wizard_Archon_ArcaneStrike; }
		  }
	 }
}
