namespace ActionBuilderMVVM.Other
{
    internal interface IConfigProvider
    {
        string SpritePath { get; set; }
        string ActionPath { get; set; }

        void Save();
        void Load();
    }
}