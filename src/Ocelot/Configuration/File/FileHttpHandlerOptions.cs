﻿namespace Ocelot.Configuration.File
{
    public class FileHttpHandlerOptions
    {
        public FileHttpHandlerOptions()
        {
            AllowAutoRedirect = false;
            UseCookieContainer = false;
            UseProxy = true;
            PrimaryHandlerName = string.Empty;
        }

        public bool AllowAutoRedirect { get; set; }

        public bool UseCookieContainer { get; set; }

        public bool UseTracing { get; set; }

        public bool UseProxy { get; set; }

        public string PrimaryHandlerName { get; set; }
    }
}
