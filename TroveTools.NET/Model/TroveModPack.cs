using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace TroveTools.NET.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    class TroveModPack
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string IdUriFormat = "trove://modpack={0}";
        public const string IdUriRegex = @"trove:[/\\]{0,2}modpack=(?<PackId>\d+)";
        public const string AdHocUriFormat = "trove://{0}?{1}";
        public const string AdHocUriRegex = @"trove:[/\\]{0,2}(?<Name>[^?]+?)/?\?(?<Mods>[0-9&]+)";
        public const string LocalSource = "Local";

        #region Constructors
        public TroveModPack() { }

        [JsonConstructor]
        public TroveModPack(string TroveUri)
        {
            this.TroveUri = TroveUri;
        }
        #endregion

        #region Public Properties
        public string PackId { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Source { get; set; } = LocalSource;

        public List<TroveMod> Mods { get; set; } = new List<TroveMod>();
        #endregion

        #region Derived Public Property
        [JsonProperty]
        public string TroveUri
        {
            get
            {
                if (string.IsNullOrEmpty(PackId))
                    return string.Format(AdHocUriFormat, WebUtility.UrlEncode(Name), string.Join("&", Mods.Select(m => m.Id)));
                else
                    return string.Format(IdUriFormat, PackId);
            }
            set
            {
                // Setting the Trove URI loads the pack and mod details from the Trovesaurus API
                Match matchId = Regex.Match(value, IdUriRegex, RegexOptions.IgnoreCase);
                if (matchId.Success)
                {
                    PackId = matchId.Groups["PackId"].Value;
                    TroveModPack pack = TrovesaurusApi.GetModPack(PackId);
                    if (pack != null)
                    {
                        // Copy pack details from Trovesaurus API mod pack
                        Url = pack.Url;
                        Name = pack.Name;
                        Author = pack.Author;
                        Source = pack.Source;

                        Mods.Clear();
                        Mods.AddRange(pack.Mods);
                    }
                    else log.ErrorFormat("Error setting mod pack URI to [{0}]: pack ID [{1}] not found", value, PackId);
                    return;
                }
                Match matchAdHoc = Regex.Match(value, AdHocUriRegex, RegexOptions.IgnoreCase);
                if (matchAdHoc.Success)
                {
                    Name = WebUtility.UrlDecode(matchAdHoc.Groups["Name"].Value);
                    Source = LocalSource;

                    Mods.Clear();
                    foreach (var modId in matchAdHoc.Groups["Mods"].Value.Split('&'))
                    {
                        var mod = TroveMod.GetMod(modId);
                        if (mod != null)
                            Mods.Add(mod);
                        else
                            log.ErrorFormat("Mod ID [{0}] not found while setting mod pack URI to [{1}]", modId, value);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Copies the mod pack installation URI to the clipboard
        /// </summary>
        public void CopyModPackUri()
        {
            string uri = TroveUri;
            Clipboard.SetText(uri);
            log.InfoFormat("Copied mod pack installation URI for {0} to clipboard: {1}", Name, uri);
        }
    }
}
