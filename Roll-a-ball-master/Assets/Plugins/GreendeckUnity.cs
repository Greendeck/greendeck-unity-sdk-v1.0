#if !UNITY_WEBPLAYER
#define USE_FileIO
#endif


using UnityEngine;
using System.Collections;


using System;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

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

namespace GreendeckUnity{
	
	public static class GreendeckUnityAPI 
	{

		public static String GetAccessToken(String CLIENT_ID, String CLIENT_SECRET) 
		{  

			try   
			{  
				string Url =   "http://greendeck-rails.herokuapp.com/api/v1/oauth/token.json";


				JSONObject body = new JSONObject();
			   

				body.Add("grant_type", "client_credentials");
				body.Add("client_id", CLIENT_ID);
				body.Add("client_secret", CLIENT_SECRET);
				body.Add("scope", "public read write");


				String response = postHTTP(Url, null, body);

				if(response.Length!=0){

					var N = JSON.Parse(response);

					String versionString = N["access_token"].Value; 

					return versionString;

				}
				else{
					return "";
				}

			}   
			catch (WebException e)   
			{  
				return "";
			}  
		}
		public static String Track(String accessToken, String eventName) {
			if (eventName != null && eventName.Length != 0) {
			
				return Track (accessToken, eventName, null);
			} else {
				return "";
			}
		}
		public static String Track(String accessToken, String eventName, JSONObject properties) 
		{  

			try   
			{  
				string Url =   "http://greendeck-rails.herokuapp.com/api/v1/"+"people/591b349e80b4e70004b37af1/events.json";

				JSONObject body = new JSONObject();

				if(eventName!=null&& eventName.Length!=0){
					JSONObject internalBody = new JSONObject();
					internalBody.Add("event_name", eventName);
					if(properties!=null){
						internalBody.Add("properties", properties);
					}
					body.Add("event", internalBody);
				}
				else{
					return "event name can't be empty";
				}

				Dictionary<String, String> headers = new Dictionary<String, String>();
				headers.Add("Authorization","Bearer "+accessToken);

				String response = postHTTP(Url, headers, body);

				if(response.Length!=0){
					return response;
				}
				else{
					return "";
				}

			}   
			catch (WebException e)   
			{  
				return "";
			}  
		}


		public static String getHTTP(String url, Dictionary<String, String> headers){
			try   
			{  
				string Url =   url;



				HttpWebRequest httpWReq = (HttpWebRequest) WebRequest.Create(Url);  

				Encoding encoding = new UTF8Encoding();  

				//byte[] data = encoding.GetBytes(postData);  

				httpWReq.ProtocolVersion = HttpVersion.Version11;  
				httpWReq.Method = "GET";  
				httpWReq.ContentType = "application/json"; 

				if(headers!=null){

					foreach(KeyValuePair<string, string> entry in headers)
					{
						// do something with entry.Value or entry.Key
						httpWReq.Headers.Add(entry.Key, entry.Value );

					}
				}


				//charset=UTF-8";  
				//httpWReq.Headers.Add("X-Amzn-Type-Version",  
				// "com.amazon.device.messaging.ADMMessage@1.0");  
				//httpWReq.Headers.Add("X-Amzn-Accept-Type",  
				// "com.amazon.device.messaging.ADMSendResult@1.0");  

				//			string _auth = string.Format("{0}:{1}", userName, passWord);  
				//			string _enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(_auth));  
				//			string _cred = string.Format("{0} {1}", "Basic", _enc);  
				//httpWReq.Headers.Add(HttpRequestHeader.Authorization,  
				// "Bearer " + accessToken);  

				//httpWReq.Headers[HttpRequestHeader.Authorization] = _cred;  
				//httpWReq.ContentLength = data.Length;  


				Stream stream = httpWReq.GetRequestStream();  
				//stream.Write(data, 0, data.Length);  
				stream.Close();  

				HttpWebResponse response = (HttpWebResponse) httpWReq.GetResponse();  
				string s = response.ToString();  
				StreamReader reader = new StreamReader(response.GetResponseStream());  
				String jsonresponse = "";  
				String temp = null;  
				while ((temp = reader.ReadLine()) != null) {  
					jsonresponse += temp;  
				}  
				return jsonresponse;  
			}   
			catch (WebException e)   
			{  

				using(WebResponse response = e.Response)   
				{  
					HttpWebResponse httpResponse = (HttpWebResponse) response;  
					Console.WriteLine("Error code: {0}", httpResponse.StatusCode);  
					using(Stream data = response.GetResponseStream())  
					using(var reader = new StreamReader(data))   
					{  
						string text = reader.ReadToEnd();  
						Console.WriteLine(text);  
					}  
				}  
				return "";  
			}

		}


		public static String postHTTP(String url, Dictionary<String, String> headers, JSONObject body){

			try   
			{  
				string Url =   url;



				HttpWebRequest httpWReq = (HttpWebRequest) WebRequest.Create(Url);  

				Encoding encoding = new UTF8Encoding();  

				string postData = body.ToString();

				byte[] data = encoding.GetBytes(postData);  

				httpWReq.ProtocolVersion = HttpVersion.Version11;  
				httpWReq.Method = "POST";  
				httpWReq.ContentType = "application/json"; 

				if(headers!=null){

					foreach(KeyValuePair<string, string> entry in headers)
					{
						// do something with entry.Value or entry.Key
						httpWReq.Headers.Add(entry.Key, entry.Value );

					}
				}

				//charset=UTF-8";  
				//httpWReq.Headers.Add("X-Amzn-Type-Version",  
				// "com.amazon.device.messaging.ADMMessage@1.0");  
				//httpWReq.Headers.Add("X-Amzn-Accept-Type",  
				// "com.amazon.device.messaging.ADMSendResult@1.0");  

				//			string _auth = string.Format("{0}:{1}", userName, passWord);  
				//			string _enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(_auth));  
				//			string _cred = string.Format("{0} {1}", "Basic", _enc);  
				//httpWReq.Headers.Add(HttpRequestHeader.Authorization,  
				// "Bearer " + accessToken);  

				//httpWReq.Headers[HttpRequestHeader.Authorization] = _cred;  
				//httpWReq.ContentLength = data.Length;  


				Stream stream = httpWReq.GetRequestStream();  
				stream.Write(data, 0, data.Length);  
				stream.Close();  

				HttpWebResponse response = (HttpWebResponse) httpWReq.GetResponse();  
				string s = response.ToString();  
				StreamReader reader = new StreamReader(response.GetResponseStream());  
				String jsonresponse = "";  
				String temp = null;  
				while ((temp = reader.ReadLine()) != null) {  
					jsonresponse += temp;  
				}  
				return jsonresponse;  
			}   
			catch (WebException e)   
			{  

				using(WebResponse response = e.Response)   
				{  
					HttpWebResponse httpResponse = (HttpWebResponse) response;  
					Console.WriteLine("Error code: {0}", httpResponse.StatusCode);  
					using(Stream data = response.GetResponseStream())  
					using(var reader = new StreamReader(data))   
					{  
						string text = reader.ReadToEnd();  
						Console.WriteLine(text);  
					}  
				}  
				return "";  
			}

		}


	}




}

