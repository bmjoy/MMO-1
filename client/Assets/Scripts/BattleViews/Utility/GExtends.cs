using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace UGameTools
{
    public static class GExtends
	{
        public static void SetLayer(this UnityEngine.GameObject u, string layer)
        {
            u.layer = UnityEngine.LayerMask.NameToLayer(layer);
        }

        public static UIMouseClick OnMouseClick(this Component mo,Action<object> callBack, object state = null)
        {
            var click = mo.gameObject.TryAdd<UIMouseClick>();
            click.userState = state;
            click.OnClick = callBack;
            return click;
        }

        public static T TryAdd<T>(this GameObject obj) where T:Component
        {
            var c =obj.GetComponent<T>();
            if (c != null) return c;
            return obj.AddComponent<T>();
        }

        public static void RestRTS(this Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localScale = Vector3.one;
            trans.localRotation = Quaternion.identity;
        }

        public static void SetLayer(this Transform trans ,int layer)
        {
            trans.gameObject.layer = layer;
            foreach (var i in trans.GetComponentsInChildren<Transform>()) i.gameObject.layer = layer;
        }


        public static T FindChild<T> (this Transform trans, string name) where T :Component
		{
			var t = FindInAllChild (trans, name);
			if (t == null)
				return null;
			else
				return t.GetComponent<T> ();
		}

        private static Transform FindInAllChild(Transform trans, string name)
        {
            if (trans.name == name) { return trans; }

            for (var i = 0; i < trans.childCount; i++)
            {
                var t = FindInAllChild(trans.GetChild(i), name);
                if (t != null) return t;
            }
            return null;
        }


		public static void ActiveSelfObject (this Component com, bool active)
		{
			com.gameObject.SetActive (active);
		}

        public static void SetText(this Button bt, string text)
        {
            var t = bt.transform.FindChild<Text>("Text");
            if (t == null) return;
            t.text = text;
        }
            
        public static Vector3 ToGVer3(this Proto.Vector3 v3)
        {
            return new Vector3(v3.X, v3.Y, v3.Z);
        }

        public static Layout.Vector3 ToLVer3(this Proto.Vector3 v3)
        {
            return new Layout.Vector3(v3.X, v3.Y, v3.Z);
        }

        public static UnityEngine.Vector3 ToVer3(this Proto.Vector3 v3)
        {
            return new Vector3(v3.X, v3.Y, v3.Z);
        }

        public static Proto.Vector3 ToPVer3(this Vector3 uv3)
        {
            return new Proto.Vector3(){ X = uv3.x, Y = uv3.y, Z = uv3.z };
        }
            
        public static void DrawSphere(Vector3 center, float m_Radius)
        {

            // 绘制圆环
            Vector3 beginPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;
            for (float theta = 0; theta < 2 * Mathf.PI; theta += 0.2f)
            {
                float x = m_Radius * Mathf.Cos(theta);
                float z = m_Radius * Mathf.Sin(theta);
                Vector3 endPoint = new Vector3(x, 0, z);
                if (theta == 0)
                {
                    firstPoint = endPoint;
                }
                else
                {
                    Gizmos.DrawLine(beginPoint+center, endPoint+center);
                }
                beginPoint = endPoint;
            }

            // 绘制最后一条线段
            Gizmos.DrawLine(firstPoint+center, beginPoint+center);
        }


        public static IList<int> SplitToInt(this string str, char sKey = '|')
        {
            var arrs = str.Split(sKey);
            var list = new List<int>();
            foreach (var i in arrs) list.Add(int.Parse(i));
            return list;
        }
    }
}
