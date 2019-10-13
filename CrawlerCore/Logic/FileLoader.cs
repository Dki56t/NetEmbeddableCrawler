using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler.Logic
{
    /// <summary>
    ///     Use it to download file from the web
    /// </summary>
    internal class FileLoader : IFileLoader
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        private static readonly HashSet<Type> ListOfExceptionsItIsAllowedToSuppress = new HashSet<Type>
        {
            typeof(TaskCanceledException),
            typeof(AuthenticationException),
            typeof(SocketException),
            typeof(HttpRequestException)
        };

        private readonly ConcurrentDictionary<string, byte> _processedUrls = new ConcurrentDictionary<string, byte>();

        public virtual async Task<byte[]> LoadBytes(string url)
        {
            return await HandleAllowedExceptions(url,
                    async content => await content.ReadAsByteArrayAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        public virtual async Task<string> LoadString(string url)
        {
            return await HandleAllowedExceptions(url,
                    async content => await content.ReadAsStringAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        private async Task<TResult> HandleAllowedExceptions<TResult>(string url, Func<HttpContent, Task<TResult>> action)
            where TResult : class
        {
            EnsureFirstLoading(url);

            try
            {
                var response = await GetAsync(url).ConfigureAwait(false);
                if (response == null) return null;

                return await action(response.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (AllowSkipException(ex))
                    return null;

                throw;
            }
        }

        private static async Task<HttpResponseMessage> GetAsync(string url)
        {
            using var cts = new CancellationTokenSource(Timeout);
            using var client = new HttpClient();
            var message = await client.GetAsync(url, cts.Token).ConfigureAwait(false);
            var numCode = (int) message.StatusCode;
            return numCode > 299 || numCode < 200 ? null : message;
        }

        [Conditional("DEBUG")]
        private void EnsureFirstLoading(string url)
        {
            if (!_processedUrls.TryAdd(url, 0))
                throw new InvalidOperationException($"Unnecessary content downloading from url: {url}");
        }

        private static bool AllowSkipException(Exception exception)
        {
            if (exception == null)
                return false;

            var type = exception.GetType();
            return ListOfExceptionsItIsAllowedToSuppress.Contains(type) || AllowSkipException(exception.InnerException);
        }
    }
}