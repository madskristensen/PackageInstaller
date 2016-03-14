using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace PackageInstaller
{
    class Settings : DialogPage
    {
        [Category(Vsix.Name)]
        [DisplayName("Bower arguments")]
        [Description("Additional arguments to pass to the 'bower install <packagename>' command. Example: --save")]
        [DefaultValue("--save")]
        public string BowerArguments { get; set; } = "--save";

        [Category(Vsix.Name)]
        [DisplayName("npm arguments")]
        [Description("Additional arguments to pass to the 'npm install <packagename>' command. Example: --save-dev")]
        [DefaultValue("--save-dev")]
        public string NpmArguments { get; set; } = "--save-dev";

        [Category(Vsix.Name)]
        [DisplayName("JSPM arguments")]
        [Description("Additional arguments to pass to the 'jspm install <packagename>' command. Example: --save-dev")]
        [DefaultValue("")]
        public string JspmArguments { get; set; } = "";

        [Category(Vsix.Name)]
        [DisplayName("TSD arguments")]
        [Description("Additional arguments to pass to the 'tsd install <packagename>' command. Example: -s")]
        [DefaultValue("-s")]
        public string TsdArguments { get; set; } = "-s";
    }
}
