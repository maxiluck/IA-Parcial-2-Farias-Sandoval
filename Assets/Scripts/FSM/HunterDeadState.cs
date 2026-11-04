public class HunterDeadState : State
{
    private HunterAgent hunter;

    public HunterDeadState(HunterAgent hunter, FiniteStateMachine stateMachine)
        : base(stateMachine)
    {
        this.hunter = hunter;
    }

    public override void Enter()
    {
        hunter.PathFollower.StopFollowing();
        hunter.SetTarget(null);
    }
}
