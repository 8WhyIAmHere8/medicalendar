namespace medi1.Services
{
    public class UserSession //Session class that keeps track of user login
    {
        private static UserSession _instance;
        private UserSession() { }
        public static UserSession Instance 
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new UserSession();
                }
                return _instance;
            }
        }
        public string Id { get; set; }
        public string UserName { get; set; }

        public bool IsLoggedIn => !string.IsNullOrEmpty(Id);

        public void Clear()
        {
            Id = null;
            UserName = null;
        }
    }
}
