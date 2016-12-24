using UnityEngine;
using System.Collections;
using Leap;

[RequireComponent(typeof(HandController))]
[RequireComponent(typeof(MainControl))]
public class GestureParser : MonoBehaviour {

	private const float GRAB_THRESHOLD = 0.9f;
	private const float DRAG_VELOCITY_THRESHOLD = 100f;
	private const float BASIC_Y_POSITION_FOR_2D = 175f;
	private const float THROW_VELOCITY_THRESHOLD = 500f;
	private const float SLICE_VELOCITY_THRESHOLD = 900f;
	private const float DOWNWARD_ANGLE_THRESHOLD = 0.3f;

	private HandController controller;
	private MainControl main;

	private bool flipPalm = false;
	private bool inSlice;
	private bool inGrab;
	private int lastGesture = -1;
	private Leap.Vector RHBeginPosition = null;
	private Leap.Vector LHBeginPosition = null;

	// Use this for initialization
	void Start () {
		controller = GetComponent<HandController>();
		main = GetComponent<MainControl> ();

		// enable gestures
		Controller leapController = controller.GetLeapController ();
		leapController.EnableGesture (Gesture.GestureType.TYPESWIPE);
		leapController.EnableGesture (Gesture.GestureType.TYPESCREENTAP);
	}
	
	// Update is called once per frame
	void Update () {
		Frame frame = controller.GetFrame ();
		Hand lefthand = null, righthand = null;
		if (frame.Hands != null && frame.Hands.Count > 0) {
			HandList hands = frame.Hands;
			// get hand reference
			foreach (Hand hand in hands) {
				if ((lefthand == null) && hand.IsLeft)
					lefthand = hand;
				if ((righthand == null) && hand.IsRight)
					righthand = hand;
			}

			// left hand is for controlling camera
			if (lefthand != null && lefthand.GrabStrength < GRAB_THRESHOLD) {
				if (LHBeginPosition != null) {
					main.LeftHandMove (lefthand.PalmPosition - LHBeginPosition);
				} else {
					LHBeginPosition = lefthand.PalmPosition;
				}
			} else {
				LHBeginPosition = null;
			}

			// gestures
			if (lastGesture != -1) {
				Gesture gesture = frame.Gesture (lastGesture);
				if (gesture.Type == Gesture.GestureType.TYPE_INVALID)
					lastGesture = -1;
			} else {
				GestureList gestures = frame.Gestures ();
				foreach (Gesture gesture in gestures) {
					if (gesture.Type == Gesture.GestureType.TYPE_SWIPE) {
						SwipeGesture swipeGesture = new SwipeGesture (gesture);
						lastGesture = gesture.Id;
						main.Swipe (swipeGesture.Direction, swipeGesture.Speed);
					} else if (gesture.Type == Gesture.GestureType.TYPE_SCREEN_TAP) {
						lastGesture = gesture.Id;
						main.Tap ();
					}
				}
			}

			// parse right hand
			if (righthand != null && righthand.Confidence > 0.3) {
				FingerList fingers = righthand.Fingers;
				// grab
				if (righthand.GrabStrength > GRAB_THRESHOLD) {
					inGrab = true;
					UpdateGrab (righthand);
					return;
				} else if (inGrab) {
					main.RightHandRelease ();
					inGrab = false;
				}

				// pinch
				if (righthand.PinchStrength > 0.2 &&
					fingers[2].IsExtended && fingers[3].IsExtended && fingers[4].IsExtended) {
					main.RightHandPinch (righthand.PinchStrength);
					return;
				}

				// slice
				if (!inSlice && righthand.PalmNormal.Roll < -1.35 && righthand.PalmNormal.Roll > -1.6 // vertical
				    && righthand.PalmVelocity.y < -SLICE_VELOCITY_THRESHOLD) {
					Debug.Log ("Slice");
					inSlice = true;
					main.RightHandSlice (new Plane (controller.transform.TransformDirection(righthand.PalmNormal.ToUnity ()), 
						controller.transform.TransformPoint (righthand.PalmPosition.ToUnityScaled ())));
					return;
				} else {
					inSlice = false;
				}
					
				// point
				if (fingers[1].IsExtended && (!fingers[2].IsExtended) && (!fingers[3].IsExtended) && (!fingers[4].IsExtended)) {
					Finger indexFinger = fingers[1];
					Vector target = indexFinger.StabilizedTipPosition - indexFinger.Direction * (100f / indexFinger.Direction.z);
					Vector3 targetFor2D = target.ToUnity ();
					main.RightHandPoint (new Vector2(targetFor2D.x, targetFor2D.y - BASIC_Y_POSITION_FOR_2D));
					return;
				} else {
					main.RightHandNotPoint ();
				}

				// palm vertically move
				if (righthand.GrabStrength <= 0.3f && 
					righthand.PalmNormal.AngleTo(Vector.Down) < DOWNWARD_ANGLE_THRESHOLD) {
					if (RHBeginPosition != null) {
						main.RightPalmMove(righthand.PalmPosition.y - RHBeginPosition.y);
					} else {
						RHBeginPosition = righthand.PalmPosition;
					}

					if(flipPalm) {
						main.RightHandFlip();
						flipPalm = false;
					}
				} else {
					RHBeginPosition = null;
				}

				// palm flip
				if(righthand.GrabStrength <= 0.3f &&
				   righthand.PalmNormal.AngleTo(Vector.Up) < DOWNWARD_ANGLE_THRESHOLD) {
					flipPalm = true;
				}
			}
		}
	}

	private void UpdateGrab(Hand hand) {
		Vector3 handPosition = controller.transform.TransformPoint(hand.PalmPosition.ToUnityScaled ());

		if (hand.PalmVelocity.Magnitude > DRAG_VELOCITY_THRESHOLD) {
			Vector3 handVelocity = controller.transform.TransformDirection (hand.PalmVelocity.ToUnityScaled());
			main.RightHandDrag (handPosition, handVelocity);
		}

		Vector handPositionFor2d = hand.StabilizedPalmPosition;
		Vector2 handPosition2d = new Vector2 (handPositionFor2d.x, handPositionFor2d.y);
		main.RightHandGrab (handPosition2d, handPosition);
	}
}
