// Decompiled with JetBrains decompiler
// Type: Bib3.Synchronization.LockExtension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Threading;

namespace Bib3.Synchronization
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
          Monitor.Exit(lockRef);
      }
    }

    public static T BranchOnTryEnter<T>(
      this object lockRef,
      int waitForLockTimeoutMilli,
      Func<T> enteredBranch,
      Func<T> enteredNotBranch)
    {
      bool lockTaken = false;
      try
      {
        Monitor.TryEnter(lockRef, waitForLockTimeoutMilli, ref lockTaken);
        return lockTaken ? enteredBranch() : enteredNotBranch();
      }
      finally
      {
        if (lockTaken)
          Monitor.Exit(lockRef);
      }
    }

    public static T BranchOnTryEnter<T>(
      this object lockRef,
      Func<T> enteredBranch,
      Func<T> enteredNotBranch)
    {
      return lockRef.BranchOnTryEnter<T>(0, enteredBranch, enteredNotBranch);
    }

    public static void IfLockIsAvailableEnter(this object lockRef, Action action) => lockRef.WhenLockIsAvailableEnter(0, action);

    public static void WhenLockIsAvailableEnter(
      this object lockRef,
      int waitForLockTimeoutMilli,
      Action action)
    {
      bool lockTaken = false;
      try
      {
        Monitor.TryEnter(lockRef, waitForLockTimeoutMilli, ref lockTaken);
        if (!lockTaken)
          return;
        action();
      }
      finally
      {
        if (lockTaken)
          Monitor.Exit(lockRef);
      }
    }

    [Obsolete("use IfLockIsAvailableEnter", true)]
    public static void InvokeIfNotLocked(this object lockRef, Action action) => lockRef.IfLockIsAvailableEnter(action);

    [Obsolete("use WhenLockIsAvailableEnter", true)]
    public static void InvokeWhenNotLocked(
      this object lockRef,
      int waitForLockTimeoutMilli,
      Action action)
    {
      lockRef.WhenLockIsAvailableEnter(waitForLockTimeoutMilli, action);
    }

    [Obsolete("InvokeIfNotLocked with lockRef as param[0](this)", true)]
    public static void InvokeIfNotLocked(this Action action, object lockRef) => lockRef.InvokeIfNotLocked(action);

    [Obsolete("EnterWhenNotLocked with lockRef as param[0](this)", true)]
    public static void InvokeWhenNotLocked(
      this Action action,
      object lockRef,
      int waitForLockTimeoutMilli)
    {
      lockRef.InvokeWhenNotLocked(waitForLockTimeoutMilli, action);
    }
  }
}
