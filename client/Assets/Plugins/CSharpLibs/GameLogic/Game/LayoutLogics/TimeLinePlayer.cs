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
			var orpoint = Line.Points.OrderBy(t => t.Time).ToList();
			foreach (var i in orpoint)
			{
				NoActivedPoints.Enqueue(i);
			}
		}

		private float startTime = -1;

		private readonly Queue<TimePoint> NoActivedPoints = new Queue<TimePoint>();

		public TimeLine Line { private set; get; }
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
				EnableLayout(layout);
			}
			IsFinshed = PlayTime >= Line.Time;
			return IsFinshed;
		}

		protected abstract void EnableLayout(LayoutBase layout);

		public bool IsFinshed { get; private set; } = false;

		protected virtual void OnDestory()
		{

		}

		public void Destory() { this.OnDestory(); }

		public float PlayTime { get; private set; } = 0f;
	}

	public class TimeLinePlayer : TimeLinePlayerBase
	{
		public TimeLinePlayer(TimeLine timeLine, MagicReleaser releaser, EventContainer eventType) : base(timeLine)
		{
			this.Releaser = releaser;
			this.TypeEvent = eventType;
		}
		public EventContainer TypeEvent { private set; get; }
		public MagicReleaser Releaser { private set; get; }

		protected override void EnableLayout(LayoutBase layout)
		{
			if (LayoutBase.IsViewLayout(layout)) return;
			LayoutBaseLogic.EnableLayout(layout, this);
		}
	}

}

