using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace LeadProxy.Clients
{
    public class HttpClient
    {
        public static HttpWebRequest CreateWebRequest( string url )
        {
            HttpWebRequest webRequest = ( HttpWebRequest )WebRequest.Create( url );
            webRequest.AllowAutoRedirect = false;
            return webRequest;
        }

        public static string RequestPost( HttpWebRequest webRequest, string body )
        {
            webRequest.Method = WebRequestMethods.Http.Post;
            return GetResponse( webRequest, StringToBytes( body ) );
        }

        public static string RequestGet( string url )
        {
            var request = CreateWebRequest( url );
            return RequestGet( request );
        }

        public static string RequestCode(HttpWebRequest webRequest)
        {
            webRequest.Method = WebRequestMethods.Http.Post;
            var response = (HttpWebResponse)webRequest.GetResponse();
            
            if (response.StatusCode != HttpStatusCode.Found)
                throw new FormatException("WrongRequest");
            Uri locationURI = new Uri(response.Headers["Location"]);
            var locationParams = HttpUtility.ParseQueryString(locationURI.Query);
            string code = locationParams["Code"];
            
            if (String.IsNullOrEmpty(code))
                throw new FormatException("CodeNotFound");

            return code;
        }

        public static string RequestGet( HttpWebRequest webRequest )
        {
            webRequest.Method = WebRequestMethods.Http.Get;
            return GetResponse( webRequest );
        }

        public static string ParamsToString( NameValueCollection nvc )
        {
            return string.Join( "&", Array.ConvertAll(
                nvc.AllKeys, key => string.Format( "{0}={1}", HttpUtility.UrlEncode( key ), HttpUtility.UrlEncode( nvc[ key ] ) ) ) );
        }

        protected static string GetResponse( HttpWebRequest webRequest, byte[] body = null )
        {
            string output = string.Empty;
            HttpStatusCode code = 0;

            ExecuteWebRequest( webRequest, out code, out output, body );

            return output;
        }

        protected static void FillBody( HttpWebRequest webRequest, byte[] body = null )
        {
            if ( body == null || body.Length == 0 )
            {
                webRequest.ContentLength = 0;
                return;
            }

            webRequest.ContentLength = body.Length;
            using ( Stream dataStream = webRequest.GetRequestStream() )
            {
                dataStream.Write( body, 0, body.Length );
            }
        }

        protected static string GetResponseData( HttpWebResponse response )
        {
            string output = string.Empty;
            using ( Stream data = response.GetResponseStream() )
            {
                if ( data != null )
                {
                    using ( var reader = new StreamReader( data ) )
                    {
                        output = reader.ReadToEnd();
                    }
                }
            }
            return output;
        }

        protected static byte[] StringToBytes( string str )
        {
            if ( str == null )
                return null;

            return Encoding.UTF8.GetBytes( str );
        }

        protected static void ExecuteWebRequest( HttpWebRequest webRequest, out HttpStatusCode code, out string output, byte[] body = null )
        {
            code = 0;
            output = String.Empty;
            HttpWebResponse response = null;

            FillBody( webRequest, body );

            try
            {
                response = ( HttpWebResponse )webRequest.GetResponse();

                code = response.StatusCode;
                output = GetResponseData( response );

                if ( response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent )
                {
                    throw new InfoWebException( string.Format( "Status is {0}, code: {1}.", response.StatusCode, ( int )response.StatusCode ) )
                    {
                        StatusCode = code,
                        ResponseData = output
                    };
                }
            }
            catch ( WebException e )
            {
                if ( e.Response == null )
                    throw;
                response = ( HttpWebResponse )e.Response;
                if ( response == null )
                    throw;

                code = response.StatusCode;
                output = GetResponseData( response );
                throw new InfoWebException( e.Message, e )
                {
                    StatusCode = code,
                    ResponseData = output
                };
            }
            finally
            {
                if ( response != null )
                    response.Close();
            }
        }
    }

    public class InfoWebException : WebException { 
        public InfoWebException() { } 
        public InfoWebException(string message) : base(message) { } 
        public InfoWebException(Exception innerException) : base("", innerException) { } 
        public InfoWebException(string message, Exception innerException) : base(message, innerException) { } 
        public HttpStatusCode StatusCode { get; set; } public string ResponseData { get; set; } 
    }
}
