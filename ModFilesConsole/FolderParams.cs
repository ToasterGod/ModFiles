namespace ModFilesConsole
{
    internal class FolderParams
    {
        public string SourceRoot { get; set; }
        public string TargetRoot { get; set; }
        public string ModFolderName { get; set; }

        public override string ToString()
        {
            return $"Current values:\n  " +
                $"SourceRoot={SourceRoot}\n  " +
                $"TargetRoot={TargetRoot}\n  " +
                $"ModFolderName={ModFolderName}";
        }

        public override bool Equals(object obj)
        {
            return (((FolderParams)obj).SourceRoot == SourceRoot) &&
                (((FolderParams)obj).TargetRoot == TargetRoot) &&
                (((FolderParams)obj).ModFolderName == ModFolderName);
        }
    }
}