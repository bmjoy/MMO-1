using Layout.EditorAttributes;

namespace Layout.LayoutElements
{
    [EditorLayout("发射自己")]
    public class LaunchSelfLayout: LayoutBase
    {
        [Label("长度")]
        public float distance;
        [Label("速度")]
        public float speed;
    }
}
