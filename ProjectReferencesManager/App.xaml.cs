﻿using ProjectReferencesManager.Tools;
using ProjectReferencesManager.Tools.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectReferencesManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            BindingErrorListener.SetTrace();
        }
    }
}
