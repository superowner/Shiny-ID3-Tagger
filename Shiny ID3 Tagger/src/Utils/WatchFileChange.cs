using System.Windows.Forms;

namespace Utils
{
    using System;
    using System.IO;

    internal partial class Utils
    {
        /// <summary>
        /// Watch for single file
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <param name="onChanged">Event of file changed</param>
        /// <param name="onDeleted">Event of file deleted</param>
        /// <param name="onCreated">Event of file created</param>
        /// <param name="onRenamed">Event of file renamed</param>
        public static FileSystemWatcher CreateFileWatcher(string path,
                                                          FileSystemEventHandler onChanged,
                                                          FileSystemEventHandler onDeleted = null,
                                                          FileSystemEventHandler onCreated = null,
                                                          RenamedEventHandler onRenamed = null)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));

            watcher.NotifyFilter = NotifyFilters.LastWrite;

            // Add event handlers.
            watcher.Changed += onChanged;
            if (onCreated != null)
            {
                watcher.Created += onCreated;
            }

            if (onDeleted != null)
            {
                watcher.Deleted += onDeleted;
            }

            if (onRenamed != null)
            {
                watcher.Renamed += onRenamed;
            }

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            return watcher;
        }
    }
}