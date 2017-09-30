using System;
using System.Net;
using System.Net.Sockets;

namespace Crawler.Logic
{
    /// <summary>
    /// Use it to download file from the web
    /// </summary>
    internal class FileLoader
    {
        private readonly WebClient _client;

        public FileLoader(WebClient client)
        {
            _client = client;
        }

        public virtual byte[] LoadBytes(string url)
        {
            try
            {
                return _client.DownloadData(url);
            }
            catch (WebException ex)
            {
                if (AllowSkipException(ex))
                    return null;
                throw;
            }
        }

        public virtual string LoadString(string url)
        {
            try
            {
                return _client.DownloadString(url);
            }
            catch (WebException ex)
            {
                if (AllowSkipException(ex))
                    return null;
                throw;
            }
        }

        private bool AllowSkipException(WebException exception)
        {
            var webResp = exception.Response as HttpWebResponse;
            if (webResp != null && webResp.StatusCode == HttpStatusCode.Forbidden)
            {
                //access is forbidden
                //we can log it and continue
                return true;
            }
            if (webResp != null && webResp.StatusCode == HttpStatusCode.NotFound)
            {
                //broken link
                //we can log it and continue
                return true;
            }
            var ex = GetFirstException(exception);
            if (ex is SocketException socketException
                && (socketException.SocketErrorCode == SocketError.AccessDenied ||
                    socketException.SocketErrorCode == SocketError.TimedOut || //www.linkedin.com
                    socketException.SocketErrorCode == SocketError.ConnectionReset)) //ru.linkedin.com
                return true;
            return false;
        }

        private Exception GetFirstException(Exception ex)
        {
            return ex.InnerException != null ? GetFirstException(ex.InnerException) : ex;
        }
    }
}
