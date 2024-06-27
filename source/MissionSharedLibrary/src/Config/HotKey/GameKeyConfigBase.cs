namespace MissionSharedLibrary.Config.HotKey
{
    public abstract class GameKeyConfigBase<T> : MissionConfigBase<T>, IGameKeyConfig where T: GameKeyConfigBase<T>
    {
        public SerializedGameKeyCategory Category { get; set; } = new SerializedGameKeyCategory();

        public string ConfigVersion { get; set; } = "1.0";

        protected override void CopyFrom(T other)
        {
            Category = other.Category;
            ConfigVersion = other.ConfigVersion;
        }

        protected override void UpgradeToCurrentVersion()
        {
        }
    }
}
