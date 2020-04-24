namespace ImageAPI.Models
{
    public class ResponseMsg
    {
        public bool IsError { get; set; }
        public int Id { get; set; }
        public string Filename { get; set; }
        public string Filepath { get; set; }
        public string Username { get; set; }
        public string Filesize { get; set; }
        public string Extension { get; set; }
        public string Datetime { get; set; }
        public string ErrorMessage { get; set; }
    }
}