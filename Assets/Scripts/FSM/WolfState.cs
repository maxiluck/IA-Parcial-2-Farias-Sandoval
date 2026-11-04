public abstract class WolfState : State
{
    protected readonly WolfAgent wolf;

    protected WolfState(WolfAgent wolf, FiniteStateMachine stateMachine)
        : base(stateMachine)
    {
        this.wolf = wolf;
    }

    protected bool TryStartChase()
    {
        if (!wolf.Perception.CanSeeTarget) return false;
        stateMachine.ChangeState(wolf.ChaseState);
        return true;
    }

    protected bool ReturnToPatrol()
    {
        if (!wolf.Perception.TargetIsProtected) return false;
        wolf.Communication.ForgetKnownPosition();
        stateMachine.ChangeState(wolf.PatrolState);
        return true;
    }
}
