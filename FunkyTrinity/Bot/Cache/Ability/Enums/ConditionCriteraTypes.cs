﻿using System;

namespace FunkyTrinity.ability
{
	 ///<summary>
	 ///
	 ///</summary>
	 [Flags]
	 public enum ConditionCriteraTypes
	 {
			None=0,
			Cluster=1,
			UnitsInRange=2,
			ElitesInRange=4,
			SingleTarget=8,
	 }
}