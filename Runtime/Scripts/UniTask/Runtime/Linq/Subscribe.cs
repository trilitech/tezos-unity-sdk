﻿using System;
using System.Threading;
using Tezos.Cysharp.Threading.Tasks.Internal;
using Subscribes = Tezos.Cysharp.Threading.Tasks.Linq.Subscribe;

namespace Tezos.Cysharp.Threading.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        // OnNext

        public static IDisposable Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource> action)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeCore(source, action, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static IDisposable Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTaskVoid> action)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeCore(source, action, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static IDisposable Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTaskVoid> action)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeCore(source, action, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static void Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource> action, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            Linq.Subscribe.SubscribeCore(source, action, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cancellationToken).Forget();
        }

        public static void Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTaskVoid> action, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            Linq.Subscribe.SubscribeCore(source, action, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cancellationToken).Forget();
        }

        public static void Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTaskVoid> action, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(action, nameof(action));

            Linq.Subscribe.SubscribeCore(source, action, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cancellationToken).Forget();
        }

        public static IDisposable SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> onNext)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeAwaitCore(source, onNext, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static void SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> onNext, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));

            Linq.Subscribe.SubscribeAwaitCore(source, onNext, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cancellationToken).Forget();
        }

        public static IDisposable SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> onNext)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeAwaitCore(source, onNext, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static void SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> onNext, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));

            Linq.Subscribe.SubscribeAwaitCore(source, onNext, Linq.Subscribe.NopError, Linq.Subscribe.NopCompleted, cancellationToken).Forget();
        }

        // OnNext, OnError

        public static IDisposable Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onError, nameof(onError));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeCore(source, onNext, onError, Linq.Subscribe.NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static IDisposable Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTaskVoid> onNext, Action<Exception> onError)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onError, nameof(onError));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeCore(source, onNext, onError, Linq.Subscribe.NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static void Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onError, nameof(onError));

            Linq.Subscribe.SubscribeCore(source, onNext, onError, Linq.Subscribe.NopCompleted, cancellationToken).Forget();
        }

        public static void Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTaskVoid> onNext, Action<Exception> onError, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onError, nameof(onError));

            Linq.Subscribe.SubscribeCore(source, onNext, onError, Linq.Subscribe.NopCompleted, cancellationToken).Forget();
        }

        public static IDisposable SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> onNext, Action<Exception> onError)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onError, nameof(onError));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeAwaitCore(source, onNext, onError, Linq.Subscribe.NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static void SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> onNext, Action<Exception> onError, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onError, nameof(onError));

            Linq.Subscribe.SubscribeAwaitCore(source, onNext, onError, Linq.Subscribe.NopCompleted, cancellationToken).Forget();
        }

        public static IDisposable SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> onNext, Action<Exception> onError)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onError, nameof(onError));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeAwaitCore(source, onNext, onError, Linq.Subscribe.NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static void SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> onNext, Action<Exception> onError, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onError, nameof(onError));

            Linq.Subscribe.SubscribeAwaitCore(source, onNext, onError, Linq.Subscribe.NopCompleted, cancellationToken).Forget();
        }

        // OnNext, OnCompleted

        public static IDisposable Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource> onNext, Action onCompleted)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onCompleted, nameof(onCompleted));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeCore(source, onNext, Linq.Subscribe.NopError, onCompleted, cts.Token).Forget();
            return cts;
        }

        public static IDisposable Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTaskVoid> onNext, Action onCompleted)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onCompleted, nameof(onCompleted));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeCore(source, onNext, Linq.Subscribe.NopError, onCompleted, cts.Token).Forget();
            return cts;
        }

        public static void Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource> onNext, Action onCompleted, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onCompleted, nameof(onCompleted));

            Linq.Subscribe.SubscribeCore(source, onNext, Linq.Subscribe.NopError, onCompleted, cancellationToken).Forget();
        }

        public static void Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTaskVoid> onNext, Action onCompleted, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onCompleted, nameof(onCompleted));

            Linq.Subscribe.SubscribeCore(source, onNext, Linq.Subscribe.NopError, onCompleted, cancellationToken).Forget();
        }

        public static IDisposable SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> onNext, Action onCompleted)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onCompleted, nameof(onCompleted));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeAwaitCore(source, onNext, Linq.Subscribe.NopError, onCompleted, cts.Token).Forget();
            return cts;
        }

        public static void SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> onNext, Action onCompleted, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onCompleted, nameof(onCompleted));

            Linq.Subscribe.SubscribeAwaitCore(source, onNext, Linq.Subscribe.NopError, onCompleted, cancellationToken).Forget();
        }

        public static IDisposable SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> onNext, Action onCompleted)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onCompleted, nameof(onCompleted));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeAwaitCore(source, onNext, Linq.Subscribe.NopError, onCompleted, cts.Token).Forget();
            return cts;
        }

        public static void SubscribeAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> onNext, Action onCompleted, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(onNext, nameof(onNext));
            Error.ThrowArgumentNullException(onCompleted, nameof(onCompleted));

            Linq.Subscribe.SubscribeAwaitCore(source, onNext, Linq.Subscribe.NopError, onCompleted, cancellationToken).Forget();
        }

        // IObserver

        public static IDisposable Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, IObserver<TSource> observer)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(observer, nameof(observer));

            var cts = new CancellationTokenDisposable();
            Linq.Subscribe.SubscribeCore(source, observer, cts.Token).Forget();
            return cts;
        }

        public static void Subscribe<TSource>(this IUniTaskAsyncEnumerable<TSource> source, IObserver<TSource> observer, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(observer, nameof(observer));

            Linq.Subscribe.SubscribeCore(source, observer, cancellationToken).Forget();
        }
    }

    internal sealed class CancellationTokenDisposable : IDisposable
    {
        readonly CancellationTokenSource cts = new CancellationTokenSource();

        public CancellationToken Token => cts.Token;

        public void Dispose()
        {
            if (!cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }
    }

    internal static class Subscribe
    {
        public static readonly Action<Exception> NopError = _ => { };
        public static readonly Action NopCompleted = () => { };

        public static async UniTaskVoid SubscribeCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    try
                    {
                        onNext(e.Current);
                    }
                    catch (Exception ex)
                    {
                        UniTaskScheduler.PublishUnobservedTaskException(ex);
                    }
                }
                onCompleted();
            }
            catch (Exception ex)
            {
                if (onError == NopError)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(ex);
                    return;
                }

                if (ex is OperationCanceledException) return;

                onError(ex);
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        public static async UniTaskVoid SubscribeCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTaskVoid> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    try
                    {
                        onNext(e.Current).Forget();
                    }
                    catch (Exception ex)
                    {
                        UniTaskScheduler.PublishUnobservedTaskException(ex);
                    }
                }
                onCompleted();
            }
            catch (Exception ex)
            {
                if (onError == NopError)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(ex);
                    return;
                }

                if (ex is OperationCanceledException) return;

                onError(ex);
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        public static async UniTaskVoid SubscribeCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTaskVoid> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    try
                    {
                        onNext(e.Current, cancellationToken).Forget();
                    }
                    catch (Exception ex)
                    {
                        UniTaskScheduler.PublishUnobservedTaskException(ex);
                    }
                }
                onCompleted();
            }
            catch (Exception ex)
            {
                if (onError == NopError)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(ex);
                    return;
                }

                if (ex is OperationCanceledException) return;

                onError(ex);
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        public static async UniTaskVoid SubscribeCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, IObserver<TSource> observer, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    try
                    {
                        observer.OnNext(e.Current);
                    }
                    catch (Exception ex)
                    {
                        UniTaskScheduler.PublishUnobservedTaskException(ex);
                    }
                }
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException) return;

                observer.OnError(ex);
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        public static async UniTaskVoid SubscribeAwaitCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    try
                    {
                        await onNext(e.Current);
                    }
                    catch (Exception ex)
                    {
                        UniTaskScheduler.PublishUnobservedTaskException(ex);
                    }
                }
                onCompleted();
            }
            catch (Exception ex)
            {
                if (onError == NopError)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(ex);
                    return;
                }

                if (ex is OperationCanceledException) return;

                onError(ex);
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        public static async UniTaskVoid SubscribeAwaitCore<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    try
                    {
                        await onNext(e.Current, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        UniTaskScheduler.PublishUnobservedTaskException(ex);
                    }
                }
                onCompleted();
            }
            catch (Exception ex)
            {
                if (onError == NopError)
                {
                    UniTaskScheduler.PublishUnobservedTaskException(ex);
                    return;
                }

                if (ex is OperationCanceledException) return;

                onError(ex);
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

    }
}