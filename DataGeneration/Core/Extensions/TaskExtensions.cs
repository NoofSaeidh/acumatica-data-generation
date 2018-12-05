namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            // Create a self-cancelling TaskCompletionSource
            var tcs = new TaskCompletionSourceWithCancellation<T>(cancellationToken);

            // Wait for completion or cancellation
            return await await Task.WhenAny(task, tcs.Task);
        }

        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            // Create a self-cancelling TaskCompletionSource
            var tcs = new TaskCompletionSourceWithCancellation<object>(cancellationToken);

            // Wait for completion or cancellation
            await await Task.WhenAny(task, tcs.Task);
        }

        private class TaskCompletionSourceWithCancellation<TResult> : TaskCompletionSource<TResult>
        {
            public TaskCompletionSourceWithCancellation(CancellationToken cancellationToken)
            {
                CancellationTokenRegistration registration =
                    cancellationToken.Register(() => TrySetCanceled());

                // Remove the registration after the task completes
                Task.ContinueWith(_ => registration.Dispose());
            }
        }
    }
}