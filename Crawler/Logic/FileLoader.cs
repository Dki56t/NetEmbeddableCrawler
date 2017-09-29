using System;
using System.Net;

namespace Crawler.Logic
{
    /// <inheritdoc />
    /// <summary>
    /// Use it to download file from the web
    /// </summary>
    internal class FileLoader : IDisposable
    {
        private readonly WebClient _client;

        public FileLoader()
        {
            _client = new WebClient();
        }

        public byte[] LoadBytes(string url)
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

        public string LoadString(string url)
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

        private bool AllowSkipException(WebException eception)
        {
            var webResp = eception.Response as HttpWebResponse;
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
            return false;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
