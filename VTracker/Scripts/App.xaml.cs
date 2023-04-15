using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using VTracker.ValApi;

namespace VTracker
{
    public partial class App : Application
    {
        public App()
        {
            Timer refreshSession = new Timer(60000);
            refreshSession.AutoReset = true;
            refreshSession.Elapsed += RefreshSession;
            refreshSession.Start();
        }
        private void RefreshSession(object sender, ElapsedEventArgs e)
        {
            ValAPI.Login();
        }
    }
}
