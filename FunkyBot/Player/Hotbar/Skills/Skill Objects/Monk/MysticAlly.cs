﻿using Zeta.Internals.Actors;

namespace FunkyBot.Player.HotBar.Skills.Monk
{
	 public class MysticAlly : Skill
	 {
		 public override void Initialize()
		  {
				Cooldown=30000;
				ExecutionType=AbilityExecuteFlags.Buff;
				WaitVars=new WaitLoops(2, 2, true);
				Cost=25;
				UseageType=AbilityUseage.Anywhere;
				IsBuff=true;
				Priority=AbilityPriority.High;
				IsSpecialAbility=true;
				Counter=1;
				PreCastFlags=(AbilityPreCastFlags.CheckEnergy|AbilityPreCastFlags.CheckCanCast|AbilityPreCastFlags.CheckPetCount);
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
				get { return SNOPower.Monk_MysticAlly; }
		  }
	 }
}
