using System;
using System.Collections.Generic;
using Proto;

namespace GameLogic.Game
{
    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs() 
        { 
            //
        }
        public ActionLockType Type { set; get; }
        public bool IsLocked { set; get; }
    }

    public class ActionLock
    {
        public int Value
        {
            get
            {
                var v = 0;

                foreach (var i in Locks)
                {
                    if (i.Value > 0)
                    {
                        v += 1 << (int)i.Key;
                    }
                }

                return v;
            }
        }

        private Dictionary<ActionLockType, int> Locks { set; get; }

        public ActionLock()
        {
            Locks = new Dictionary<ActionLockType, int>();
            var values = Enum.GetValues(typeof(ActionLockType));
            foreach (var i in values)
            {
                Locks.Add((ActionLockType)i, 0);
            }
        }

        public bool IsLock(ActionLockType type)
        {
            return Locks[type] > 0;
        }

        public void Lock(ActionLockType type)
        {
            bool isLocked = IsLock(type);
            Locks[type]++;
            if (isLocked != IsLock(type))
            {
                OnStateOnchanged?.Invoke(this, new StateChangedEventArgs { Type = type, IsLocked = IsLock(type) });
            }
        }

        public void Unlock(ActionLockType type)
        {
            bool isLocked = IsLock(type);
            Locks[type]--;
            if (isLocked != IsLock(type))
            {
                OnStateOnchanged?.Invoke(this, new StateChangedEventArgs { Type = type, IsLocked = IsLock(type) });
            }
        }

        //发生变化
        public EventHandler<StateChangedEventArgs> OnStateOnchanged;
    }
}

