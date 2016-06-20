using UnityEngine;

namespace UIComponent
{
	public class CursorVisible : MonoBehaviour
	{
		bool flg;
		void Update()
		{
			if (enabled == true)
			{
				Cursor.visible = true;
			}
		}
		void OnDisable()
		{
			Cursor.visible = false;
		}
		void OnApplicationQuit()
		{
			Cursor.visible = true;
		}
	}
}
