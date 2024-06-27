
namespace MissionSharedLibrary.Config.HotKey
{
    public interface IGameKeyConfig
    {
        SerializedGameKeyCategory Category { get; set; }
        bool Serialize();
        bool Deserialize();
    }
}
