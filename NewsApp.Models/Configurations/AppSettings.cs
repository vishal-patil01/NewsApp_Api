namespace NewsApp.Models.Configurations
{
    public class AppSettings
    {
        public static ConfigurationSettings ConfigurationSettings { get; set; }
    }
    public class ConfigurationSettings
    {
        public string NewsServiceBaseUrl { get; set; }
        public int MaxStoriesCount { get; set; }
    }
}
