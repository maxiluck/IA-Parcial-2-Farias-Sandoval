public abstract class HunterState : State
{
    protected readonly HunterAgent hunter;

    protected HunterState(HunterAgent hunter, FiniteStateMachine stateMachine)
        : base(stateMachine)
    {
        this.hunter = hunter;
    }

    protected bool TryStartFlee()
    {
        if (!hunter.ShouldFlee) return false;
        stateMachine.ChangeState(hunter.FleeState);
        return true;
    }
}
