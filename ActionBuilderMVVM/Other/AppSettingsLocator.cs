using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionBuilderMVVM.Other
{
    internal class AppSettingsLocator
    {
        private AppSettingsLocator()
        {
        }

        private static AppSettings _instance;

        public static AppSettings Instance => _instance ?? (_instance = new AppSettings());
    }
}
