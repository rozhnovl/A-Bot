// LockExtension
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace ABot
{
	public static class LockExtension
	{
		public static bool IsLocked(this object lockRef)
		{
			bool lockTaken = false;
			try
			{
				Monitor.TryEnter(lockRef, ref lockTaken);
				return !lockTaken;
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(lockRef);
				}
			}
		}

		public static T BranchOnTryEnter<T>(this object lockRef, int waitForLockTimeoutMilli, Func<T> enteredBranch,
			Func<T> enteredNotBranch)
		{
			bool lockTaken = false;
			try
			{
				Monitor.TryEnter(lockRef, waitForLockTimeoutMilli, ref lockTaken);
				if (lockTaken)
				{
					return enteredBranch();
				}

				return enteredNotBranch();
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(lockRef);
				}
			}
		}

		public static T BranchOnTryEnter<T>(this object lockRef, Func<T> enteredBranch, Func<T> enteredNotBranch)
		{
			return BranchOnTryEnter(lockRef, 0, enteredBranch, enteredNotBranch);
		}

		public static void IfLockIsAvailableEnter(this object lockRef, Action action, string lockName = null)
		{
			WhenLockIsAvailableEnter(lockRef, 0, action, lockName);
		}

		private static ConcurrentDictionary<string, Mutex> ResolvedMutexes = new ConcurrentDictionary<string, Mutex>();
		public static void WhenLockIsAvailableEnter(this object lockRef, int waitForLockTimeoutMilli, Action action,
			string lockName = null)
		{
			bool lockTaken = false;
			if (lockName != null)
			{
				Mutex mutex = ResolvedMutexes.GetOrAdd(lockName, (ln) =>
				{
					try
					{
						return Mutex.OpenExisting(lockName);
					}
					catch
					{
						return new Mutex(false, lockName);
					}
				});

				try
				{
					lockTaken = mutex.WaitOne(waitForLockTimeoutMilli);
					if (lockTaken)
						action();
				}
				catch (Exception e)
				{
					Debug.WriteLine("Exception caught in WhenLockIsAvailableEnter: "+ e.ToString());
				}
				finally
				{
					if (lockTaken)
					{
						mutex.ReleaseMutex();
					}
				}

			}
			else
			{

				try
				{
					Monitor.TryEnter(lockRef, waitForLockTimeoutMilli, ref lockTaken);
					if (lockTaken)
					{
						action();
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine("Exception caught in WhenLockIsAvailableEnter: " + e.ToString());
				}
				finally
				{
					if (lockTaken)
					{
						Monitor.Exit(lockRef);
					}
				}
			}
		}

	}
}