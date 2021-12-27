
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 
/// * OnHover (isOver) is sent when the mouse hovers over a collider or moves away.
/// * OnPress (isDown) is sent when a mouse button gets pressed on the collider.
/// 
/// * OnDragStart () is sent to a game object under the touch just before the OnDrag() notifications begin.
/// * OnDrag (pos) is sent to an object that's being dragged.
/// * OnDragOver (draggedObject) is sent to a game object when another object is dragged over its area.
/// * OnDragOut (draggedObject) is sent to a game object when another object is dragged out of its area.
/// * OnDragEnd () is sent to a dragged object when the drag event finishes.
/// 
/// </summary>

public class MultiTouchScan : MonoBehaviour
{
	public enum ControlScheme
	{
		Mouse,
		Touch,
		Controller,
	}

	/// <summary>
	/// Whether the touch event will be sending out the OnClick notification at the end.
	/// </summary>

	public enum ClickNotification
	{
		None,
		Always,
		BasedOnDelta,
	}

	/// <summary>
	/// Ambiguous mouse, touch, or controller event.
	/// </summary>

	public class MouseOrTouch
	{
		public KeyCode key = KeyCode.None;
		public Vector2 pos;				// Current position of the mouse or touch event
		public Vector2 lastPos;			// Previous position of the mouse or touch event
		public Vector2 delta;			// Delta since last update
		public Vector2 totalDelta;		// Delta since the event started being tracked

		public Camera pressedCam;		// Camera that the OnPress(true) was fired with

		public GameObject last;			// Last object under the touch or mouse
		public GameObject current;		// Current game object under the touch or mouse
		public GameObject pressed;		// Last game object to receive OnPress
		public GameObject dragged;		// Game object that's being dragged

		public float pressTime = 0f;	// When the touch event started
		public float clickTime = 0f;	// The last time a click event was sent out

		public ClickNotification clickNotification = ClickNotification.Always;
		public bool touchBegan = true;
		public bool pressStarted = false;
		public bool dragStarted = false;
		public int ignoreDelta = 0;

		/// <summary>
		/// Delta time since the touch operation started.
		/// </summary>

		public float deltaTime { get { return RealTime.time - pressTime; } }
	}

	/// <summary>
	/// Which layers will receive events.
	/// </summary>

	public LayerMask eventReceiverMask = -1;


	/// <summary>
	/// Whether multi-touch is allowed.
	/// </summary>

	private bool allowMultiTouch = true;

	/// <summary>
	/// How much the mouse has to be moved after pressing a button before it starts to send out drag events.
	/// </summary>

	private float mouseDragThreshold = 4f;

	/// <summary>
	/// How far the mouse is allowed to move in pixels before it's no longer considered for click events, if the click notification is based on delta.
	/// </summary>

	private float mouseClickThreshold = 10f;

	/// <summary>
	/// How much the mouse has to be moved after pressing a button before it starts to send out drag events.
	/// </summary>

	private float touchDragThreshold = 40f;

	/// <summary>
	/// How far the touch is allowed to move in pixels before it's no longer considered for click events, if the click notification is based on delta.
	/// </summary>

	private float touchClickThreshold = 40f;

	/// <summary>
	/// Raycast range distance. By default it's as far as the camera can see.
	/// </summary>

	public float rangeDistance = -1f;

	/// <summary>
	/// Last raycast hit prior to sending out the event. This is useful if you want detailed information
	/// about what was actually hit in your OnClick, OnHover, and other event functions.
	/// Note that this is not going to be valid if you're using 2D colliders.
	/// </summary>

	private RaycastHit lastHit;

	/// <summary>
	/// Last camera active prior to sending out the event. This will always be the camera that actually sent out the event.
	/// </summary>

	private Camera currentCamera = null;

	/// <summary>
	/// Current control scheme. Derived from the last event to arrive.
	/// </summary>

