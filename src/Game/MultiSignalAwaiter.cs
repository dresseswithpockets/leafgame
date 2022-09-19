using System;
using System.Runtime.CompilerServices;
using Godot;

namespace leafgame.Game
{
    public class MultiSignalAwaiter : IAwaiter<object[]>, INotifyCompletion, IAwaitable<object[]>
    {
        private Action _action;

        public void OnCompleted(Action continuation) => _action = continuation;

        public object[] GetResult()
        {
            throw new NotImplementedException();
        }

        public bool IsCompleted { get; }
        public IAwaiter<object[]> GetAwaiter() => this;
    }
}