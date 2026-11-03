public class FiniteStateMachine
{
    public State CurrentState { get; private set; }

    public void ChangeState(State nextState)
    {
        if (nextState == null || nextState == CurrentState)
            return;

        CurrentState?.Exit();

        CurrentState = nextState;

        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}