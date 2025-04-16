namespace medi1.Services
{
    public class UserSession //Session class that keeps track of user login (Only 1 Instance exists)
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

        //Methods Add Login method that retrieves all needed data ?
        public void Clear()
        {
            Id = null;
            UserName = null;
        }
    }
}
