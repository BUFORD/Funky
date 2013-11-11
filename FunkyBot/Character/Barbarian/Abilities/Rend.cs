﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyBot.AbilityFunky.Abilities.Barb
{
	 public class Rend : Ability, IAbility
	 {
		  public Rend()
				: base()
		  {
		  }

		  public override SNOPower Power
		  {
				get { return SNOPower.Barbarian_Rend; }
		  }

		  public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.RuneIndexCache[this.Power]:-1; } }

		  public override void Initialize()
		  {
				Cooldown=3500;
				ExecutionType=AbilityExecuteFlags.Self;
				WaitVars=new WaitLoops(3, 3, true);
				Cost=20;
				UseageType=AbilityUseage.Combat;
				Priority=AbilityPriority.High;
				PreCastFlags=(AbilityPreCastFlags.CheckRecastTimer|AbilityPreCastFlags.CheckEnergy|
														 AbilityPreCastFlags.CheckCanCast|AbilityPreCastFlags.CheckPlayerIncapacitated);

                ClusterConditions = new ClusterConditions(5d, 8, 2, true, 0.90d);
                TargetUnitConditionFlags = new UnitTargetConditions(TargetProperties.None, 10);

                FcriteriaCombat = new Func<bool>(() =>
                {
                    return true;
                });
		  }

		  #region IAbility
		  public override int GetHashCode()
		  {
				return (int)this.Power;
		  }
		  public override bool Equals(object obj)
		  {
				//Check for null and compare run-time types. 
				if (obj==null||this.GetType()!=obj.GetType())
				{
					 return false;
				}
				else
				{
					 Ability p=(Ability)obj;
					 return this.Power==p.Power;
				}
		  }


		  #endregion
	 }
}
