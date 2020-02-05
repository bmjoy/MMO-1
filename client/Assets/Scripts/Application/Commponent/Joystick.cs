using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float maxRadius = 100; //Handle 移动最大半径
    public JoystickEvent OnValueChanged = new JoystickEvent(); //事件
    [System.Serializable] public class JoystickEvent : UnityEvent<Vector2> { }
    private RectTransform backGround, handle, direction; //摇杆背景、摇杆手柄、方向指引
    private Vector2 joysticValue = Vector2.zero;
    public bool IsDraging { get; private set; }
    private void Awake()
    {
        backGround = transform.Find("BackGround") as RectTransform;
        handle = transform.Find("BackGround/Handle") as RectTransform;
        direction = transform.Find("BackGround/Direction") as RectTransform;
        direction.gameObject.SetActive(false);
    }
    void Update()
    {
        if (IsDraging) //摇杆拖拽进行时驱动事件
        {
            joysticValue.x = handle.anchoredPosition.x / maxRadius;
            joysticValue.y = handle.anchoredPosition.y / maxRadius;
            OnValueChanged.Invoke(joysticValue);
        }
    }
    //按下时同步摇杆位置
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        Vector3 backGroundPos = new Vector3() // As it is too long for trinocular operation so I create Vector3 like this.
        {
            x = eventData.position.x,
            y = eventData.position.y,
            z = (null == eventData.pressEventCamera) ? backGround.position.z :
            eventData.pressEventCamera.WorldToScreenPoint(backGround.position).z //无奈，这个坐标转换不得不做啊,就算来来回回的折腾。
        };
        backGround.position = (null == eventData.pressEventCamera) ? backGroundPos : eventData.pressEventCamera.ScreenToWorldPoint(backGroundPos);
        //Vector3 vector;
        //if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out vector))
        //{
        //    backGround.position = vector;
        //}
        IsDraging = true;
    }
    // 当鼠标拖拽时
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Vector2 backGroundPos = (null == eventData.pressEventCamera) ?
            backGround.position : eventData.pressEventCamera.WorldToScreenPoint(backGround.position);
        Vector2 direction = eventData.position - backGroundPos; //得到方位盘中心指向光标的向量
        float distance = Vector3.Magnitude(direction); //获取向量的长度
        float radius = Mathf.Clamp(distance, 0, maxRadius); //锁定 Handle 半径
        handle.localPosition = direction.normalized * radius; //更新 Handle 位置
        UpdateDirectionArrow(direction);

        //Vector2 vector;
        //if (RectTransformUtility.ScreenPointToLocalPointInRectangle(backGround, eventData.position, eventData.pressEventCamera, out vector))
        //{
        //float distance = Vector3.Magnitude(vector); //获取向量的长度
        //float radius = Mathf.Clamp(distance, 0, maxRadius); //锁定 Handle 半径
        //handle.localPosition = vector.normalized * radius; //更新 Handle 位置
        //UpdateDirectionArrow(vector);
        //}
    }
    //更新指向器的朝向
    private void UpdateDirectionArrow(Vector2 position)
    {
        if (position.x != 0 || position.y != 0)
        {
            direction.gameObject.SetActive(true);
            direction.localEulerAngles = new Vector3(0, 0, Vector2.Angle(Vector2.right, position) * (position.y > 0 ? 1 : -1));
        }
    }
    // 当鼠标停止拖拽时
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        direction.gameObject.SetActive(false);
        backGround.localPosition = Vector3.zero;
        handle.localPosition = Vector3.zero;
        IsDraging = false;
    }
}
