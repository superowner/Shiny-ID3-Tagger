// <copyright file="ISetting.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>

namespace Shiny_ID3_Tagger.Setting
{
    /// <summary>
    /// Interface for Setting
    /// </summary>
    /// <example>
    /// Can be used for the application setting
    /// </example>
    public interface ISetting
    {
        /// <summary>
        /// Get Value by key
        /// </summary>
        /// <param name="key">Key of the setting</param>
        /// <returns>Returns the value of the key</returns>
        dynamic GetValue(string key);

        /// <summary>
        /// Set Value
        /// </summary>
        /// <param name="key">The key of setting</param>
        /// <param name="value">The value to be in the key</param>
        /// <returns>The result if the setting was successful </returns>
        bool SetValue(string key, dynamic value);

        /// <summary>
        /// Remove Key
        /// </summary>
        /// <param name="key">Key to remove from the setting</param>
        /// <returns>The result of the remove operation</returns>
        bool RemoveKey(string key);

        /// <summary>
        /// Reset Setting to default
        /// </summary>
        void ResetToDefault();
    }
}