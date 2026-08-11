namespace Invariable
{
    public abstract class ConfigBase
    {
        public int Id;

        public abstract void Deserialize(ConfigReader configReader);
    }
}