using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Net.Security;

using SimpleJSON;
using GreendeckUnity;

public class PlayerController : MonoBehaviour 
{	
	public float speed;
	public GUIText countText;
	public GUIText winText;
	private int count;
	private int numberOfGameObjects;
	Rigidbody rb;
	public static GreendeckUnityAPI gd;
	public float speedAc;

	//accelerometer
	private Vector3 zeroAc;
	private Vector3 curAc;
	private float sensH = 10;
	private float sensV = 10;
	private float smooth = 0.5f;
	private float GetAxisH = 0;
	private float GetAxisV = 0;

	

	void Awake(){
		#if (UNITY_IPHONE && !UNITY_EDITOR)
		DontDestroyOnLoad(gameObject);
		#endif

		DontDestroyOnLoad(gameObject);

		

	}
		
	void Start()
	{
		
		count = 0;
		rb = GetComponent<Rigidbody>();
		SetCountText();
		winText.text = "";
		numberOfGameObjects =14;

		ResetAxes (); 

		String CLIENT_ID = "72f8dd6fef58edcabfc4b0ed605131e0d7dda6361a353f2fd61be82f923f90c7";
		String CLIENT_SECRET = "ed49bc06bfb8f33fff5179c0c792b37db4f780109f4a9d4ce4d9f9ec7ae909d0";

		gd = GreendeckUnityAPI.initialize (CLIENT_ID, CLIENT_SECRET);


	}
	void Update()
	{

		if (SystemInfo.deviceType == DeviceType.Desktop) 
		{
			// Exit condition for Desktop devices
			if (Input.GetKey("escape"))
				Application.Quit();
		}
		else
		{
			// Exit condition for mobile devices
			if (Input.GetKeyDown(KeyCode.Escape))
				Application.Quit();            
		}



	}
	
	void FixedUpdate ()
	{
		if (SystemInfo.deviceType == DeviceType.Desktop) {
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");
		
		Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
		
		rb.AddForce (movement * speed * Time.deltaTime);
		}else 
		{
			//get input by accelerometer
			curAc = Vector3.Lerp(curAc, Input.acceleration-zeroAc, Time.deltaTime/smooth);
			GetAxisV = Mathf.Clamp(curAc.y * sensV, -1, 1);
			GetAxisH = Mathf.Clamp(curAc.x * sensH, -1, 1);
			// now use GetAxisV and GetAxisH instead of Input.GetAxis vertical and horizontal
			// If the horizontal and vertical directions are swapped, swap curAc.y and curAc.x
			// in the above equations. If some axis is going in the wrong direction, invert the
			// signal (use -curAc.x or -curAc.y)

			Vector3 movement = new Vector3 (GetAxisH, 0.0f, GetAxisV);

			rb.AddForce(movement * speedAc);

		}
	}
	
	void OnTriggerEnter(Collider other) 
	{
		print ("OnTriggerEnter");

		if (other.gameObject.tag=="PickUp")
		{
			print ("Tag is pickup with count: "+ count);
			other.gameObject.SetActive (false);
			count = count + 1;

			print ("Tag is pickup with incremented count: "+ count);

			SetCountText();

			// record event with properties
			Dictionary<string, object> Props = new Dictionary<string, object> ();
			Props.Add("Score", ""+count);

			GreendeckUnityAPI.Track("Collission Triggered");

			print ("event recorded"+ count);
		}
	}
	
	void SetCountText()
	{
		countText.text = "Count: " + count.ToString();

		

		print ("SetCountText"+ count);

		if (count == 1) {

			GreendeckUnityAPI.Identify("user3@gmail.com");

		}
		if (count >= 14) {

			print ("true PlayerWins"+ count);
			winText.text = "YOU WIN!";


		} else {
			print ("false PlayerWins"+ count);
			winText.text = "";
		}
	}
	//accelerometer
	void ResetAxes(){
		zeroAc = Input.acceleration;
		curAc = Vector3.zero;
	}



}

