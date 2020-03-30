using System;
using Layout.EditorAttributes;
using Proto;

namespace Layout.LayoutElements
{
    public enum CharacterType
    { 
        ConfigID, //配表
        OwnerID //复制自己
    }

    [EditorLayout("召唤单位")]
    public class CallUnitLayout:LayoutBase
    {
        public CallUnitLayout()
        {
            valueFrom = GetValueFrom.CurrentConfig;
            level = 1;
        }

        [Label("召唤ID来源")]
        public CharacterType CType = CharacterType.ConfigID;

        [Label("召唤角色ID")]
        public int characterID;

        [Label("等级取之来源")]
        public GetValueFrom valueFrom;

        [Label("等级")]
        public int level;


        [Label("AIPath(默认角色表AI)")]
        [EditorStreamingPath]
        public string AIPath;

        [Label("持续时间(秒)")]
        public float time;

        [Label("召唤物最大数量")]
        public int maxNum;

        [Label("偏移")]
        public Vector3 offset = new Vector3(0,0,1);

        public override string ToString()
        {
            return string.Format("time:{0} ID:{1}", time, characterID);
        }
    }
}
