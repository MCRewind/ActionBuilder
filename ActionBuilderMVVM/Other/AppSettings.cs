using System;
using System.Configuration;
using System.IO;

namespace ActionBuilderMVVM.Other
{
    public class AppSettings : ApplicationSettingsBase
    {
        [UserScopedSetting]
        public string SpritePath
        {
            get => (string) this["SpritePath"];
            set => this["SpritePath"] = value;
        }

        [UserScopedSetting]
        public string ActionPath
        {
            get => (string) this["ActionPath"];
            set => this["ActionPath"] = value;
        }
    }
}