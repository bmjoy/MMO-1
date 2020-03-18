using System;
using Layout.LayoutElements;
using GameLogic.Game.Elements;
using EngineCore.Simulater;
using Layout;
using System.Collections.Generic;

namespace GameLogic.Game.LayoutLogics
{
	public class TimeLinePlayer
	{
		public TimeLinePlayer (TimeLine timeLine, MagicReleaser releaser, EventContainer eventType)
		{
			Line = timeLine;
			Releaser = releaser;
			TypeEvent = eventType;
			players = new List<IParticlePlayer> ();
		}

		private float lastTime = -1;
		private float startTime = 0;

        public TimeLine Line{ private set; get; }

		public MagicReleaser Releaser{ private set; get; }

		public EventContainer TypeEvent{ private set; get; }

		public bool Tick(GTime time)
		{
			if (lastTime < 0) 
			{
				startTime = time.Time;
				lastTime = time.Time - 0.01f;
				return false;
			}
			var old = lastTime - startTime;
			var now = time.Time- startTime;

			for(var i  = 0;i<Line.Points.Count;i++)
			{
				var point = Line.Points [i];
				if (point.Time > old && point.Time <= now)
                {
					var layout = Line.FindLayoutByGuid(point.GUID);
					LayoutBaseLogic.EnableLayout (layout, this);
				}
			}
			lastTime = time.Time;
			var result=  now > Line.Time ;
			IsFinshed = result;
			PlayTime = now;
			return result;
		}

        public bool IsFinshed { get; private set; } = false;

        private readonly List<IParticlePlayer> players;

		public void AttachParticle(IParticlePlayer particle)
		{
			players.Add (particle);
		}

		public void Destory()
		{
			foreach (var i in players) 
            {
				if (i.CanDestory) {
					i.DestoryParticle ();
				}
			}
		}

        public float PlayTime { get; private set; } = 0f;
    }
}

