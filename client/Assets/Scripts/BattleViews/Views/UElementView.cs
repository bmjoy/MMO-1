using UnityEngine;
using System.Collections;
using GameLogic.Game.Elements;
using EngineCore.Simulater;
using Google.Protobuf;
using Proto;

public abstract class UElementView : MonoBehaviour, IBattleElement, ISerializerableElement
{

    public int Index { set; get; }
    public UPerceptionView PerView { private set; get; }

    public GObject GElement { private set; get; }

    public void SetPrecpetion(UPerceptionView view)
    {
        PerView = view;
    }

    #region IBattleElement implementation

    void IBattleElement.JoinState(int index)
    {
        OnJoined();
        this.Index = index;
        CreateNotify(this.ToInitNotify());
        PerView.AttachView(this);
    }

    void IBattleElement.ExitState(int index)
    {
        PerView.DeAttachView(this);
        CreateNotify(new Notify_ElementExitState { Index = Index });
        DestorySelf();  
    }

    void IBattleElement.AttachElement(GObject el)
    {
        GElement = el;
    }

    #endregion


    public void DestorySelf()
    {
        if (!this) return;
        Destroy(this.gameObject, 0.3f);
    }

    public virtual void OnJoined() { }

    public abstract IMessage ToInitNotify();

    protected void CreateNotify(IMessage notify)
    {
        PerView.AddNotify(notify);//  AddNotify();
    }
}
