// <copyright file="ISetting.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>

namespace Shiny_ID3_Tagger.Setting
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Interface for Setting
    /// </summary>
    /// <example>
    /// Can be used for the application setting
    /// </example>
    public abstract class BaseSetting
    {
        private System.Configuration.ApplicationSettingsBase val;

        /// <summary>
        /// Default Setting
        /// </summary>
        private JObject defaultSetting = JObject.FromObject(new
        {
            Branch = "develop",
            DebugLevel = 2,
            PreferTags = true,
            RemoveBrackets = true,
            RemoveFeaturing = true,
            OverwriteImage = true,
            AutoSearch = true,
            AutoCapitalize = true,
            ThresholdRedValue = 30,
            MaxImageSize = 500,
            UserAgent = "Shiny ID3 Tagger/1.0 ( shinyid3tagger@gmail.com )",
            FilenamePatterns = new[]
            {
                "^(\\d+\\s)?(-\\s+)?(?<artist>.*?\\w+)\\s+-\\s+(?<title>\\w+.*)$",
                "^(\\d+_)?(?<artist>.*?\\w+)_(?<title>\\w+.*)$"
            },
            CoverPriority = new[]
            {
                "Napster (Rhapsody)",
                "Discogs",
                "Qobuz",
                "Tidal",
                "Genius",
                "7digital",
                "Deezer",
                "iTunes",
                "Last.fm",
                "Spotify",
                "Gracenote (Sony)",
                "Amazon",
                "Musicbrainz",
                "Netease",
                "QQ (Tencent)"
            },
            LyricsPriority = new[]
            {
                "Viewlyrics",
                "Lololyrics",
                "Chartlyrics",
                "Netease",
                "Xiami"
            }
        });

        protected BaseSetting(JObject defaultSetting)
        {
            this.defaultSetting = defaultSetting;
        }

        /// <summary>
        /// Get Value by key
        /// </summary>
        /// <param name="key">Key of the setting</param>
        /// <returns>Returns the value of the key</returns>
        public abstract dynamic GetValue(string key);

        /// <summary>
        /// Set Value
        /// </summary>
        /// <param name="key">The key of setting</param>
        /// <param name="value">The value to be in the key</param>
        /// <returns>The result if the setting was successful </returns>
        public abstract bool SetValue(string key, dynamic value);

        /// <summary>
        /// Remove Key
        /// </summary>
        /// <param name="key">Key to remove from the setting</param>
        /// <returns>The result of the remove operation</returns>
        public abstract bool RemoveKey(string key);

        /// <summary>
        /// Reset Setting to default
        /// </summary>
        public abstract void ResetToDefault();
    }
}