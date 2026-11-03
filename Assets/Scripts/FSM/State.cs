public abstract class State
{
    protected FiniteStateMachine stateMachine;

    protected State(FiniteStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }

    public virtual void Update() { }

    public virtual void Exit() { }
}