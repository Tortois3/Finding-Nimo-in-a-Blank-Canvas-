namespace GameForms.Data
{
    public class UserInfo
    {
        public int PlayerID { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
