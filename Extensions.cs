namespace backup_manager
{
    public static class Extensions
    {
        public static Task<bool> WaitOneAsync(this WaitHandle waitHandle, int timeoutMs)
        {
            if (waitHandle == null)
                throw new ArgumentNullException(nameof(waitHandle));

            var tcs = new TaskCompletionSource<bool>();

            RegisteredWaitHandle registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                callBack: (state, timedOut) => { tcs.TrySetResult(!timedOut); },
                state: null,
                millisecondsTimeOutInterval: timeoutMs,
                executeOnlyOnce: true);

            return tcs.Task.ContinueWith((antecedent) =>
            {
                registeredWaitHandle.Unregister(waitObject: null);
                try
                {
                    return antecedent.Result;
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}