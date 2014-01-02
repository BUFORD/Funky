using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Common;

namespace FunkyBot
{
    public class StuckManager
    {
        private static StuckManager _instance;

        private DateTime _lastCheckedPostition = DateTime.Now;
        private Vector3 _previousPosition = Vector3.Zero;

        private StuckManager() { }

        public static StuckManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StuckManager();
                }

                return _instance;
            }
        }

        public StuckStatus StuckStatus { get; set; }
        
        public StuckStatus IsStuck(Vector3 currentPosition)
        {
            if (DateTime.Now.Subtract(_lastCheckedPostition).TotalMilliseconds >= 3500) //herbfunk: added 500ms
            {
                _lastCheckedPostition = DateTime.Now;
                if (_previousPosition != Vector3.Zero && _previousPosition.Distance(currentPosition) <= 4f)
                {
                    return StuckStatus.Stuck;
                }

                _previousPosition = currentPosition;
            }

            return StuckStatus.NotStuck;
        }
    }

    public enum StuckStatus
    {
        NotStuck,
        Pending,
        Stuck,
        Failure
    }


}
