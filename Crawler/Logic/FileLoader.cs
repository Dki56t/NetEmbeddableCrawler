﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Crawler.Logic
{
    /// <summary>
    /// Use it to download file from the web
    /// </summary>
    internal class FileLoader
    {
        public virtual async Task<byte[]> LoadBytes(string url)
        {

            try
            {
                return await Task.Run(() =>
                {
                    using (WebClient client = new WebClient())
                        return client.DownloadData(url);
                });
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
                return await Task.Run(() =>
                {
                    using (WebClient client = new WebClient())
                        return client.DownloadString(url);
                });
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
            return ex is SocketException socketException
                   && (socketException.SocketErrorCode == SocketError.AccessDenied ||
                       socketException.SocketErrorCode == SocketError.TimedOut || //www.linkedin.com
                       socketException.SocketErrorCode == SocketError.ConnectionReset); //ru.linkedin.com
        }

        private static Exception GetFirstException(Exception ex)
        {
            while (true)
            {
                if (ex.InnerException == null) return ex;
                ex = ex.InnerException;
            }
        }
    }
}
