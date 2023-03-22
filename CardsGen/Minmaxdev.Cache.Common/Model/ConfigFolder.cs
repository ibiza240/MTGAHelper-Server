namespace Minmaxdev.Cache.Common.Model
{
    public interface IConfigFolder
    {
        string Folder { get; }
    }

    public class ConfigFolder : IConfigFolder
    {
        /// <summary>
        /// IMPORTANT!!! Default value is current directory or else Path.Combine crashes with a default null value
        /// </summary>
        public string Folder => ".";
    }

    public class ConfigFolderNone : IConfigFolder
    {
        public string Folder => string.Empty;
    }
}