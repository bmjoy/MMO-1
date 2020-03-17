using System;
using Layout.LayoutElements;
using GameLogic.Game.Elements;
using System.Collections.Generic;
using System.Reflection;
using GameLogic.Game.Perceptions;
using ExcelConfig;
using System.Linq;
using EConfig;
using UVector3 = UnityEngine.Vector3;
using EngineCore.Simulater;

namespace GameLogic.Game.LayoutLogics
{
    /// <summary>
    /// 处理layout
    /// </summary>
	public class HandleLayoutAttribute:Attribute
	{
		public HandleLayoutAttribute(Type handleType)
		{
			HandleType = handleType;
		}
        /// <summary>
        /// layout type
        /// </summary>
		public Type HandleType{set;get;}
	}
    /// <summary>
    /// Layout base logic.
    /// </summary>
    public static class LayoutBaseLogic
	{
        #region  EnableLayout
		static  LayoutBaseLogic ()
		{
			var type = typeof(LayoutBaseLogic);
			var methods=type.GetMethods (BindingFlags.Public | BindingFlags.Static);
			foreach (var i in methods) {
                if (!(i.GetCustomAttributes(typeof(HandleLayoutAttribute), false) is HandleLayoutAttribute[] atts) || atts.Length == 0)
                    continue;
                _handler.Add (atts[0].HandleType, i);
			}
		}

		private static readonly Dictionary<Type,MethodInfo> _handler = new Dictionary<Type, MethodInfo> ();

		public static void EnableLayout(LayoutBase layout, TimeLinePlayer player)
		{
            if (_handler.TryGetValue(layout.GetType(), out MethodInfo m))
            {
                m.Invoke(null, new object[] { player, layout });
            }
            else
            {
                throw new Exception("No Found handle Type :" + layout.GetType());
            }
        }

        #endregion

        #region LookAtTarget
		//LookAtTarget
		[HandleLayout(typeof(LookAtTarget))]
		public static void LookAtTargetActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
            linePlayer.Releaser.ReleaserTarget?.Releaser?.LookAt(linePlayer.Releaser.ReleaserTarget.ReleaserTarget);
		}
        #endregion

