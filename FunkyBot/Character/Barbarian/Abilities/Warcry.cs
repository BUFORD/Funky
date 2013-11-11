﻿using System;
using FunkyBot.Cache;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyBot.AbilityFunky.Abilities.Barb
{
	 public class Warcry : Ability, IAbility
	 {
		  public Warcry()
				: base()
		  {
		  }

		  public override SNOPower Power
		  {
				get { return SNOPower.Barbarian_WarCry; }
		  }

		  public override int RuneIndex { get { return Bot.Class.HotBar.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.HotBar.RuneIndexCache[this.Power]:-1; } }

		  public override void Initialize()
		  {
				Cooldown=20500;
				ExecutionType=AbilityExecuteFlags.Buff;
				WaitVars=new WaitLoops(1, 1, true);
				Cost=0;
				Range=0;
				IsBuff=true;
				UseageType=AbilityUseage.Anywhere;
				Priority=AbilityPriority.Highest;
				PreCastFlags=(AbilityPreCastFlags.CheckCanCast|AbilityPreCastFlags.CheckPlayerIncapacitated);
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