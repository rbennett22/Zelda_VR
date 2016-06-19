using UnityEngine;
using System.Collections.Generic;

using Object = System.Object;

namespace Immersio.Utility
{
    public class PasswordLock
    {
        public delegate void EntryInput_Del(PasswordLock sender, Object entry);
        public EntryInput_Del CorrectEntryCallback, IncorrectEntryCallback;

        public delegate void CorrectPasswordEntered_Del(PasswordLock sender);
        public CorrectPasswordEntered_Del CorrectPasswordEnteredCallback;


        Object[] _password;
        List<Object> _entered = new List<Object>();


        public PasswordLock(Object[] solution)
        {
            _password = solution;
        }


        public bool HasCorrectPasswordBeenEntered()
        {
            if (_entered.Count != _password.Length) { return false; }
            for (int i = 0; i < _password.Length; i++)
            {
                if (_password[i] != _entered[i]) { return false; }
            }
            return true;
        }

        public int PasswordLength { get { return _password.Length; } }
        public Object GetPasswordEntryAt(int index)
        {
            return _password[index];
        }

        public int RemainingEntriesNeeded { get { return _password.Length - _entered.Count; } }


        public void InputNextEntry(Object entry)
        {
            if (HasCorrectPasswordBeenEntered())
            {
                return;
            }

            Object nextRequiredEntry = NextRequiredEntryInSequence;

            _entered.Add(entry);

            if (entry == nextRequiredEntry)
            {
                OnCorrectEntry(entry);
            }
            else
            {
                OnIncorrectEntry(entry);
            }
        }
        Object NextRequiredEntryInSequence
        {
            get
            {
                int i = _entered.Count;
                return (i >= _password.Length) ? null : _password[i];
            }
        }

        void OnCorrectEntry(Object entry)
        {
            if (CorrectEntryCallback != null)
            {
                CorrectEntryCallback(this, entry);
            }

            if (HasCorrectPasswordBeenEntered())
            {
                OnCorrectPasswordEntered();
            }
        }
        void OnCorrectPasswordEntered()
        {
            if (CorrectPasswordEnteredCallback != null)
            {
                CorrectPasswordEnteredCallback(this);
            }
        }

        void OnIncorrectEntry(Object entry)
        {
            if (IncorrectEntryCallback != null)
            {
                IncorrectEntryCallback(this, entry);
            }

            ResetEntered();
        }

        public void ResetEntered()
        {
            _entered.Clear();
        }
    }
}