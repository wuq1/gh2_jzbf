using System;
using System.Reflection;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;

namespace jz
{
    public sealed class jzPluginInfo : Grasshopper2.Framework.Plugin
    {
        static T GetAttribute<T>() where T : Attribute => typeof(jzPluginInfo).Assembly.GetCustomAttribute<T>();

        public jzPluginInfo()
          : base(new Guid("bfeaac89-2ec5-4e4b-ad6e-ea51585a0ae2"),
                 new Nomen(
                    GetAttribute<AssemblyTitleAttribute>()?.Title,
                    GetAttribute<AssemblyDescriptionAttribute>()?.Description),
                 typeof(jzPluginInfo).Assembly.GetName().Version)
        {
            Icon = AbstractIcon.FromResource("jzPlugin", typeof(jzPluginInfo));
        }

        public override string Author => GetAttribute<AssemblyCompanyAttribute>()?.Company;

        public override sealed IIcon Icon { get; }

        public override sealed string Copyright => GetAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? base.Copyright;

        // public override sealed string Website => "https://mywebsite.example.com";

        // public override sealed string Contact => "3203527177@qq.com";

        // public override sealed string LicenceAgreement => "license or URL";

    }
}