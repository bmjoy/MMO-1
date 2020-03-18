using System;
using Layout.LayoutElements;
using GameLogic.Game.Elements;
using EngineCore.Simulater;
using Layout;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Game.LayoutLogics
{
	public class TimeLinePlayer
	{
		public TimeLinePlayer(TimeLine timeLine, MagicReleaser releaser, EventContainer eventType)
		{
			Line = timeLine;
			Releaser = releaser;
			TypeEvent = eventType;
			players = new List<IParticlePlayer>();
			var orpoint = Line.Points.OrderBy(t => t.Time).ToList();
			foreach (var i in orpoint)
			{
				NoActivedPoints.Enqueue(i);
			}
		}

		private float startTime = -1;

		private readonly Queue<TimePoint> NoActivedPoints = new Queue<TimePoint>();

        public TimeLine Line{ private set; get; }

		public MagicReleaser Releaser{ private set; get; }

		public EventContainer TypeEvent{ private set; get; }

		public bool Tick(GTime time)
		{
			if (startTime < 0)
			{
				startTime = time.Time;
				return false;
			}
			PlayTime = time.Time - startTime;

			while (NoActivedPoints.Count > 0 && NoActivedPoints.Peek().Time < PlayTime)
			{
				var point = NoActivedPoints.Dequeue();
				var layout = Line.FindLayoutByGuid(point.GUID);
				LayoutBaseLogic.EnableLayout(layout, this);
			}
			IsFinshed = PlayTime>= Line.Time;
			return IsFinshed;
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

