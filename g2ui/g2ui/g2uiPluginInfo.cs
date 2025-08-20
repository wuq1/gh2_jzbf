using System;
using System.Reflection;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;

namespace g2ui
{
    public sealed class g2uiPluginInfo : Grasshopper2.Framework.Plugin
    {
        static T GetAttribute<T>() where T : Attribute => typeof(g2uiPluginInfo).Assembly.GetCustomAttribute<T>();

        public g2uiPluginInfo()
          : base(new Guid("30fa5c34-031a-46e5-b438-dc17e09a0c64"),
                 new Nomen(
                    GetAttribute<AssemblyTitleAttribute>()?.Title,
                    GetAttribute<AssemblyDescriptionAttribute>()?.Description),
                 typeof(g2uiPluginInfo).Assembly.GetName().Version)
        {
            Icon = AbstractIcon.FromResource("g2uiPlugin", typeof(g2uiPluginInfo));
        }

        public override string Author => GetAttribute<AssemblyCompanyAttribute>()?.Company;

        public override sealed IIcon Icon { get; }

        public override sealed string Copyright => GetAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? base.Copyright;

        // public override sealed string Website => "https://mywebsite.example.com";

        // public override sealed string Contact => "myemail@example.com";

        // public override sealed string LicenceAgreement => "license or URL";

    }
}