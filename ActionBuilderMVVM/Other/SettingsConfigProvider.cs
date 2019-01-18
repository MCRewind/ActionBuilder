namespace ActionBuilderMVVM.Other
{
    internal class SettingsConfigProvider : IConfigProvider
    {
        public string SpritePath { get; set; }
        public string ActionPath { get; set; }

        public void Load()
        {
            var settings = AppSettingsLocator.Instance;


            SpritePath = settings.SpritePath;
            ActionPath = settings.ActionPath;
        }

        public void Save()
        {
            var settings = AppSettingsLocator.Instance;

            settings.SpritePath = SpritePath;
            settings.ActionPath = ActionPath;

            settings.Save();
        }
    }
}
