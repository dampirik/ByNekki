namespace Assets.Scripts
{
    public delegate void ChangeState(GUIState oldState, GUIState newState);

    public enum GUIState
    {
        None = -1,
        Stop = 0,
        Play = 1,
    }
}
