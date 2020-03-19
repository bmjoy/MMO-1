﻿using Layout.EditorAttributes;

namespace Layout.LayoutElements
{

    [EditorLayout("播放声音", ViewOnly = true)]
    public  class PlaySoundLayout:LayoutBase
    {
        [Label("子物体资源目录")]
        [EditorResourcePath]
        public string resourcesPath;
        [Label("目标")]
        public TargetType target;

        [Label("起始骨骼")]
        [EditorBone]
        public string fromBone;
        [Label("音量")]
        public float value =1;
    }
}
