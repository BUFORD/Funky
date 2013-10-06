﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.AbilityFunky.Abilities.Barb
{
	 public class Frenzy : Ability, IAbility
	 {
		  public Frenzy()
				: base()
		  {
		  }

		  public override SNOPower Power
		  {
				get { return SNOPower.Barbarian_Frenzy; }
		  }

		  public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.RuneIndexCache[this.Power]:-1; } }

		  public override void Initialize()
		  {
				Cooldown=5;
				ExecutionType=AbilityExecuteFlags.Target;
				WaitVars=new WaitLoops(0, 0, true);
				Cost=0;
				Range=10;
				UseageType=AbilityUseage.Combat;
				Priority=AbilityPriority.None;
				PreCastFlags=(AbilityPreCastFlags.CheckRecastTimer|AbilityPreCastFlags.CheckCanCast|
											AbilityPreCastFlags.CheckPlayerIncapacitated);
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
