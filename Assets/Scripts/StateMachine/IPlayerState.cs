public interface IPlayerState
{
    string Name { get; }
    void Enter();
    void Tick();
    void Exit();
}