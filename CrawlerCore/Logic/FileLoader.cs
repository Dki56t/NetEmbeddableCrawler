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
                using var client = new HttpClient();
                return await (await client.GetAsync(url).ConfigureAwait(false)).Content.ReadAsByteArrayAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
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
                using var client = new HttpClient();
                return await (await client.GetAsync(url).ConfigureAwait(false)).Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
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
                       socketException.SocketErrorCode == SocketError.TimedOut || // www.linkedin.com
                       socketException.SocketErrorCode == SocketError.ConnectionReset); // www.ru.linkedin.com
        }

        private static bool AllowSkipWebException(WebException webException)
        {
            if (webException == null)
                return false;

            var webResp = webException.Response as HttpWebResponse;
            if (webResp != null && webResp.StatusCode == HttpStatusCode.Forbidden)
                // Access is forbidden.
                return true;

            return webResp != null && webResp.StatusCode == HttpStatusCode.NotFound;
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