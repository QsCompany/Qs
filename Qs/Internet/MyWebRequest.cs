using System;
using System.IO;
using System.Net;
using System.Text;

namespace Qs.Internet
{
    public class MyWebRequest
    {
        private readonly WebRequest request;
        private Stream dataStream;

        public string Status { get; set; }

        public MyWebRequest(string url)
        {
            // Create a request using a URL that can receive a post.

            request = WebRequest.Create(url);
        }

        public MyWebRequest(string url, string method)
            : this(url)
        {

            if (method.Equals("GET") || method.Equals("POST"))
            {
                // Set the Method property of the request to POST.
                request.Method = method;
            }
            else
            {
                throw new Exception("Invalid Method Type");
            }
        }

        public MyWebRequest(string url, string method, string data)
            : this(url, method)
        {

            // Create POST data and convert it to a byte array.
            var postData = data;
            var byteArray = Encoding.UTF8.GetBytes(postData);

            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";

            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            // Get the request stream.
            dataStream = request.GetRequestStream();

            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);

            // Close the Stream object.
            dataStream.Close();

        }

        public string GetResponse()
        {
            // Get the original response.
            var response = request.GetResponse();

            this.Status = ((HttpWebResponse) response).StatusDescription;

            // Get the stream containing all content returned by the requested server.
            dataStream = response.GetResponseStream();

            // Open the stream using a StreamReader for easy access.
            var reader = new StreamReader(dataStream);

            // Read the content fully up to the end.
            var responseFromServer = reader.ReadToEnd();

            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        public MyWebRequest(string url, string method, StringBuilder data) : this(url, method)
        {

            if (request.Method == "POST")
            {
                // Create POST data and convert it to a byte array.
                var byteArray = Encoding.UTF8.GetBytes(data.ToString());

                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";

                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                dataStream = request.GetRequestStream();

                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);

                // Close the Stream object.
                dataStream.Close();
            }
            else
            {
                var finalUrl = string.Format("{0}{1}", url, "?" + data);
                request = WebRequest.Create(finalUrl);

                var response = request.GetResponse();

                //Now, we read the response (the string), and output it.
                dataStream = response.GetResponseStream();
            }
        }

        public void Test()
        {
            //create the constructor with post type and few data
            var myRequest = new MyWebRequest("http://www.yourdomain.com", "POST", "a=value1&b=value2");
            Console.WriteLine(myRequest.GetResponse());
        }
    }
    

namespace MakeAGETRequest_charp
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		public static void Main(string[] args)
		{
		    const string sURL = "http://www.msdn.com";

		    var webResponse = WebRequest.Create(sURL).GetResponse();
		    var objStream = webResponse.GetResponseStream();

			var objReader = new StreamReader(objStream);

			var sLine = "";
			var i = 0;
		    var sb = new StringBuilder((int) webResponse.ContentLength);
		    while (sLine != null)
		    {
		        i++;
		        sLine = objReader.ReadLine();
		        if (sLine != null)
		        {
		            Console.WriteLine("{0}:{1}", i, sLine);
		            sb.Append(sLine);
		        }
		    }
		    var e = sb.ToString();
		    Console.ReadLine();
		}
	}
}
}