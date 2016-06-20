using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIComponent
{
	public class Draggable : MonoBehaviour,IBeginDragHandler, IDragHandler
	{
		public Vector2 position;
		public void OnBeginDrag(PointerEventData pointerEventData)
		{
			position = pointerEventData.position;
		}
		public void OnDrag(PointerEventData pointerEventData)
		{
			var d = pointerEventData.position - position;
			transform.position = d + (Vector2)transform.position;
			position = pointerEventData.position;
		}
	}
}
