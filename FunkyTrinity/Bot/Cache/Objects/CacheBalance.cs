﻿using System;
using Zeta;
using Zeta.Internals.Actors;

namespace FunkyTrinity
{
    public partial class Funky
    {
		  internal class CacheBalance
		  {
				public int iThisItemLevel { get; set; }
				public ItemType thisItemType { get; set; }
				public ItemBaseType thisItemBaseType { get; set; }
				public bool bThisTwoHand { get; set; }
				public bool bThisOneHand { get; set; }
				public FollowerType thisFollowerType { get; set; }

				public bool bNeedsUpdated { get; set; }
				public CacheBalance(int itemlevel, ItemType itemtype, ItemBaseType itembasetype, bool onehand, bool twohand, FollowerType followertype)
				{
					 iThisItemLevel=itemlevel;
					 thisItemType=itemtype;
					 bThisOneHand=onehand;
					 bThisTwoHand=twohand;
					 thisItemBaseType=itembasetype;
					 thisFollowerType=followertype;
					 bNeedsUpdated=false;
				}

				public CacheBalance(int itemlevel, ItemType itemtype, bool onehand, FollowerType followertype)
				{
					 iThisItemLevel=itemlevel;
					 thisItemType=itemtype;
					 bThisOneHand=onehand;
					 thisFollowerType=followertype;
					 bNeedsUpdated=true;
				}

				public CacheBalance()
				{
					 bNeedsUpdated=true;
				}
		  }
    }
}