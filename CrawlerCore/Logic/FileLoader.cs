using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Crawler.Logic
{
    /// <summary>
    ///     Use it to download file from the web
    /// </summary>
    internal class FileLoader : IFileLoader
    {
        public virtual async Task<byte[]> LoadBytes(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    return await (await client.GetAsync(url)).Content.ReadAsByteArrayAsync();
                }
            }
            catch (HttpRequestException ex)
            {
                if (AllowSkipException(ex.InnerException as WebException))
                    return null;

                throw;
            }
            catch (WebException ex)
            {
                if (AllowSkipException(ex))
                    return null;

                throw;
            }
        }

        public virtual async Task<string> LoadString(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    return await (await client.GetAsync(url)).Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                if (AllowSkipException(ex))
                    return null;

                throw;
            }
        }

        private static bool AllowSkipException(Exception exception)
        {
            var (rootException, webException) = GetRootAndWebException(exception);

            return AllowSkipWebException(webException) ||
                   rootException is SocketException socketException
                   && (socketException.SocketErrorCode == SocketError.AccessDenied ||
                       socketException.SocketErrorCode == SocketError.TimedOut || //www.linkedin.com
                       socketException.SocketErrorCode == SocketError.ConnectionReset); //www.ru.linkedin.com
        }

        private static bool AllowSkipWebException(WebException webException)
        {
            if (webException == null)
                return false;

            var webResp = webException.Response as HttpWebResponse;
            if (webResp != null && webResp.StatusCode == HttpStatusCode.Forbidden)
                //access is forbidden
                //we can log it and continue
                return true;
            if (webResp != null && webResp.StatusCode == HttpStatusCode.NotFound)
                //broken link
                //we can log it and continue
                return true;

            return false;
        }

        private static (Exception rootException, WebException webException) GetRootAndWebException(Exception ex)
        {
            WebException webException = null;
            Exception rootException = null;

            var current = ex;
            while (current != null)
            {
                if (current is WebException web) webException = web;
                if (current.InnerException == null) rootException = current;

                current = current.InnerException;
            }

            return (rootException, webException);
        }
    }
}