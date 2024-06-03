namespace AnodyneSharp.States
{
    public interface IStateSetter
    {
        public void SetState<T>() where T : State, new();
    }
}

