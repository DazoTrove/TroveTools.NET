using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Model;

namespace TroveTools.NET.Framework
{
    class WebClientExtended : WebClient
    {
        public WebClientExtended() : this(new CookieContainer()) { }

        public WebClientExtended(CookieContainer container) : base()
        {
            CookieContainer = container;
        }

        /// <summary>
        /// Gets or sets the value of the Accept HTTP header
        /// </summary>
        /// <returns>
        /// The value of the Accept HTTP header. The default value is null.
        /// </returns>
        public string Accept { get; set; }

        /// <summary>
        /// Gets or sets the value of the Content-type HTTP header.
        /// </summary>
        /// <returns>
        /// The value of the Content-type HTTP header. The default value is null.
        /// </returns>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the time-out value in milliseconds for the System.Net.HttpWebRequest.GetResponse
        /// and System.Net.HttpWebRequest.GetRequestStream methods.   
        /// </summary>
        /// <returns>
        /// The number of milliseconds to wait before the request times out. The default
        /// value is 100,000 milliseconds (100 seconds).
        /// </returns>
        public int Timeout { get; set; } = -1;

        /// <summary>
        /// Gets or sets a value that indicates whether to make a persistent connection to the Internet resource.
        /// </summary>
        /// <returns>
        /// true if the request to the Internet resource should contain a Connection HTTP
        /// header with the value Keep-alive; otherwise, false. The default is true.
        /// </returns>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// Gets or sets the value of the If-Modified-Since HTTP header.
        /// </summary>
        /// <returns>
        /// A System.DateTime that contains the contents of the If-Modified-Since HTTP header.
        /// The default value is the current date and time.
        /// </returns>
        public DateTime IfModifiedSince { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the type of decompression that is used.
        /// </summary>
        /// <returns>
        /// A T:System.Net.DecompressionMethods object that indicates the type of decompression that is used.
        /// </returns>
        public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.None;

        /// <summary>
        /// Gets or sets the cookies associated with the request.
        /// </summary>
        /// <returns>
        /// A System.Net.CookieContainer that contains the cookies associated with this request.
        /// </returns>
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();

        /// <summary>
        /// Gets the URI of the Internet resource that responded to the request.
        /// </summary>
        /// <returns>
        /// A System.Uri that contains the URI of the Internet resource that responded to the request.
        /// </returns>
        public Uri ResponseUri { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            var httpRequest = request as HttpWebRequest;
            if (httpRequest != null)
            {
                httpRequest.Timeout = Timeout;
                httpRequest.CookieContainer = CookieContainer;
                httpRequest.KeepAlive = KeepAlive;
                httpRequest.MaximumAutomaticRedirections = 3;
                httpRequest.UserAgent = string.Format("Mozilla/5.0 (compatible; TroveTools/{0})", ApplicationDetails.GetCurrentVersion());
                if (AutomaticDecompression != DecompressionMethods.None) httpRequest.AutomaticDecompression = AutomaticDecompression;
                if (IfModifiedSince != DateTime.MinValue) httpRequest.IfModifiedSince = IfModifiedSince;
                if (!string.IsNullOrEmpty(Accept)) httpRequest.Accept = Accept;
                if (!string.IsNullOrEmpty(ContentType)) httpRequest.ContentType = ContentType;
            }
            else
            {
                request.Timeout = Timeout;
            }
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ProcessResponse(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ProcessResponse(response);
            return response;
        }

        private void ProcessResponse(WebResponse response)
        {
            var httpResponse = response as HttpWebResponse;
            if (httpResponse != null)
            {
                CookieContainer.Add(httpResponse.Cookies);
                ResponseUri = httpResponse.ResponseUri;
            }
        }
    }
}
