using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace GameForms
{
    [DesignerCategory("Code")]
    public sealed class WindowsMediaPlayerHost : AxHost
    {
        private const string WindowsMediaPlayerClsid = "6bf52a52-394a-11d3-b153-00c04f79faa6";
        private dynamic? _player;

        public WindowsMediaPlayerHost() : base(WindowsMediaPlayerClsid)
        {
        }

        private dynamic Player
        {
            get
            {
                _player ??= GetOcx();
                return _player;
            }
        }

        public string URL
        {
            get
            {
                try
                {
                    return Convert.ToString(Player.URL) ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    Player.URL = value ?? string.Empty;
                }
                catch
                {
                }
            }
        }

        public bool AutoStart
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(Player.settings.autoStart);
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    Player.settings.autoStart = value;
                }
                catch
                {
                }
            }
        }

        public bool Loop
        {
            set
            {
                try
                {
                    Player.settings.setMode("loop", value);
                }
                catch
                {
                }
            }
        }

        public bool StretchToFit
        {
            set
            {
                try
                {
                    Player.stretchToFit = value;
                }
                catch
                {
                }
            }
        }

        public string UiMode
        {
            set
            {
                try
                {
                    Player.uiMode = value ?? "none";
                }
                catch
                {
                }
            }
        }

        public void PlayMedia()
        {
            try
            {
                Player.Ctlcontrols.play();
            }
            catch
            {
            }
        }

        public void StopMedia()
        {
            try
            {
                Player.Ctlcontrols.stop();
            }
            catch
            {
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                StopMedia();

            base.Dispose(disposing);
        }
    }
}
