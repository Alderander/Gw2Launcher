﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class Account : IDisposable
        {
            public event EventHandler<Account> Exited;

            public Account(Settings.IAccount settings)
            {
                this.Settings = settings;
                this.Process = new LinkedProcess(this);
            }

            public byte isRelaunch;
            public byte inQueueCount;
            public byte errors;

            public WindowWatcher watcher;

            public WindowWatcher Watcher
            {
                get
                {
                    return watcher;
                }
                set
                {
                    if (watcher != value)
                    {
                        if (watcher != null)
                            watcher.Dispose();
                        watcher = value;
                    }
                }
            }

            private FileLocker.SharedFile gfxLock;
            public FileLocker.SharedFile GfxLock
            {
                get
                {
                    return gfxLock;
                }
                set
                {
                    if (gfxLock != value)
                    {
                        if (gfxLock != null)
                            gfxLock.Release();
                        gfxLock = value;
                    }
                }
            }

            public void Dispose()
            {
                if (watcher != null)
                    watcher.Dispose();
            }

            //public bool InUse
            //{
            //    get
            //    {
            //        return InUseCount > 0;
            //    }
            //    set
            //    {
            //        if (value)
            //        {
            //            InUseCount++;
            //        }
            //        else if (InUseCount > 0)
            //            InUseCount--;
            //    }
            //}

            //public byte InUseCount
            //{
            //    get;
            //    set;
            //}

            public Settings.IAccount Settings
            {
                get;
                private set;
            }

            public LinkedProcess Process
            {
                get;
                private set;
            }

            public AccountState State
            {
                get;
                private set;
            }

            public Tools.Icons Icons
            {
                get;
                set;
            }

            public bool IsActive
            {
                get
                {
                    switch (this.State)
                    {
                        case AccountState.Active:
                        case AccountState.ActiveGame:
                        case AccountState.Updating:
                        case AccountState.UpdatingVisible:
                            return true;
                    }
                    return false;
                }
            }

            public void SetState(AccountState state, bool announce, object data)
            {
                if (this.State != state)
                {
                    AccountState previousState = this.State;
                    this.State = state;
                    if (announce && AccountStateChanged != null)
                    {
                        lock (queueAnnounce)
                        {
                            queueAnnounce.Enqueue(new QueuedAnnounce(this.Settings, state, previousState, data));
                            if (taskAnnounce == null || taskAnnounce.IsCompleted)
                            {
                                taskAnnounce = new Task(
                                    delegate
                                    {
                                        DoAnnounce();
                                    });
                                taskAnnounce.Start();
                            }
                        }
                    }
                }
            }

            public void SetState(AccountState state, bool announce)
            {
                SetState(state, announce, null);
            }

            public void OnExited()
            {
                this.GfxLock = null;
                if (Exited != null)
                    Exited(this, this);
            }
        }
    }
}
