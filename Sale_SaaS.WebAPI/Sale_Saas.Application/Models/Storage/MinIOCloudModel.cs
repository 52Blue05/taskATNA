namespace Sale_Saas.Application.Models.Storage
{
    public class MinIOCloudModel
    {
        public string FolderName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string FilePath { get; set; }
        public string ServerPath { get; set; }
        public string Extension { get; set; }
    }
}
