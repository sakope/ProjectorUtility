using UnityEngine;
using UnityEngine.Events;

namespace UIComponent
{
	public class Edit : MonoBehaviour
	{
		public GameObject target;
		public KeyCode settingKeyCode;
		public UnityEvent onKeyDown;
		public UnityEvent onEditUpdate;

		private bool edit;
		void Start ()
		{
			target.SetActive (false);
		}
		void Update ()
		{
			if (Input.GetKeyDown (settingKeyCode) == true)
			{
				edit = !edit;
				var active = !target.activeSelf;
				target.SetActive(active);
				onKeyDown.Invoke();
			}
			if(edit == true)
			{
				onEditUpdate.Invoke();
			}
		}
	}
}