	public ControlScheme currentScheme
	{
		get
		{
			if (mCurrentKey == KeyCode.None) return ControlScheme.Touch;
			if (mCurrentKey >= KeyCode.JoystickButton0) return ControlScheme.Controller;
			return ControlScheme.Mouse;
		}
		set
		{
			if (value == ControlScheme.Mouse)
			{
				currentKey = KeyCode.Mouse0;
			}
			else if (value == ControlScheme.Controller)
			{
				currentKey = KeyCode.JoystickButton0;
			}
			else if (value == ControlScheme.Touch)
			{
				currentKey = KeyCode.None;
			}
			else currentKey = KeyCode.Alpha0;
		}
	}

	/// <summary>
	/// ID of the touch or mouse operation prior to sending out the event.
	/// Mouse ID is '-1' for left, '-2' for right mouse button, '-3' for middle.
	/// </summary>

	private int currentTouchID = -100;

	static KeyCode mCurrentKey = KeyCode.Alpha0;

	/// <summary>
	/// Key that triggered the event, if any.
	/// </summary>

	public KeyCode currentKey
	{
		get
		{
			return mCurrentKey;
		}
		set
		{
			if (mCurrentKey != value)
			{
				mCurrentKey = value;
			}
		}
	}

	/// <summary>
	/// Ray projected into the screen underneath the current touch.
	/// </summary>

	public Ray currentRay
	{
		get
		{
			return (currentCamera != null && currentTouch != null) ?
				currentCamera.ScreenPointToRay(currentTouch.pos) : new Ray();
		}
	}

	/// <summary>
	/// Current touch, set before any event function gets called.
	/// </summary>

	private MouseOrTouch currentTouch = null;

	/// <summary>
	/// If set, this game object will receive all events regardless of whether they were handled or not.
	/// </summary>

	/// <summary>
	/// If events don't get handled, they will be forwarded to this game object.
	/// </summary>

	private GameObject fallThrough;

	public delegate void MoveDelegate (Vector2 delta);
	public delegate void VoidDelegate (GameObject go);
	public delegate void BoolDelegate (GameObject go, bool state);
	public delegate void FloatDelegate (GameObject go, float delta);
	public delegate void VectorDelegate (GameObject go, Vector2 delta);
	public delegate void ObjectDelegate (GameObject go, GameObject obj);
	public delegate void KeyCodeDelegate (GameObject go, KeyCode key);

	/// <summary>
	/// These notifications are sent out prior to the actual event going out.
	/// </summary>

	public BoolDelegate			onPress;
	public VectorDelegate		onDrag;
	public VoidDelegate			onDragStart;
	public ObjectDelegate		onDragOver;
	public ObjectDelegate		onDragOut;
	public VoidDelegate			onDragEnd;
	public ObjectDelegate		onDrop;

	// Mouse events
	static MouseOrTouch[] mMouse = new MouseOrTouch[] { new MouseOrTouch(), new MouseOrTouch(), new MouseOrTouch() };

	/// <summary>
	/// List of all the active touches.
	/// </summary>

	private List<MouseOrTouch> activeTouches = new List<MouseOrTouch>();

	// Used internally to store IDs of active touches
	static List<int> mTouchIDs = new List<int>();

	// Mouse input is turned off on iOS
	float mNextRaycast = 0f;

	static GameObject mRayHitObject;


	/// <summary>
	/// Convenience function that returns the main HUD camera.
	/// </summary>

	private Camera mainCamera;


	/// <summary>
	/// Raycast into the screen underneath the touch and update its 'current' value.
	/// </summary>

	public void Raycast (MouseOrTouch touch)
	{
		if (!Raycast(touch.pos)) mRayHitObject = fallThrough;
		touch.last = touch.current;
		touch.current = mRayHitObject;
	}

	/// <summary>
	/// Returns the object under the specified position.
	/// </summary>

	public bool Raycast (Vector3 inPos)
	{
		MultiTouchScan cam = this;

		Vector3 pos = currentCamera.ScreenToViewportPoint(inPos);
		if (float.IsNaN(pos.x) || float.IsNaN(pos.y)) return false;

		// If it's outside the camera's viewport, do nothing
		if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f) return false;

		// Cast a ray into the screen
		Ray ray = currentCamera.ScreenPointToRay(inPos);