        #region MissileLayout
		[HandleLayout(typeof(MissileLayout))]
		public static void MissileActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
			var layout = layoutBase as MissileLayout;
			var per = linePlayer.Releaser.Controllor.Perception as BattlePerception;
			var missile = per.CreateMissile (layout, linePlayer.Releaser);
			linePlayer.Releaser.AttachElement(missile);
		}
		#endregion

		#region MotionLayout
		[HandleLayout(typeof(MotionLayout))]
		public static void MotionActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
			var layout = layoutBase as MotionLayout;
			var releaser = linePlayer.Releaser;
			if (layout.targetType == Layout.TargetType.Releaser)
			{
				releaser.ReleaserTarget?.Releaser?.PlayMotion(layout.motionName);
			}
			else if (layout.targetType == Layout.TargetType.Target)
			{
				releaser.ReleaserTarget?.ReleaserTarget?.PlayMotion(layout.motionName);
			}
		}
        #endregion

        #region DamageLayout
		[HandleLayout(typeof(DamageLayout))]
		public static void DamageActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
			var releaser = linePlayer.Releaser;
			var layout = layoutBase as DamageLayout;

			BattleCharacter orginTarget = null;
			if (layout.target == Layout.TargetType.Releaser) {
				orginTarget = releaser.ReleaserTarget.Releaser;
			} else if (layout.target == Layout.TargetType.Target) 
			{
				//release At Position?
				if(releaser.ReleaserTarget.ReleaserTarget ==null) return ;
				orginTarget = releaser.ReleaserTarget.ReleaserTarget;
			}
			if (orginTarget == null) {
				throw new Exception ("Do not have target of orgin. type:" + layout.target.ToString ());
			}
			var offsetPos = new UVector3 (layout.offsetPosition.x, 
				layout.offsetPosition.y, layout.offsetPosition.z);
			var per = releaser.Controllor.Perception  as BattlePerception;
			var targets = per.FindTarget (orginTarget,
				layout.fiterType, 
				layout.damageType, 
				layout.radius,
				layout.angle, 
				layout.offsetAngle,
                offsetPos,releaser.ReleaserTarget.Releaser.TeamIndex);

			releaser.ShowDamageRange(layout);

			if (string.IsNullOrEmpty (layout.effectKey))
			{
				return;
			}

			//完成一次目标判定
			if (targets.Count > 0) 
			{
				if (layout.effectType == EffectType.EffectGroup) 
				{
					var group = linePlayer.TypeEvent.FindGroupByKey(layout.effectKey);
					if (group == null) return;
					foreach (var t in targets)
					{
						if (!t) continue;
						foreach (var i in group.effects)
						{
							EffectBaseLogic.EffectActive(t, i, releaser);
						}
					}
				}
			}

		}
        #endregion

        #region ParticleLayout
		[HandleLayout(typeof(ParticleLayout))]
		public static void ParticleActive(TimeLinePlayer linePlayer,LayoutBase layoutBase)
		{
			var layout = layoutBase as ParticleLayout;
			var per = linePlayer.Releaser.Controllor.Perception as BattlePerception;
            var particle = per.CreateParticlePlayer (linePlayer.Releaser, layout);
            if (particle == null) return;
			switch (layout.destoryType) 
			{
			case  ParticleDestoryType.LayoutTimeOut:
				linePlayer.AttachParticle (particle);
				break;
			case ParticleDestoryType.Time:
				particle.AutoDestory (layout.destoryTime);
				break;
			case ParticleDestoryType.Normal:
				//自动销亡
				break;
			}
		}
        #endregion

        #region CallUnitLayout
        [HandleLayout(typeof(CallUnitLayout))]
        public static void CallUnitActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
        {
            var unitLayout = layoutBase as CallUnitLayout;
            var releaser = linePlayer.Releaser;
            var charachter = releaser.ReleaserTarget.Releaser;
            var per = releaser.Controllor.Perception as BattlePerception;
            int level = unitLayout.level;

            switch (unitLayout.valueFrom)
            {
                case Proto.GetValueFrom.CurrentConfig:
                    break;
                case Proto.GetValueFrom.MagicLevelParam1:
                    {
                        var param1 = releaser[0];
                        if (string.IsNullOrEmpty(param1)) return;
                        level = Convert.ToInt32(param1);
                    }
                    break;
            }

            //判断是否达到上限
            if (unitLayout.maxNum <= releaser.UnitCount) return;

            var data = ExcelToJSONConfigManager
                .Current.GetConfigByID<CharacterData>(unitLayout.characterID);
           
			var magics = per.CreateHeroMagic(data.ID);
			var unit = per.CreateCharacter(
				level,
				data,
				magics,
                null,
				charachter.TeamIndex,
				charachter.Position + charachter.Rototion * UVector3.forward * charachter.Radius,
				charachter.Rototion.eulerAngles,
				string.Empty, data.Name
			);

			unit.LookAt(releaser.ReleaserTarget.ReleaserTarget);

            releaser.AttachElement(unit, false,unitLayout.time);
            releaser.OnEvent(Layout.EventType.EVENT_UNIT_CREATE);
            per.ChangeCharacterAI(data.AIResourcePath,unit);
			unit.OnDead = (el) => { GObject.Destroy(el, 3); };
        }
		#endregion

		#region LaunchSelfLayout

		[HandleLayout(typeof(LaunchSelfLayout))]
		public static void LaunchSelftActive(TimeLinePlayer linePlayer, LayoutBase layoutBase)
		{
			var launch = layoutBase as LaunchSelfLayout;
			var releaser = linePlayer.Releaser;
			var character = releaser.ReleaserTarget.Releaser;
			var dis = launch.distance;
			if (launch.reachType == TargetReachType.DistanceOfTaget)
			{
				dis = BattlePerception.Distance(character, releaser.ReleaserTarget.ReleaserTarget) + 2;
			}

			character.BeginLauchSelf(character.Rototion,
				dis,
				launch.speed,
				(hit, obj) =>
				{
					if (hit.IsDeath) return;
					if (obj is MagicReleaser r)
					{
						if (hit.TeamIndex == r.ReleaserTarget.Releaser.TeamIndex) return;
						if (r.TryHit(hit)) r.OnEvent(Layout.EventType.EVENT_MISSILE_HIT);
					}
				},
				releaser);
		}
		#endregion

	}
}

