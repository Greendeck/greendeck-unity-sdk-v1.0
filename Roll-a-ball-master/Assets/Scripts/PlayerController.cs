using UnityEngine;
using System.Collections;

using System;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using CleverTap;
using CleverTap.Utilities;

using GreendeckUnity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Net.Security;

public class PlayerController : MonoBehaviour 
{	
	public float speed;
	public GUIText countText;
	public GUIText winText;
	private int count;
	private int numberOfGameObjects;
	Rigidbody rigidbody;

	public float speedAc;

	//accelerometer
	private Vector3 zeroAc;
	private Vector3 curAc;
	private float sensH = 10;
	private float sensV = 10;
	private float smooth = 0.5f;
	private float GetAxisH = 0;
	private float GetAxisV = 0;

	public String CLEVERTAP_ACCOUNT_ID = "TEST-K76-W49-494Z";
	public String CLEVERTAP_ACCOUNT_TOKEN = "TEST-c40-6a3";
	public int CLEVERTAP_DEBUG_LEVEL = 0;
	public bool CLEVERTAP_ENABLE_PERSONALIZATION = true;

	void Awake(){
		#if (UNITY_IPHONE && !UNITY_EDITOR)
		DontDestroyOnLoad(gameObject);
		#endif

		DontDestroyOnLoad(gameObject);

		#if (UNITY_ANDROID && !UNITY_EDITOR)
		DontDestroyOnLoad(gameObject);
		CleverTapBinding.SetDebugLevel(CLEVERTAP_DEBUG_LEVEL);
		CleverTapBinding.Initialize(CLEVERTAP_ACCOUNT_ID, CLEVERTAP_ACCOUNT_TOKEN);
		if (CLEVERTAP_ENABLE_PERSONALIZATION) {
		CleverTapBinding.EnablePersonalization();
		}
		#endif

	}
		
	void Start()
	{
		#if (UNITY_IPHONE && !UNITY_EDITOR)
		// register for push notifications
		CleverTap.CleverTapBinding.RegisterPush();
		// set to 0 to remove icon badge
		CleverTap.CleverTapBinding.SetApplicationIconBadgeNumber(0);
		#endif

		count = 0;
		rigidbody = GetComponent<Rigidbody>();
		SetCountText();
		winText.text = "";
		numberOfGameObjects =14;

		ResetAxes (); 

		String CLIENT_ID = "514c89cf6dd8ff4770d0afcb697640c95ed40cef3527f98002f706952d8b7956";
		String CLIENT_SECRET = "80944fa19ab375b910da940fe91a1ad2e4ca2b1cd6fed2dc31dcaebb4ffb5958";

		String token = GreendeckUnityAPI.GetAccessToken(CLIENT_ID, CLIENT_SECRET);

		print (token);

		print (GreendeckUnityAPI.Track(token, "Start Game Greendeck"));

		// record basic event with no properties
		CleverTapBinding.RecordEvent("Start Game");



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
		
		rigidbody.AddForce (movement * speed * Time.deltaTime);
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

			rigidbody.AddForce(movement * speedAc);

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
			print ("dictionary addition done "+ count);

			CleverTapBinding.RecordEvent("Collision", Props);

			print ("event recorded"+ count);
		}
	}
	
	void SetCountText()
	{
		countText.text = "Count: " + count.ToString();

		print ("SetCountText"+ count);

		if (count >= 14) {

			print ("true PlayerWins"+ count);
			winText.text = "YOU WIN!";

			// record basic event with no properties
			//CleverTapBinding.RecordEvent("Player Wins");

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

