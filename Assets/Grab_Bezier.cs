using UnityEngine;
using System.Collections;

public class Grab_Bezier : MonoBehaviour {

	public GameObject mPointer;
	public GameObject mLine;
	public GameObject mCurveLine;

	public Vector3 startPoint;

	private bool hasHit;

	private Vector3 lastMousePos;
	private Vector3 lastStartPoint;
	private Vector3 direction;
	private float angle;
	private float lastAngle;
	private Quaternion targetRot;
	private Vector3 targetPoint;
		// Use this for initialization

	//public Vector3 p0,p1,p2,p3;

	public int SEGMENT_COUNT;

	void Start () {
		hasHit= false;

		startPoint = transform.localPosition;
		lastStartPoint = startPoint;
		direction = new Vector3(0,0,1.0f);
		//xlastAngle = 90;

		SEGMENT_COUNT = 19;
	}

	void Update () 
	{
		RaycastHit rayHit;

		if(Input.GetMouseButtonDown(0)) 
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if(Physics.Raycast(ray, out rayHit))
			{
				toggleCamFollow();
				hasHit = true;
				lastMousePos = Input.mousePosition;
				//startPoint = transform.position;
			}
		}

		if(hasHit)
		{
			//Deal with camera movement and mouse position change
			Vector2 moveDelta = lastMousePos - Input.mousePosition;
			Vector3 pos = new Vector3(moveDelta.x/25.0f, 0, moveDelta.y/25.0f);
			Quaternion camRot = Camera.main.transform.rotation;
			pos = camRot * pos;
			pos.y = 0;
			transform.localPosition -= pos;

			// first control point is startpoint plus "velocity" in "direction"
			Vector3 ControlP1 = startPoint;
			ControlP1 += direction;

			//target point is where you are now
			targetPoint = transform.localPosition;
			//second control point is the same position as target point. 
			Vector3 ControlP2 = targetPoint;

			//find the desired end angle
			angle = Mathf.Atan2(targetPoint.z - startPoint.z, targetPoint.x - startPoint.x);
			angle = angle * Mathf.Rad2Deg;
			angle *= 2.0f;
			angle -= 90.0f;
	
			//do the rotation. 
			targetRot = Quaternion.Euler(0, -angle,0);
			transform.rotation = targetRot;

			//Draw the curve
			for(int i = 0; i <= SEGMENT_COUNT; i++)
			{
				float t = i / (float) SEGMENT_COUNT;
				Vector3 q1 = CalculateBezierPoint(t, startPoint, ControlP1, targetPoint, targetPoint);
				mCurveLine.GetComponent<LineRenderer>().SetPosition(i, q1);
			}

			//debug lines 
			Debug.DrawLine(startPoint, ControlP1);
			Debug.DrawLine(ControlP1, targetPoint);
			Vector3 test = transform.localPosition - startPoint;
			Vector3 newControlPoint = targetPoint + test;
			Debug.DrawLine(targetPoint, newControlPoint);

			//mouse pos for next frame
			lastMousePos = Input.mousePosition;
		}

		if(Input.GetMouseButtonUp(0) && hasHit)
		{
			//stop updating curve draw
			hasHit = false;

			direction = transform.localPosition - startPoint;//targetRot * direction;
			startPoint = transform.localPosition;

			Vector3 newControlPoint = startPoint + direction;
			transform.position = newControlPoint;
			targetPoint = newControlPoint;

			for(int i = 0; i <= SEGMENT_COUNT; i++)
			{
				float t = i / (float) SEGMENT_COUNT;
				Vector3 q1 = CalculateBezierPoint(t, startPoint, newControlPoint, newControlPoint, newControlPoint);
				mCurveLine.GetComponent<LineRenderer>().SetPosition(i, q1);
			}

			lastAngle = angle;

			toggleCamFollow();
		}
	}



	void toggleCamFollow()
	{
		/*
		if(Camera.main.GetComponent<SmoothFollow>().enabled)
		{
			Camera.main.GetComponent<SmoothFollow>().enabled = false;
		} else {
			Camera.main.GetComponent<SmoothFollow>().enabled = true;
		}*/
	}

	Vector3 CalculateBezierPoint(float t,Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1.0f - t;
		float tt = t*t;
		float uu = u*u;
		float uuu = uu * u;
		float ttt = tt * t;
		
		Vector3 p = uuu * p0; //first term
		p += 3.0f * uu * t * p1; //second term
		p += 3.0f * u * tt * p2; //third term
		p += ttt * p3; //fourth term
		
		return p;
	}

}
