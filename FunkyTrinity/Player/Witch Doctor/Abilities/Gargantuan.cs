﻿using System;

using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.Ability.Abilities.WitchDoctor
{
	public class Gargantuan : ability, IAbility
	{
		public Gargantuan() : base()
		{
		}



		public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.RuneIndexCache[this.Power]:-1; } }

		public override void Initialize()
		{
			ExecutionType = AbilityExecuteFlags.Buff;
			WaitVars = new WaitLoops(2, 1, true);
			Cost = 147;
			Counter = 1;
			UseageType=AbilityUseage.Anywhere;
			Priority = AbilityPriority.High;
			PreCastFlags = (AbilityPreCastFlags.CheckPlayerIncapacitated | AbilityPreCastFlags.CheckCanCast |
			                     AbilityPreCastFlags.CheckEnergy | AbilityPreCastFlags.CheckPetCount);
			IsBuff=true;
			 FcriteriaBuff =
				new Func<bool>(
					() =>
					{
						return Bot.Class.RuneIndexCache[SNOPower.Witchdoctor_Gargantuan] != 0 && Bot.Character.PetData.Gargantuan == 0;
					});
			FcriteriaCombat = new Func<bool>(() =>
			{
				 return (Bot.Class.RuneIndexCache[SNOPower.Witchdoctor_Gargantuan]==0&&
				        (Bot.Combat.iElitesWithinRange[(int) RangeIntervals.Range_15] >= 1 ||
				         (Bot.Target.CurrentUnitTarget.IsEliteRareUnique && Bot.Target.CurrentTarget.RadiusDistance <= 15f))
								||Bot.Class.RuneIndexCache[SNOPower.Witchdoctor_Gargantuan]!=0&&Bot.Character.PetData.Gargantuan==0);
			});
		}

		#region IAbility



		public override int GetHashCode()
		{
			return (int) this.Power;
		}

		public override bool Equals(object obj)
		{
			//Check for null and compare run-time types. 
			if (obj == null || this.GetType() != obj.GetType())
			{
				return false;
			}
			else
			{
				ability p = (ability) obj;
				return this.Power == p.Power;
			}
		}

		#endregion

		public override SNOPower Power
		{
			get { return SNOPower.Witchdoctor_Gargantuan; }
		}
	}
}
