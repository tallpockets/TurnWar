using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Grab : MonoBehaviour {

	public GameObject mDirectionPointer;
	public GameObject mControlGrab;
	public GameObject mCurveLine;
	public GameObject mShip;
	public GameObject mSlider;
	private float mSliderTime;

	public float kTurnStep; //how long is the time in pause turn
	public float kTimeStep;///how much time passes in a simulate turn
	private float mStep;

	private Vector3 mCurrentPosition;
	private Vector3 mLastCurrentPosition;
	private Vector3 mCurrentDirection;
	private Vector3 mLastCurrentDirection;
	private int mDirectionUpdateModulo;

	public Vector3 mStartPoint;
	private Vector3 mTargetPoint;
	private Vector3 mDirection;

	private Vector3 resultDirectionPoint;
	private Vector3 resultTargetPoint;

	private Vector3 mLastMousePos;

	private bool mGo;
	private bool mLastGo;

	private bool mHasHit;
	private bool mNoInput;
	private bool mFirstFrame;

	public int mSegmentCount;

	void Start () {
		mHasHit= false;
		mNoInput = true;
		mFirstFrame = true;
		mGo = false;
		kTimeStep = 3.0f;

		mStartPoint = new Vector3(0,4,0);
		mShip.transform.position = mStartPoint;

		mDirectionPointer.transform.position = new Vector3(0,4,5);
		mDirection = new Vector3(0,0,5.0f);

		StartCoroutine("Turn");
	}


	IEnumerator Turn()
	{
		yield return new WaitForSeconds(kTurnStep);
		GoShipGo();
	}
	
	
	void GoShipGo(){
		if(mGo == false)
		{
			mGo = true;
			mHasHit = false;
			mStep = 0;
			mSliderTime = 0;
			mLastCurrentPosition = new Vector3(0,0,0);
		}
	}


	void Update () 
	{
		RaycastHit rayHit;

		if(Input.GetMouseButtonDown(0)) 
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if(Physics.Raycast(ray, out rayHit))
			{
				if(rayHit.transform.gameObject.layer == (int)8)
				{
					mHasHit = true;
					mNoInput = false;
				}
				mLastMousePos = Input.mousePosition;
			}
		}

		if(mHasHit || mFirstFrame)
		{

			if(!mFirstFrame)
			{
				//Deal with mouse position change
				Vector2 moveDelta = mLastMousePos - Input.mousePosition;
				Vector3 pos = new Vector3(moveDelta.x/15.0f, 0, moveDelta.y/15.0f);
				//Deal with camera movement for position change
				Quaternion camRot = Camera.main.transform.rotation;
				pos = camRot * pos;
				pos.y = 0;
				//update control point position
				mControlGrab.transform.position -= pos;
			}
			mFirstFrame = false;

			mTargetPoint = mControlGrab.transform.position;

			//Draw the projected trajectory
			for(int i = 0; i <= mSegmentCount; i++)
			{
				float t = i / (float) mSegmentCount;
				Vector3 dir = mTargetPoint - mStartPoint;
				Vector3 q1 = mStartPoint + ((mDirection + (dir * t)) * t);
				mCurveLine.GetComponent<LineRenderer>().SetPosition(i, q1);

				if(i == mSegmentCount-1)
					resultDirectionPoint = q1;

				resultTargetPoint = q1;
			}

			//find direction of the end points of curve (direction of new trajectory)
			Vector3 d = resultDirectionPoint - resultTargetPoint;
			float angle = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;

			//rotate the direction pointer to match
			Quaternion targetRot = Quaternion.Euler(0,angle - 180.0f,0);
			mDirectionPointer.transform.rotation = targetRot;
			mDirectionPointer.transform.position = resultTargetPoint;

			//debug line draw 
			Debug.DrawLine(mStartPoint, mTargetPoint);

			//mouse pos for next frame
			mLastMousePos = Input.mousePosition;
		}


		if(Input.GetMouseButtonUp(0) && mHasHit)
		{
			mHasHit = false;
		}

		if(mGo == true)
		{
			//only do this the first time you start animating
			if(mGo == true && mGo != mLastGo)
			{
				//enable vfx
				//disable line draw
				mCurveLine.GetComponent<LineRenderer>().enabled = false;
				mDirectionPointer.SetActive(false);
			}
		
			//get the current position
			Vector3 dir = mTargetPoint - mStartPoint;
			Vector3 mCurrentPosition = mStartPoint + ((mDirection + (dir * mStep/kTimeStep)) * mStep/kTimeStep);
			mShip.transform.position = mCurrentPosition;


			//skip rotating the first step, since we are already in the right rotation
			//and because it's a pain to dig up the previous position at this point
			if(mStep != 0)
			{
				//find the new direction to point in
				mCurrentDirection = mCurrentPosition - mLastCurrentPosition;
			
				float angle = Mathf.Atan2(mCurrentDirection.x, mCurrentDirection.z) * Mathf.Rad2Deg;

				//float bank = Vector3.Dot(mLastCurrentDirection.normalized, mCurrentDirection.normalized);
				//bank = Mathf.Abs(bank-1);
				//bank = bank * 90.0f;

				//rotate the ship
				Quaternion targetRot = Quaternion.Euler(0,angle,0);
				mShip.transform.rotation = targetRot;
			}

			//do this at the very end
			if(mStep >= kTimeStep)
			{
				mGo = false;
				resultDirectionPoint = mLastCurrentPosition;
				resultTargetPoint = mCurrentPosition;

				SetupNextMove();
				mNoInput = true; //must happen after SetupNextMove

				//disable vfx
				//enable line draw
				mCurveLine.GetComponent<LineRenderer>().enabled = true;
				mDirectionPointer.SetActive(true);

				StartCoroutine("Turn");
			}

			mLastCurrentPosition = mCurrentPosition;
			mLastCurrentDirection = mCurrentDirection;
			mStep += Time.deltaTime;
		}

		if(mGo == false)
		{
			Slider slid = mSlider.GetComponent<Slider>();
			slid.value = Mathf.Abs((mSliderTime/kTurnStep)-1);

			mSliderTime += Time.deltaTime; 
		}

		mLastGo = mGo;
	}




	public void SetupNextMove()
	{	

		if(mNoInput){
			//if not input continue in same direction
		} else{
			//update direction
			mDirection = resultTargetPoint - resultDirectionPoint;
			//add magnitude to direction
			Vector3 dMag = resultTargetPoint - mStartPoint;
			mDirection = mDirection.normalized * dMag.magnitude;
		}

		//update start point = result end position
		mStartPoint = resultTargetPoint;
		
		//new target point (start + direction*magnitude)
		mTargetPoint = resultTargetPoint + mDirection;
		
		//update ui positions
		mControlGrab.transform.position = mStartPoint;
		mDirectionPointer.transform.position = mTargetPoint;
		
		//draw new curve showing next turn direction
		for(int i = 0; i <= mSegmentCount; i++)
		{
			float t = i / (float) mSegmentCount;
			Vector3 q1 = mStartPoint + mDirection * t;
			mCurveLine.GetComponent<LineRenderer>().SetPosition(i, q1);
		} 

	}
}
