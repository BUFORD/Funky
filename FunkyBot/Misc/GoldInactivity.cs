using System;
using System.Windows.Threading;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;

namespace FunkyBot
{
    public class GoldInactivity
    {
        public const int LIMIT = 120;

        private static GoldInactivity _instance;
        private DispatcherTimer _timer;
        private int _previousAmount = 0;
        private DateTime _lastUpdate = DateTime.Now;

        public event Action OnGoldInactivity;

        public static GoldInactivity Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GoldInactivity();
                }

                return _instance;
            }
        }

        private GoldInactivity()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 10);
            _timer.Tick += _timer_Tick;
            GameEvents.OnGameLeft += GameEvents_OnGameLeft;
            GameEvents.OnGameJoined += GameEvents_OnGameJoined;
            BotMain.OnStop += BotMain_OnStop;
            BotMain.OnStart += BotMain_OnStart;
        }

        void HandleTimer()
        {
            if (!ZetaDia.IsInGame)
            {
                Stop();
            }
            else
            {
                Start();
            }
        }

        private void Stop()
        {
            Logging.Write("[GoldInactivity] Stop");
            _timer.Stop();
        }

        private void Start()
        {
            Logging.Write("[GoldInactivity] Start");
            _previousAmount = Bot.Character.Data.Coinage;
            _lastUpdate = DateTime.Now;
            _timer.Start();
        }

        public void CheckInactivity()
        {
            var currentAmount = Bot.Character.Data.Coinage;
            if (currentAmount != _previousAmount)
            {
                _previousAmount = currentAmount;
                _lastUpdate = DateTime.Now;
                return;
            }

            var elapsedTime = DateTime.Now.Subtract(_lastUpdate).Seconds;
            if (elapsedTime >= LIMIT)
            {
                if (OnGoldInactivity != null)
                {
                    Logging.Write("[GoldInactivity] GoldInactivity limit reached");
                    Stop();
                    OnGoldInactivity();
                }
            }
        }

        #region Event Handlers

        void _timer_Tick(object sender, EventArgs e)
        {
            CheckInactivity();
        }

        void BotMain_OnStart(IBot bot)
        {
            HandleTimer();
        }

        void BotMain_OnStop(IBot bot)
        {
            Stop();
        }

        void GameEvents_OnGameJoined(object sender, EventArgs e)
        {
            Start();
        }

        void GameEvents_OnGameLeft(object sender, EventArgs e)
        {
            Stop();
        }

        #endregion
    }
}
