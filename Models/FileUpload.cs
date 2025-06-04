namespace sendbol_videoshop.Server.Models
{
    public class FileUpload
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required byte[] Content { get; set; }
    }
}