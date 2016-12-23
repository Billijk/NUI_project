using UnityEngine;
using System.Collections;
using Leap;

public class GestureParser : MonoBehaviour {

	private const float GRAB_THRESHOLD = 0.9f;
	private const float GRAB_DISTANCE = 0.5f;
	private const float GRAB_STRENGTH = 50f;
	private const int HAND_LAYER_INDEX = 8;
	private const float SLICE_VELOCITY_THRESHOLD = 1000f;

	private HandController controller;
	private MainControl main;

	private bool inSlice;
	private int lastGesture = -1;
	private Leap.Vector LHBeginPosition;

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
						ScreenTapGesture tapGesture = new ScreenTapGesture (gesture);
						lastGesture = gesture.Id;
						main.Tap ();
					}
				}
			}

			// parse right hand
			if (righthand != null && righthand.Confidence > 0.3) {
				// grab
				if (righthand.GrabStrength > GRAB_THRESHOLD) {
					UpdateGrab (righthand);
				} else {
					main.RightHandRelease ();
				}

				// pinch
				if (righthand.PinchStrength > 0.2) {
					main.RightHandPinch (righthand.PinchStrength);
				}

				// slice
				if (!inSlice && righthand.PalmNormal.Roll < -1.7 && righthand.PalmNormal.Roll > -1.9// vertical
				    && righthand.PalmVelocity.y < -SLICE_VELOCITY_THRESHOLD) {
					inSlice = true;
					main.RightHandSlice (new Plane (righthand.PalmNormal.ToUnity (), 
						controller.transform.TransformPoint (righthand.PalmPosition.ToUnityScaled ())));
				} else {
					inSlice = false;
				}
					
				// point
				FingerList extended = righthand.Fingers.Extended ();
				if (extended.Count == 1 && extended [0].Type == Finger.FingerType.TYPE_INDEX) {
					Finger indexFinger = extended [0];
					Vector target = indexFinger.StabilizedTipPosition - indexFinger.Direction * (100f / indexFinger.Direction.z);
					main.RightHandPoint (target.x, target.y);
				} else {
					main.RightHandNotPoint ();
				}
			} else {
				main.RightHandNotPoint ();
				main.RightHandRelease ();
			}
		}
	}

	private void UpdateGrab(Hand hand) {
		Collider grabbed = null;
		Vector3 handPosition = controller.transform.TransformPoint(hand.PalmPosition.ToUnityScaled ());

		int layerMask = ~(1 << HAND_LAYER_INDEX);
		Collider[] closeThings = Physics.OverlapSphere (handPosition, GRAB_DISTANCE, layerMask);
		float shortestDist = GRAB_DISTANCE;
		for (int i = 0; i < closeThings.Length; ++i) {
			Vector3 dist = handPosition - closeThings [i].transform.position;
			if (closeThings [i].GetComponent<Rigidbody>() != null && dist.magnitude < shortestDist) {
				grabbed = closeThings [i];
				shortestDist = dist.magnitude;
			}
		}

		if (grabbed != null) {
			main.RightHandGrab (grabbed.gameObject);
			//Vector3 delta = handPosition - grabbed.transform.position;
			//grabbed.GetComponent<Rigidbody> ().AddForce (delta * GRAB_STRENGTH);
		} else {
			// ray cast
			RaycastHit hit;
			if (Physics.Raycast (handPosition, hand.PalmNormal.ToUnity (), out hit, 15f, layerMask)) {
				main.RightHandGrab (hit.collider.gameObject);
			}
				
		}

	}
}