		// Raycast into the screen
		int mask    = (int)cam.eventReceiverMask;
		float dist  = (cam.rangeDistance > 0f) ? cam.rangeDistance : currentCamera.farClipPlane - currentCamera.nearClipPlane;
		if (Physics.Raycast(ray, out lastHit, dist, mask))
		{
			mRayHitObject = lastHit.collider.gameObject;
		
			return true;
		}

		return false;
	}

	static int mNotifying = 0;

	/// <summary>
	/// Generic notification function. Used in place of SendMessage to shorten the code and allow for more than one receiver.
	/// </summary>

	public void Notify (GameObject go, string funcName, object obj)
	{
		if (mNotifying > 10) return;

		// Automatically forward events to the currently open popup list
		if (currentScheme == ControlScheme.Controller && UIPopupList.isOpen &&
			UIPopupList.current.source == go && UIPopupList.isOpen)
			go = UIPopupList.current.gameObject;

		if (go && go.activeInHierarchy)
		{
			++mNotifying;
			//if (currentScheme == ControlScheme.Controller)
			//	Debug.Log((go != null ? "[" + go.name + "]." : "[global].") + funcName + "(" + obj + ");", go);
			go.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
			--mNotifying;
		}
	}

	/// <summary>
	/// Get the details of the specified mouse button.
	/// </summary>

	public MouseOrTouch GetMouse (int button) { return mMouse[button]; }

	/// <summary>
	/// Get or create a touch event. If you are trying to iterate through a list of active touches, use activeTouches instead.
	/// </summary>

	public MouseOrTouch GetTouch (int id, bool createIfMissing = false)
	{
		if (id < 0) return GetMouse(-id - 1);

		for (int i = 0, imax = mTouchIDs.Count; i < imax; ++i)
			if (mTouchIDs[i] == id) return activeTouches[i];

		if (createIfMissing)
		{
			MouseOrTouch touch = new MouseOrTouch();
			touch.pressTime = RealTime.time;
			touch.touchBegan = true;
			activeTouches.Add(touch);
			mTouchIDs.Add(id);
			return touch;
		}
		return null;
	}

	/// <summary>
	/// Remove a touch event from the list.
	/// </summary>

	public void RemoveTouch (int id)
	{
		for (int i = 0, imax = mTouchIDs.Count; i < imax; ++i)
		{
			if (mTouchIDs[i] == id)
			{
				mTouchIDs.RemoveAt(i);
				activeTouches.RemoveAt(i);
				return;
			}
		}
	}

	/// <summary>
	/// Add this camera to the list.
	/// </summary>

	void Awake ()
	{
		mainCamera = Camera.main;
		currentCamera = mainCamera;

		#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_WP_8_1 || UNITY_BLACKBERRY || UNITY_WINRT || UNITY_METRO)
		currentScheme = ControlScheme.Touch;
		#else
		//if (Application.platform == RuntimePlatform.PS3 ||
		//Application.platform == RuntimePlatform.XBOX360)
		{
		currentScheme = ControlScheme.Controller;
		}
		#endif

		// Save the starting mouse position
		mMouse[0].pos = Input.mousePosition;

		for (int i = 1; i < 3; ++i)
		{
			mMouse[i].pos = mMouse[0].pos;
			mMouse[i].lastPos = mMouse[0].pos;
		}

	}

	/// <summary>
	/// Sort the list when enabled.
	/// </summary>

	void OnEnable ()
	{
		
	}

	/// <summary>
	/// Remove this camera from the list.
	/// </summary>

	void OnDisable () {
		
	}

	/// <summary>
	/// We don't want the camera to send out any kind of mouse events.
	/// </summary>

	void Start ()
	{
		if (Application.isPlaying)
		{
			// Always set a fall-through object
			if (fallThrough == null)
			{
				fallThrough = gameObject;
			}
		}
	}

	#if UNITY_EDITOR
	void OnValidate () { Start(); }
	#endif

	/// <summary>
	/// Check the input and send out appropriate events.
	/// </summary>

	void Update ()
	{

	}

	/// <summary>
	/// Keep an eye on screen size changes.
	/// </summary>

	void LateUpdate ()
	{
		#if UNITY_EDITOR
		if (!Application.isPlaying) return;
		#endif

		//if (BattleSystem.Instance.battleData.gameState == GameState.Game)
		ProcessEvents();
	}

	/// <summary>
	/// Process all events.
	/// </summary>

	void ProcessEvents ()
	{
		// Process touch events first
		ProcessTouches();

		currentTouchID = -100;
	}

	/// <summary>
	/// Update mouse input.
	/// </summary>

	public void ProcessMouse ()
	{
		// Is any button currently pressed?
		bool isPressed = false;

		for (int i = 0; i < 3; ++i)
		{
			if (Input.GetMouseButtonDown(i))
			{
				currentKey = KeyCode.Mouse0 + i;
				isPressed = true;
			}
			else if (Input.GetMouseButton(i))
			{
				currentKey = KeyCode.Mouse0 + i;
				isPressed = true;
			}
		}

		// We're currently using touches -- do nothing
		if (currentScheme == ControlScheme.Touch) return;

		currentTouch = mMouse[0];

		// Update the position and delta
		Vector2 pos = Input.mousePosition;

		if (currentTouch.ignoreDelta == 0)
		{
			currentTouch.delta = pos - currentTouch.pos;
		}
		else
		{
			--currentTouch.ignoreDelta;
			currentTouch.delta.x = 0f;
			currentTouch.delta.y = 0f;
		}

		float sqrMag = currentTouch.delta.sqrMagnitude;
		currentTouch.pos = pos;

		bool posChanged = false;

		if (currentScheme != ControlScheme.Mouse)
		{
			if (sqrMag < 0.001f) return; // Nothing changed and we are not using the mouse -- exit
			currentKey = KeyCode.Mouse0;
			posChanged = true;
		}
		else if (sqrMag > 0.001f) posChanged = true;

		// Propagate the updates to the other mouse buttons
		for (int i = 1; i < 3; ++i)
		{
			mMouse[i].pos = currentTouch.pos;
			mMouse[i].delta = currentTouch.delta;
		}

		// No need to perform raycasts every frame
		if (isPressed || posChanged || mNextRaycast < RealTime.time)
		{
			mNextRaycast = RealTime.time + 0.02f;
			Raycast(currentTouch);
			for (int i = 0; i < 3; ++i) mMouse[i].current = currentTouch.current;
		}

		bool highlightChanged = (currentTouch.last != currentTouch.current);

		currentTouchID = -1;
		if (highlightChanged) currentKey = KeyCode.Mouse0;

		// Generic mouse move notifications
		if (posChanged)
		{
			currentTouch = null;
		}

		// Process all 3 mouse buttons as individual touches
		for (int i = 0; i < 3; ++i)
		{
			bool pressed = Input.GetMouseButtonDown(i);
			bool unpressed = Input.GetMouseButtonUp(i);
			if (pressed || unpressed) currentKey = KeyCode.Mouse0 + i;
			currentTouch = mMouse[i];

			#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			if (i == 0 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				currentTouchID = -2;
				currentKey = KeyCode.Mouse1;
			}
			else
			#endif
			{
				currentTouchID = -1 - i;
				currentKey = KeyCode.Mouse0 + i;
			}

			// We don't want to update the last camera while there is a touch happening
			if (pressed)
			{
				currentTouch.pressedCam = currentCamera;
				currentTouch.pressTime = RealTime.time;
			}
			else if (currentTouch.pressed != null) currentCamera = currentTouch.pressedCam;

			// Process the mouse events
			ProcessTouch(pressed, unpressed);
		}

		// If nothing is pressed and there is an object under the touch, highlight it
		if (!isPressed && highlightChanged)
		{
			currentTouch = mMouse[0];
			currentTouchID = -1;
			currentKey = KeyCode.Mouse0;
		}

		currentTouch = null;

		// Update the last value
		mMouse[0].last = mMouse[0].current;
		for (int i = 1; i < 3; ++i) mMouse[i].last = mMouse[0].last;
	}

	static bool mUsingTouchEvents = true;

	public class Touch
	{
		public int fingerId;
		public TouchPhase phase = TouchPhase.Began;
		public Vector2 position;
		public int tapCount = 0;
	}

	/// <summary>
	/// Update touch-based events.
	/// </summary>

	public void ProcessTouches ()
	{
		int count = Input.touchCount;

		for (int i = 0; i < count; ++i)
		{
			int fingerId;
			TouchPhase phase;
			Vector2 position;
			int tapCount;

			UnityEngine.Touch touch = Input.GetTouch(i);
			phase = touch.phase;
			fingerId = touch.fingerId;
			position = touch.position;
			tapCount = touch.tapCount;

			currentTouchID = allowMultiTouch ? fingerId : 1;
			currentTouch = GetTouch(currentTouchID, true);

			bool pressed = (phase == TouchPhase.Began) || currentTouch.touchBegan;
			bool unpressed = (phase == TouchPhase.Canceled) || (phase == TouchPhase.Ended);
			currentTouch.delta = position - currentTouch.pos;
			currentTouch.pos = position;
			currentKey = KeyCode.None;

			// Raycast into the screen
			Raycast(currentTouch);

			// We don't want to update the last camera while there is a touch happening
			if (pressed) currentTouch.pressedCam = currentCamera;
			else if (currentTouch.pressed != null) currentCamera = currentTouch.pressedCam;

			// Double-tap support
			if (tapCount > 1) currentTouch.clickTime = RealTime.time;

			// Process the events from this touch
			ProcessTouch(pressed, unpressed);

			// If the touch has ended, remove it from the list
			if (unpressed) RemoveTouch(currentTouchID);

			currentTouch.touchBegan = false;
			currentTouch.last = null;
			currentTouch = null;

			// Don't consider other touches
			if (!allowMultiTouch) break;
		}

		if (count == 0)
		{
			// Skip the first frame after using touch events
			if (mUsingTouchEvents)
			{
				mUsingTouchEvents = false;
				return;
			}

			ProcessMouse();
		}
		else mUsingTouchEvents = true;
	}

	/// <summary>
	/// Process the press part of a touch.
	/// </summary>

	void ProcessPress (bool pressed, float click, float drag)
	{
		// Send out the press message
		if (pressed)
		{
			currentTouch.pressStarted = true;
			if (onPress != null && currentTouch.pressed)
				onPress(currentTouch.pressed, false);

			Notify(currentTouch.pressed, "OnPress", false);

			currentTouch.pressed = currentTouch.current;
			currentTouch.dragged = currentTouch.current;
			currentTouch.clickNotification = ClickNotification.BasedOnDelta;
			currentTouch.totalDelta = Vector2.zero;
			currentTouch.dragStarted = false;

			if (onPress != null && currentTouch.pressed)
				onPress(currentTouch.pressed, true);

			Notify(currentTouch.pressed, "OnPress", true);

		}
		else if (currentTouch.pressed != null && (currentTouch.delta.sqrMagnitude != 0f || currentTouch.current != currentTouch.last))
		{
			// Keep track of the total movement
			currentTouch.totalDelta += currentTouch.delta;
			float mag = currentTouch.totalDelta.sqrMagnitude;
			bool justStarted = false;

			// If the drag process hasn't started yet but we've already moved off the object, start it immediately
			if (!currentTouch.dragStarted && currentTouch.last != currentTouch.current)
			{
				currentTouch.dragStarted = true;
				currentTouch.delta = currentTouch.totalDelta;

				// OnDragOver is sent for consistency, so that OnDragOut is always preceded by OnDragOver

				if (onDragStart != null) onDragStart(currentTouch.dragged);
				Notify(currentTouch.dragged, "OnDragStart", null);

				if (onDragOver != null) onDragOver(currentTouch.last, currentTouch.dragged);
				Notify(currentTouch.last, "OnDragOver", currentTouch.dragged);

			}
			else if (!currentTouch.dragStarted && drag < mag)
			{
				// If the drag event has not yet started, see if we've dragged the touch far enough to start it
				justStarted = true;
				currentTouch.dragStarted = true;
				currentTouch.delta = currentTouch.totalDelta;
			}

			// If we're dragging the touch, send out drag events
			if (currentTouch.dragStarted)
			{
				bool isDisabled = (currentTouch.clickNotification == ClickNotification.None);

				if (justStarted)
				{
					if (onDragStart != null) onDragStart(currentTouch.dragged);
					Notify(currentTouch.dragged, "OnDragStart", null);

					if (onDragOver != null) onDragOver(currentTouch.last, currentTouch.dragged);
					Notify(currentTouch.current, "OnDragOver", currentTouch.dragged);
				}
				else if (currentTouch.last != currentTouch.current)
				{
					if (onDragOut != null) onDragOut(currentTouch.last, currentTouch.dragged);
					Notify(currentTouch.last, "OnDragOut", currentTouch.dragged);

					if (onDragOver != null) onDragOver(currentTouch.last, currentTouch.dragged);
					Notify(currentTouch.current, "OnDragOver", currentTouch.dragged);
				}

				if (onDrag != null) onDrag(currentTouch.dragged, currentTouch.delta);
				Notify(currentTouch.dragged, "OnDrag", currentTouch.pos);

				currentTouch.last = currentTouch.current;

				if (isDisabled)
				{
					// If the notification status has already been disabled, keep it as such
					currentTouch.clickNotification = ClickNotification.None;
				}
				else if (currentTouch.clickNotification == ClickNotification.BasedOnDelta && click < mag)
				{
					// We've dragged far enough to cancel the click
					currentTouch.clickNotification = ClickNotification.None;
				}
			}
		}
	}

	/// <summary>
	/// Process the release part of a touch.
	/// </summary>

	void ProcessRelease (bool isMouse, float drag)
	{
		// Send out the unpress message
		if (currentTouch == null) return;
		currentTouch.pressStarted = false;

		if (currentTouch.pressed != null)
		{

			// Send the notification of a touch ending
			if (onPress != null) onPress(currentTouch.pressed, false);
			Notify(currentTouch.pressed, "OnPress", false);

			// If there was a drag event in progress, make sure OnDragOut gets sent
			if (currentTouch.dragStarted)
			{
				if (onDragOut != null) onDragOut(currentTouch.last, currentTouch.dragged);
				Notify(currentTouch.last, "OnDragOut", currentTouch.dragged);

				if (onDragEnd != null) onDragEnd(currentTouch.dragged);
				Notify(currentTouch.dragged, "OnDragEnd", null);
			}

			// If the button/touch was released on the same object, consider it a click and select it
			if (currentTouch.dragStarted) // The button/touch was released on a different object
			{
				// Send a drop notification (for drag & drop)
				if (onDrop != null) onDrop(currentTouch.current, currentTouch.dragged);
				Notify(currentTouch.current, "OnDrop", currentTouch.dragged);
			}
		}
		currentTouch.dragStarted = false;
		currentTouch.pressed = null;
		currentTouch.dragged = null;
	}

	bool HasCollider (GameObject go)
	{
		if (go == null) return false;
		Collider c = go.GetComponent<Collider>();
		if (c != null) return c.enabled;
		Collider2D b = go.GetComponent<Collider2D>();
		return (b != null && b.enabled);
	}

	/// <summary>
	/// Process the events of the specified touch.
	/// </summary>

	public void ProcessTouch (bool pressed, bool released)
	{

		// Whether we're using the mouse
		bool isMouse = (currentScheme == ControlScheme.Mouse);
		float drag   = isMouse ? mouseDragThreshold : touchDragThreshold;
		float click  = isMouse ? mouseClickThreshold : touchClickThreshold;

		// So we can use sqrMagnitude below
		drag *= drag;
		click *= click;

		if (currentTouch.pressed != null)
		{
			if (released) ProcessRelease(isMouse, drag);
			ProcessPress(pressed, click, drag);

		}
		else if (isMouse || pressed || released)
		{
			ProcessPress(pressed, click, drag);
			if (released) ProcessRelease(isMouse, drag);
		}
	}

}
