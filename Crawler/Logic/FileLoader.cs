﻿using System;
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

        private readonly ConcurrentDictionary<string, Exception> _failedUrls =
            new ConcurrentDictionary<string, Exception>();

        private readonly ConcurrentDictionary<string, byte> _processedUrls = new ConcurrentDictionary<string, byte>();
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _token;
        private bool _disposed;

        public FileLoader(CancellationToken token)
        {
            _token = token;
            _client = new HttpClient();
            _semaphore = new SemaphoreSlim(32);
        }

        public IDictionary<string, Exception> FailedUrls
        {
            get { return _failedUrls.ToDictionary(p => p.Key, p => p.Value); }
        }

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FileLoader));

            _client.Dispose();
            _semaphore.Dispose();

            _disposed = true;
        }

        public async Task<byte[]> LoadBytesAsync(string url)
        {
            _token.ThrowIfCancellationRequested();

            return await HandleAllowedExceptionsAsync(url,
                    async content => await content.ReadAsByteArrayAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        public async Task<string> LoadStringAsync(string url)
        {
            _token.ThrowIfCancellationRequested();

            return await HandleAllowedExceptionsAsync(url,
                    async content => await content.ReadAsStringAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        private async Task<TResult> HandleAllowedExceptionsAsync<TResult>(string url,
            Func<HttpContent, Task<TResult>> action)
            where TResult : class
        {
            EnsureFirstLoading(url);

            await _semaphore.WaitAsync(_token).ConfigureAwait(false);

            try
            {
                var response = await GetAsync(url).ConfigureAwait(false);
                if (response == null) return null;

                return await action(response.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!AllowSkipException(ex))
                    throw;

                _failedUrls.TryAdd(url, ex);
                return null;
            }
            finally
            {
                _semaphore.Release(1);
            }
        }

        private async Task<HttpResponseMessage> GetAsync(string url)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_token);
            cts.CancelAfter(Timeout);

            var message = await _client.GetAsync(url, cts.Token).ConfigureAwait(false);
            var numCode = (int) message.StatusCode;
            return numCode > 299 || numCode < 200 ? null : message;
        }

        private static bool AllowSkipException(Exception exception)
        {
            if (exception == null)
                return false;

            var type = exception.GetType();
            return ListOfExceptionsItIsAllowedToSuppress.Contains(type) || AllowSkipException(exception.InnerException);
        }

        [Conditional("DEBUG")]
        private void EnsureFirstLoading(string url)
        {
            if (!_processedUrls.TryAdd(url, 0))
                throw new InvalidOperationException($"Unnecessary content downloading from url: {url}");
        }
    }
}