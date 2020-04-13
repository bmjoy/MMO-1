using UnityEngine;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;
using Layout.LayoutElements;
using GameLogic;
using UVector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using EngineCore.Simulater;
using Layout;

public class UMagicReleaserView : UElementView, IMagicReleaser
{
    public void SetData(int releaser, int target, UVector3 targetpos, ReleaserModeType rmType,string magicKey)
    {
        CharacterTarget = PerView.GetViewByIndex<UCharacterView>(target);
        CharacterReleaser = PerView.GetViewByIndex<UCharacterView>(releaser);
        RIndex = releaser;
        TIndex = target;
        TargetPos = targetpos;
        RMType = rmType;
        if (PerView is IBattlePerception per)
        {
            Magic = per?.GetMagicByKey(magicKey);
        }
        MagicKey = magicKey;
    }


    private MagicData Magic;
    public UVector3 TargetPos;
    private int RIndex;
    private int TIndex;

    public UCharacterView CharacterTarget { private set; get; }
    public UCharacterView CharacterReleaser { private set; get; }

    public ReleaserModeType RMType { private set; get; }

    public string MagicKey { get; private set; }

    private readonly LinkedList<TimeLineViewPlayer> _players = new LinkedList<TimeLineViewPlayer>();

    void IMagicReleaser.PlayTimeLine(int pIndex, int PathIndex, int targetIndex, int type)
    {
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_PlayTimeLine
        {
            PathIndex = PathIndex,
            Index = Index,
            TargetIndex = targetIndex,
            Type = type,
            PlayIndex = pIndex
        });
#endif
#if !UNITY_SERVER

        if (Magic == null) { Debug.LogError($"Nofoun magic key {MagicKey}"); return; }

        var eType = (Layout.EventType)type;
        var tar = PerView.GetViewByIndex<UCharacterView>(targetIndex);
        if (PerView is IBattlePerception per)
        {
            if (PathIndex < 0 || Magic.Containers.Count <= PathIndex)
            {
                Debug.LogError($"Index out of bounds {PathIndex} magic containers {Magic?.Containers?.Count}");
                return;
            }
            var e = Magic.Containers[PathIndex];
            PlayLine(pIndex, per?.GetTimeLineByPath(e.layoutPath), tar, eType);
        }
#endif
    }

    void IMagicReleaser.CancelTimeLine(int pIndex)
    {
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_CancelTimeLine
        {
            Index = Index,
            PlayIndex = pIndex
        }); ;
#endif
#if !UNITY_SERVER
        foreach (var i in _players)
        {
            if (i.Index == pIndex)
            {
                _players.Remove(i);
                i.Destory();
                break;
            }
        }     
#endif
    }

    private TimeLineViewPlayer PlayLine(int pIndex,TimeLine timeLine, UCharacterView eventTarget, Layout.EventType type)
    {
        if (timeLine == null) return null;
        var player = new TimeLineViewPlayer(pIndex,timeLine, this, eventTarget, type);
        _players.AddLast(player);
        return player;
    }

    void IMagicReleaser.PlayTest(int pIndex,TimeLine line)
    {
        PlayLine(pIndex, line, this.CharacterTarget, Layout.EventType.EVENT_START);
    }

    private void TickTimeLine(GTime time)
    {
        var current = _players.First;
        while (current != null)
        {
            if (current.Value.Tick(time))
            {
                current.Value.Destory();
                _players.Remove(current);
            }
            current = current.Next;
        }
    }


    private readonly List<IParticlePlayer> pPlayers  = new List<IParticlePlayer>();

    internal void AttachParticle(IParticlePlayer particle)
    {
        pPlayers.Add(particle);
    }

    public override IMessage ToInitNotify()
    {
        var createNotify = new Notify_CreateReleaser
        {
            Index = Index,
            ReleaserIndex = RIndex,
            TargetIndex = TIndex,
            MagicKey = MagicKey,
            Position = TargetPos.ToPV3(),
            RMType = RMType
        };
        return createNotify;
    }

    private void OnDestroy()
    {
        foreach (var i in pPlayers) i.DestoryParticle();
        pPlayers.Clear();
        foreach (var i in _players) i.Destory();
        _players.Clear();
    }

    private void Update()
    {
        TickTimeLine(PerView.GetTime());
    }


    void IMagicReleaser.ShowDamageRanger(DamageLayout layout, UVector3 tar, Quaternion rototion)
    {
#if UNITY_EDITOR
        if (layout.RangeType.damageType == Layout.LayoutElements.DamageType.Rangle)
        {
            var pos = tar + rototion * layout.RangeType.offsetPosition.ToUV3();
            DamageRangeDebuger.TryGet(this.gameObject).AddDebug(layout, pos,rototion);
        }
#endif
    }

  
}
