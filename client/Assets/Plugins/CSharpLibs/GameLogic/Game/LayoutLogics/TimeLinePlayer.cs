using System;
using Layout.LayoutElements;
using GameLogic.Game.Elements;
using EngineCore.Simulater;
using Layout;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Game.LayoutLogics
{
	public abstract class TimeLinePlayerBase
	{
		public TimeLinePlayerBase(TimeLine timeLine)
		{
			Line = timeLine;
		}
		private float startTime = -1;
		private readonly Queue<TimePoint> NoActivedPoints = new Queue<TimePoint>();
		public TimeLine Line { private set; get; }
		public bool Tick(GTime time)
		{
			if (startTime < 0)
			{
				startTime = time.Time;
				var orpoint = Line.Points.OrderBy(t => t.Time).ToList();
				NoActivedPoints.Clear();
				foreach (var i in orpoint)
				{
					if (i.Time < ToTime) continue;
					NoActivedPoints.Enqueue(i);
				}
				return false;
			}
			PlayTime = time.Time - startTime;

			while (NoActivedPoints.Count > 0
				&& NoActivedPoints.Peek().Time < PlayTime)
			{
				var point = NoActivedPoints.Dequeue();
				var layout = Line.FindLayoutByGuid(point.GUID);
				EnableLayout(layout);
			}
			IsFinshed = PlayTime >= Line.Time;
			return IsFinshed;
		}

		protected abstract void EnableLayout(LayoutBase layout);

		public bool IsFinshed { get; private set; } = false;

		protected virtual void OnDestory() { }

		public void Destory() { this.OnDestory(); }

		public float PlayTime { get; private set; } = 0f;
		private float ToTime = -1;

		private int currentRepeatTime = 0;


		public void Repeat(int maxTimes,float toTime)
		{
			if (currentRepeatTime >= maxTimes) return;
			startTime = -1;
			currentRepeatTime++;
			ToTime = toTime;
		}
	}

	public class TimeLinePlayer : TimeLinePlayerBase
	{
		public TimeLinePlayer(TimeLine timeLine,
            MagicReleaser releaser,
            EventContainer eventType,
            BattleCharacter eventTarget) : base(timeLine)
		{
			this.Releaser = releaser;
			this.TypeEvent = eventType;
			this.EventTarget = eventTarget;
		}
		public EventContainer TypeEvent { private set; get; }
		public MagicReleaser Releaser { private set; get; }
        public BattleCharacter EventTarget { set; get; }

		protected override void EnableLayout(LayoutBase layout)
		{
			if (LayoutBase.IsLogicLayout(layout))
				LayoutBaseLogic.EnableLayout(layout, this);
		}
	}

}

