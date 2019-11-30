using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler.Logic
{
    /// <summary>
    ///     Use it to download files from the web
    /// </summary>
    internal sealed class FileLoader : IFileLoader, IDisposable
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        private static readonly HashSet<Type> ListOfExceptionsItIsAllowedToSuppress = new HashSet<Type>
        {
            typeof(TaskCanceledException),
            typeof(AuthenticationException),
            typeof(SocketException),
            typeof(HttpRequestException)
        };

        private readonly HttpClient _client;

        private readonly ConcurrentDictionary<Uri, Exception> _failedUris =
            new ConcurrentDictionary<Uri, Exception>(new UriComparer());

        private readonly ConcurrentDictionary<Uri, byte> _processedUris =
            new ConcurrentDictionary<Uri, byte>(new UriComparer());

        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _token;
        private bool _disposed;

        public FileLoader(CancellationToken token)
        {
            _token = token;
            _client = new HttpClient();
            _semaphore = new SemaphoreSlim(32);
        }

        public IDictionary<Uri, Exception> FailedUris
        {
            get { return _failedUris.ToDictionary(p => p.Key, p => p.Value); }
        }

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FileLoader));

            _client.Dispose();
            _semaphore.Dispose();

            _disposed = true;
        }

        public async Task<byte[]?> LoadBytesAsync(Uri uri)
        {
            _token.ThrowIfCancellationRequested();

            return await HandleAllowedExceptionsAsync(uri,
                    async content => await content.ReadAsByteArrayAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        public async Task<string?> LoadStringAsync(Uri uri)
        {
            _token.ThrowIfCancellationRequested();

            return await HandleAllowedExceptionsAsync(uri,
                    async content => await content.ReadAsStringAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        private async Task<TResult?> HandleAllowedExceptionsAsync<TResult>(Uri uri,
            Func<HttpContent, Task<TResult>> action)
            where TResult : class
        {
            EnsureFirstLoading(uri);

            await _semaphore.WaitAsync(_token).ConfigureAwait(false);

            try
            {
                var response = await GetAsync(uri).ConfigureAwait(false);
                if (response == null) return null;

                return await action(response.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!AllowSkipException(ex))
                    throw;

                _failedUris.TryAdd(uri, ex);
                return null;
            }
            finally
            {
                _semaphore.Release(1);
            }
        }

        private async Task<HttpResponseMessage?> GetAsync(Uri uri)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_token);
            cts.CancelAfter(Timeout);

            var message = await _client.GetAsync(uri, cts.Token).ConfigureAwait(false);
            var numCode = (int) message.StatusCode;
            return numCode > 299 || numCode < 200 ? null : message;
        }

        private static bool AllowSkipException(Exception? exception)
        {
            if (exception == null)
                return false;

            var type = exception.GetType();
            return ListOfExceptionsItIsAllowedToSuppress.Contains(type) || AllowSkipException(exception.InnerException);
        }

        [Conditional("DEBUG")]
        private void EnsureFirstLoading(Uri uri)
        {
            if (!_processedUris.TryAdd(uri, 0))
                throw new InvalidOperationException($"Unnecessary content downloading from uri: {uri.OriginalString}");
        }
    }
}