namespace Utils
{
    using System.Text.RegularExpressions;

    internal partial class Utils
    {
        /// <summary>
        /// Regex for file validate
        /// </summary>
        /// <seealso href="https://stackoverflow.com/a/3068130/5923666"/>
        private static Regex fileRegex =
            new Regex(
                @"^(([a-zA-Z]:|\\)\\)?(((\.)|(\.\.)|([^\\/:\*\?""\|<>\. ](([^\\/:\*\?""\|<>\. ])|([^\\/:\*\?""\|<>]*[^\\/:\*\?""\|<>\. ]))?))\\)*[^\\/:\*\?""\|<>\. ](([^\\/:\*\?""\|<>\. ])|([^\\/:\*\?""\|<>]*[^\\/:\*\?""\|<>\. ]))?$");

        public static bool IsValidFilePath(string path)
        {
            return fileRegex.IsMatch(path);
        }
    }
}