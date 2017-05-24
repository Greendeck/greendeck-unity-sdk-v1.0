#if !UNITY_WEBPLAYER
#define USE_FileIO
#endif


using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

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

	public class GreendeckPerson {
		String personName;
		String personCode;

		public String GetPersonCode(){
			return this.personCode;
		}
		public void SetPersonCode(String personCode){
			this.personCode = personCode;
		}
	}
	
	public static class GreendeckUnityAPI 
	{
		private static GreendeckUnityAPI uniqueInstance;

		static String accessTokenString; 
		static String accessTokenJSON; 
		static GreendeckPerson customer; 
		String clientId; String clientSecret;
		private static GreendeckUnityAPI.FetchResponseListener listener;

		// Assign the listener implementing events interface that will receive the events
		public void setFetchResponseListener(GreendeckUnityAPI.FetchResponseListener l) {
			this.listener = l;
		}
	
		public static String host = "http://greendeck-rails.herokuapp.com";
		public static String AuthEndPoint = host+"/api/v1/oauth/token.json";
		public static String TransactionEndPoint = host+"/api/v1/transactions";
		public static String CustomerApiEndPoint = host+"/api/v1/people";
		public static String EventApiEndPoint = host+"/api/v1/events";
		public static String FetchApiEndPoint = host+"/api/v1/fetch";

		public GreendeckUnityAPI(){
		}

		public interface FetchResponseListener
		{
			// These methods are the different events and
			// need to pass relevant arguments related to the event triggered
			void onSuccess(JSONObject response);
			// or when data has been loaded
			void onFailure(String error);
		}

		public GreendeckUnityAPI(String _clientId, String _clientSecret){
			clientId = _clientId;
			clientSecret = _clientSecret;
			GetAccessToken(clientId, clientSecret, AuthEndPoint);
		}

		public static GreendeckUnityAPI initialize(String _clientId, String _clientSecret) {
			if ((uniqueInstance == null)) {
				try {
					uniqueInstance = new GreendeckUnityAPI(_clientId, _clientSecret);
				}
				catch (Exception e) {
					throw new Exception(e.Message);
				}

			}
			else if (isTokenExpired(uniqueInstance.accessTokenJSON)) {
				uniqueInstance = new GreendeckUnityAPI(_clientId, _clientSecret);
			}

			return uniqueInstance;
		}



		private static bool isTokenExpired(String accessToken) {

			var N = JSON.Parse(accessToken);

			long expiresIn = N["expires_in"].Value; 
			Console.WriteLine("expires_in: " + expiresIn);
			long createdAt = N["created_at"].Value; 
			Console.WriteLine("created at: " + createdAt);

			long currentTime = Time.time;

			Console.WriteLine("current: " + currentTime);
			return ((createdAt + expiresIn) 
				< currentTime);
		}

		public static void GetAccessToken(String CLIENT_ID, String CLIENT_SECRET, String url) 
		{  

			try   
			{  
				string Url = url;


				JSONObject body = new JSONObject();
			   

				body.Add("grant_type", "client_credentials");
				body.Add("client_id", CLIENT_ID);
				body.Add("client_secret", CLIENT_SECRET);
				body.Add("scope", "public read write");


				String response = postHTTP(Url, null, body);

				if(response.Length!=0){

					accessTokenJSON = response;
					Console.WriteLine("TOKEN received");
					if(isTokenExpired(accessTokenJSON)){
						GetAccessToken(CLIENT_ID, CLIENT_SECRET, url);
					}
					else{
						var N = JSON.Parse(response);
						String at = N["access_token"].Value; 
						accessTokenString =  at;
						Console.WriteLine("TOKEN: "+ at);
					}

				}
				else{
					Console.WriteLine("TOKEN response not received");
					return "";
				}

			}   
			catch (WebException e)   
			{  
				return "";
				Console.WriteLine ("TOKEN error on receiving");
			}  
		}

		public static void Identify(){
			
			String guestCode = "guest" + UnityEngine.Random.Range (1, 100000000);
			Identify(guestCode);
		}
		public static void Identify(String identifier){
			Identify (identifier);
		}
		public static void Identify(String identifier, Dictionary<String, object> properties){
			Dictionary<String, String> headers = new Dictionary<String, String>();
			headers.Add("Authorization","Bearer "+accessTokenString);

			string Url =  CustomerApiEndPoint+"/?person_code"+identifier;

			String response = getHTTP (Url, headers);

			var N = JSON.Parse(response);
			String person = N["person"].Value; 

			if (person!=null){
				var N1 = JSON.Parse(person);
				String personCode = N1["person_code"].Value; 
				GreendeckPerson c = new GreendeckPerson ();
				c.SetPersonCode (personCode);
				customer = c;
				Console.WriteLine ("CUSTOMER with identifier: "+identifier+" found");
			}
			else{
				//customer with this identifier not present
				Console.WriteLine("CUSTOMER with identifier: "+identifier+" not present. Creating CUSTOMER now");
				CreateCustomer(identifier, properties);
			}


		}
		public static void CreateCustomer(String identifier){
		
			CreateCustomer (identifier, null);

		}
		public static void CreateCustomer(String identifier, Dictionary<String, object> properties){
			Dictionary<String, String> headers = new Dictionary<String, String>();
			headers.Add("Authorization","Bearer "+accessTokenString);

			string Url =  CustomerApiEndPoint;
			JSONObject body = new JSONObject ();
			if (properties == null)
				body = GetCustomerJSONObject (identifier, properties);
			else
				body = GetCustomerJSONObject (identifier);

			String response = postHTTP (Url, headers, body);

			var N = JSON.Parse(response);
			String person = N["person"].Value; 

			if (person!=null){
				var N1 = JSON.Parse(person);
				String personCode = N1["person_code"].Value; 
				GreendeckPerson c = new GreendeckPerson ();
				c.SetPersonCode (personCode);
				customer = c;
				Console.WriteLine ("CUSTOMER created");
			}
			else{
				Console.WriteLine ("CUSTOMER not created");
				//customer could not be created
			}


		}

		public static void TrackWithoutCustomer(String eventName){
			TrackWithoutCustomer (eventName, null, null);
		}
		public static void TrackWithoutCustomer(String eventName, Dictionary<String, object> properties){
			TrackWithoutCustomer (eventName, properties, null);
		}
		public static void TrackWithoutCustomer(String eventName, Dictionary<String, object> properties, String productCode){
			Dictionary<String, String> headers = new Dictionary<String, String>();
			headers.Add("Authorization","Bearer "+accessTokenString);

			string Url =  EventApiEndPoint;

			JSONObject request = new JSONObject();
			if (properties!=null)
				request = GetEventJSONObject(eventName, null, productCode, properties);
			else
				request = GetEventJSONObject(eventName, null, productCode);

			String response = postHTTP (Url, headers, request);

			Console.WriteLine ("EVENT created: " + response);
		}

		public static void Track(String eventName){
			TrackWithoutCustomer (eventName, null, null);
		}
		public static void Track(String eventName, Dictionary<String, object> properties){
			TrackWithoutCustomer (eventName, properties, null);
		}
		public static void Track(String eventName, Dictionary<String, object> properties, String productCode){
			Dictionary<String, String> headers = new Dictionary<String, String>();
			headers.Add("Authorization","Bearer "+accessTokenString);

			string Url =  EventApiEndPoint;
			String personCode = "";
			if (customer!=null) {
				
				personCode = customer.GetPersonCode;
			}
			else{
				TrackWithoutCustomer(eventName, properties, productCode);
			}


			JSONObject request = new JSONObject();
			if (properties!=null)
				request = GetEventJSONObject(eventName, personCode, productCode, properties);
			else
				request = GetEventJSONObject(eventName, personCode, productCode);

			String response = postHTTP (Url, headers, request);

			Console.WriteLine ("EVENT created: " + response);
		}

		public static void Transact(String transactionCode, float quantity, float price, String productCode){
			Transact (transactionCode, quantity, price, productCode, null);
		}
		public static void Transact(String transactionCode, float quantity, float price, String productCode, Dictionary<String, object> properties){
			Dictionary<String, String> headers = new Dictionary<String, String>();
			headers.Add("Authorization","Bearer "+accessTokenString);

			string Url =  TransactionEndPoint;
			String personCode = "";
			if (customer!=null) {

				personCode = customer.GetPersonCode;


				JSONObject propertiesJSONObject = new JSONObject();
				if (properties != null) {
					foreach(KeyValuePair<string, object> entry in properties){

						String key = ""+entry.Key;
						var value = entry.Value;


						try {
							propertiesJSONObject.Add(key, value);
						} catch (Exception e) {

						}
					}
				}

				JSONObject transactionJSONObject = new JSONObject();
				try {
					transactionJSONObject.Add("transaction_code", transactionCode);
					transactionJSONObject.Add("price", price);
					transactionJSONObject.Add("quantity", quantity);
					transactionJSONObject.Add("properties", propertiesJSONObject);

					if ((personCode==null||personCode.Equals(""))&&(productCode==null||productCode.Equals("")))
					{
						Console.WriteLine ("TRANSACTION error creating request. No person or product found");
					}

					else if ((personCode!=null||!personCode.Equals(""))&&(productCode==null||productCode.Equals("")))
					{
						transactionJSONObject.Add("person_code", personCode);
					}

					else if ((personCode==null||personCode.Equals(""))&&(productCode!=null||!productCode.Equals("")))
					{
						transactionJSONObject.Add("product_code", productCode);
					}

					else if ((personCode!=null||!personCode.Equals(""))&&(productCode!=null||!productCode.Equals("")))
					{
						transactionJSONObject.Add("person_code", personCode);
						transactionJSONObject.Add("product_code", productCode);
					}

				} catch (Exception e) {
					Console.WriteLine ("TRANSACTION error creating request");
				}

				JSONObject request = new JSONObject();
				try {
					request.Add("transaction", transactionJSONObject);
				} catch (Exception e) {

				}

				String response = postHTTP (Url, headers, request);
			}

			else{
				//cannot transact without customer
				Console.WriteLine ("TRANSACTION no customer found");
			}


		}

		public static void Fetch(String productCode){
			Dictionary<String, String> headers = new Dictionary<String, String>();
			headers.Add("Authorization","Bearer "+accessTokenString);

			string Url = FetchApiEndPoint;
			String personCode = "";
			if (customer!=null) {

				personCode = customer.GetPersonCode;

				if(productCode!=null && !productCode.Equals("")){
					String encPersonCode = personCode;
					String encProductCode = productCode;
					try {
						encPersonCode = personCode.Replace(".","%2E");
						encProductCode = productCode.Replace(".","%2E");
					} catch (Exception e) {

					}

					String response = getWithListenerHTTP (Url+"/"+encPersonCode+"/product/"+encProductCode, headers);

				}
				else{
					Console.WriteLine ("FETCH no product_code found");
				}


			}
			else{
				//cannot Fetch without customer
				Console.WriteLine ("FETCH no customer found");
			}

		}

		public static JSONObject GetCustomerJSONObject(String identifier){

			JSONObject customer = new JSONObject();
			JSONObject customerToSend = new JSONObject();
			try {
				customer.Add("person_code", identifier);
			
				customerToSend.Add("person", customer);
			} catch (Exception e) {
				
			}
			return customerToSend;

		}

		public static JSONObject GetCustomerJSONObject(String identifier, Dictionary<String, object> dictionary){

			JSONObject customer = new JSONObject();
			JSONObject customerToSend = new JSONObject();
			try {
				customer.Add("person_code", identifier);

				JSONObject properties = new JSONObject();
				foreach(KeyValuePair<string, object> entry in dictionary){

					String key = ""+entry.Key;
					var value = entry.Value;


					try {
						properties.Add(key, value);
					} catch (Exception e) {
						
					}
				}

				customer.Add("properties", properties);

				customerToSend.Add("person", customer);

			} catch (Exception e) {
				
			}
			return customerToSend;

		}

		public static JSONObject GetEventJSONObject(String eventName, String personCode, String productCode){

			JSONObject eventJSON = new JSONObject();
			JSONObject eventJSONToSend = new JSONObject();
			try {
				eventJSON.Add("event_name", eventName);
				if (productCode==null||productCode.Equals(""))
				{}
				else
				{
					eventJSON.Add("product_code", productCode);
				}
				if (personCode==null||personCode.Equals(""))
				{}
				else
				{
					eventJSON.Add("person_code", personCode);
				}

				eventJSONToSend.Add("event", eventJSON);
			} catch (Exception e) {
				
			}
			return eventJSONToSend;

		}

		public static JSONObject GetEventJSONObject(String eventName, String personCode, String productCode, Dictionary<String, object> dictionary){


			JSONObject eventJSON = new JSONObject();
			JSONObject eventJSONToSend = new JSONObject();
			try {
				eventJSON.Add("event_name", eventName);

				if (productCode==null||productCode.Equals(""))
				{}
				else
				{
					eventJSON.Add("product_code", productCode);
				}
				if (personCode==null||personCode.Equals(""))
				{}
				else
				{
					eventJSON.Add("person_code", personCode);
				}

				JSONObject properties = new JSONObject();
				foreach(KeyValuePair<string, object> entry in dictionary){

					String key = ""+entry.Key;
					var value = entry.Value;


					try {
						properties.Add(key, value);
					} catch (Exception e) {

					}
				}

				eventJSON.Add("properties", properties);

				eventJSONToSend.Add("event", eventJSON);

			} catch (Exception e) {
				
			}
			return eventJSONToSend;
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

		public static void getWithListenerHTTP(String url, Dictionary<String, String> headers){
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
				listener.onSuccess(jsonresponse);
			}   
			catch (WebException e)   
			{  
				listener.onFailure ("Fetch Failed");

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

