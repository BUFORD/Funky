﻿using System;
using System.Collections.Generic;
using System.Linq;
using FunkyTrinity.ability;
using FunkyTrinity.Cache;

namespace FunkyTrinity.Movement.Clustering
{
    internal class ClusterCollection
    {
        public virtual ClusterConditions clusterConditions { get; set; }
        public List<UnitCluster> CurrentClusters { get; set; }
        internal DateTime lastClusterComputed { get; set; }
        internal DateTime lastClusterRefresh { get; set; }

        private int refreshRate;
        ///<summary>
        ///The rate at which refreshing the clusters can occur. (Exisiting Clusters)
        ///</summary>
        public int RefreshRate
        {
            get { return refreshRate; }
            set { refreshRate = value; }
        }
        private int updateRate;
        ///<summary>
        ///The rate at which the clusters are recomputed. (New Clusters)
        ///</summary>
        public int UpdateRate
        {
            get { return updateRate; }
            set { updateRate = value; }
        }



        public bool ShouldUpdateClusters
        {
            get
            {
                return DateTime.Now.Subtract(lastClusterComputed).TotalMilliseconds > this.UpdateRate;
            }
        }

        ///<summary>
        ///Iterates thru the cluster list and calls the update method on each.
        ///</summary>
        public virtual void RefreshClusters()
        {
            if (DateTime.Now.Subtract(lastClusterRefresh).TotalMilliseconds < this.RefreshRate)
                return;


            if (CurrentClusters.Count > 0)
            {
                foreach (var item in CurrentClusters)
                {
                    item.UpdateUnitPointLists(clusterConditions);
                }

                CurrentClusters = CurrentClusters.Where(c => c.ListUnits.Count >= clusterConditions.MinimumUnits && (clusterConditions.DOTDPSRatio == 0.00d || c.Info.DotDPSRatio <= clusterConditions.DOTDPSRatio)).ToList();
            }

            lastClusterRefresh = DateTime.Now;
        }

        public IEnumerable<CacheUnit> RetrieveAllUnits()
        {
            return CurrentClusters.SelectMany(o => o.ListUnits);
        }

        ///<summary>
        ///Updates the Cluster List!
        ///</summary>
        public virtual void UpdateClusters()
        {
            CurrentClusters.Clear();

            //Get unit objects only!
            List<CacheUnit> listObjectUnits = Bot.ValidObjects.OfType<CacheUnit>().Where(u =>
                 Bot.Combat.UnitRAGUIDs.Contains(u.RAGUID)
                 && u.CentreDistance <= clusterConditions.MaximumDistance
                 && u.CentreDistance >= clusterConditions.MinimumDistance
                 && (!clusterConditions.IgnoreNonTargetable || u.IsTargetable.HasValue && u.IsTargetable.Value)).ToList();


            //Logging.WriteVerbose("Total Units {0}", listObjectUnits.Count.ToString());
            if (listObjectUnits.Count > 0)
            {
                CurrentClusters = cluster.RunKmeans(listObjectUnits, clusterConditions.ClusterDistance).Where(c => c.Info.Properties.HasFlag(clusterConditions.ClusterFlags) && c.ListUnits.Count >= clusterConditions.MinimumUnits && (clusterConditions.DOTDPSRatio == 0.00d || c.Info.DotDPSRatio <= clusterConditions.DOTDPSRatio)).ToList();

                //Sort by distance -- reverse to get nearest unit First
                if (CurrentClusters.Count > 0)
                {
                    CurrentClusters = CurrentClusters.OrderBy(o => o.NearestMonsterDistance).ToList();
                    CurrentClusters.First().ListUnits.Sort();
                    CurrentClusters.First().ListUnits.Reverse();
                }
            }

            lastClusterComputed = DateTime.Now;
        }

        public ClusterCollection(ClusterConditions CC, int updaterate = 200, int refreshrate = 100)
        {
            CurrentClusters = new List<UnitCluster>();
            lastClusterComputed = DateTime.Today;
            lastClusterRefresh = DateTime.Today;
            clusterConditions = CC;
            RefreshRate = refreshrate;
            UpdateRate = updaterate;
        }
    }

    internal class ClusterTargetCollection : ClusterCollection
    {
        public ClusterTargetCollection(ClusterConditions CC) : base(CC) { }


        public override ClusterConditions clusterConditions
        {
            get
            {
                return new ClusterConditions(Bot.SettingsFunky.ClusterDistance, 125f, Bot.SettingsFunky.ClusterMinimumUnitCount, false);
            }
            set
            {
                base.clusterConditions = value;
            }
        }

        public override void RefreshClusters()
        {
            if (DateTime.Now.Subtract(base.lastClusterRefresh).TotalMilliseconds < base.RefreshRate)
                return;


            if (CurrentClusters.Count > 0)
            {
                foreach (var item in base.CurrentClusters)
                {
                    item.UpdateUnitPointLists(this.clusterConditions);
                }

                base.CurrentClusters = base.CurrentClusters
                    .Where(c => c.ListUnits.Count >= this.clusterConditions.MinimumUnits
                        && (this.clusterConditions.DOTDPSRatio == 0.00d
                        || c.Info.DotDPSRatio <= this.clusterConditions.DOTDPSRatio)).ToList();
            }

            base.lastClusterRefresh = DateTime.Now;
        }
        public override void UpdateClusters()
        {
            CurrentClusters.Clear();

            //TODO: listObjectUnits gets enumerated an extra time
            //Get unit objects only!
            List<CacheUnit> listObjectUnits = Bot.ValidObjects.OfType<CacheUnit>().Where(u =>
                 Bot.Combat.UnitRAGUIDs.Contains(u.RAGUID)
                 && u.CentreDistance <= this.clusterConditions.MaximumDistance
                 && (!this.clusterConditions.IgnoreNonTargetable || u.IsTargetable.HasValue && u.IsTargetable.Value)).ToList();


            //Logging.WriteVerbose("Total Units {0}", listObjectUnits.Count.ToString());
            if (listObjectUnits.Count > 0)
            {
                var currentClusters = cluster.RunKmeans(listObjectUnits, this.clusterConditions.ClusterDistance)
                     .Where(c => c.ListUnits.Count >= this.clusterConditions.MinimumUnits
                         && (this.clusterConditions.DOTDPSRatio == 0.00d
                         || c.Info.DotDPSRatio <= this.clusterConditions.DOTDPSRatio));

                //Sort by distance -- reverse to get nearest unit First
                if (currentClusters.Any())
                {
                    var sortedClusters = currentClusters.OrderBy(o => o.NearestMonsterDistance);
                    CurrentClusters.AddRange(currentClusters);
                    CurrentClusters.First().ListUnits.Sort();
                    CurrentClusters.First().ListUnits.Reverse();
                }
            }

            lastClusterComputed = DateTime.Now;
        }
    }
}