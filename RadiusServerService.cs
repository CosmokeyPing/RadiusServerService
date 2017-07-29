﻿using log4net;
using System;
using System.IO;
using System.Net;
using System.ServiceProcess;

namespace Flexinets.Radius
{
    public partial class RadiusServerService : ServiceBase
    {
        private RadiusServer _rsauth;
        private RadiusServer _rsacct;
        private readonly ILog _log = LogManager.GetLogger(typeof(RadiusServerService));


        public RadiusServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            try
            {
                _log.Info("Reading configuration");
                var path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\dictionary";
                var dictionary = new RadiusDictionary(path);
                _log.Info("Configuration read");

                _rsauth = new RadiusServer(new IPEndPoint(IPAddress.Any, 1812), dictionary, RadiusServerType.Authentication);
                _rsacct = new RadiusServer(new IPEndPoint(IPAddress.Any, 1813), dictionary, RadiusServerType.Accounting);

                var packetHandler = new MockPacketHandler();
                _rsauth.AddPacketHandler(IPAddress.Parse("127.0.0.1"), "secret", packetHandler);
                _rsacct.AddPacketHandler(IPAddress.Parse("127.0.0.1"), "secret", packetHandler);

                _rsauth.Start();
                _rsacct.Start();
            }
            catch (Exception ex)
            {
                _log.Fatal("Failed to start service", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            _rsauth?.Stop();
            _rsauth?.Dispose();
            _rsacct?.Stop();
            _rsacct?.Dispose();
        }
    }
}
