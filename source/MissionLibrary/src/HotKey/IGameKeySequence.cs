using TaleWorlds.InputSystem;

namespace MissionLibrary.HotKey
{
    public interface IGameKeySequence
    {
        bool IsKeyDownInOrder(IInputContext input = null);
        bool IsKeyPressedInOrder(IInputContext input = null);
        bool IsKeyReleasedInOrder(IInputContext input = null);
        bool IsKeyDown(IInputContext input = null);
        bool IsKeyPressed(IInputContext input = null);
        bool IsKeyReleased(IInputContext input = null);


        string ToSequenceString();
    }
}
