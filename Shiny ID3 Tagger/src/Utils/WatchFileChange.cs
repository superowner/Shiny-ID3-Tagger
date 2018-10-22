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
        /// <param name="onCreated">Event of file created</param>
        /// <param name="onDeleted">Event of file deleted</param>
        /// <param name="onRenamed">Event of file renamed</param>
        public static void CreateFileWatcher(string path,
                                             FileSystemEventHandler onChanged,
                                             FileSystemEventHandler onCreated,
                                             FileSystemEventHandler onDeleted,
                                             RenamedEventHandler onRenamed)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                // Watch the directory of the path
                Path = Path.GetDirectoryName(path),
                // Watch only for the wanted file
                Filter = Path.GetFileName(path),
                /* Watch for changes in LastAccess and LastWrite times, and
                   the renaming of files or directories. */
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                                        | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            // Add event handlers.
            watcher.Changed += onChanged;
            watcher.Created += onCreated;
            watcher.Deleted += onDeleted;
            watcher.Renamed += onRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

// Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}