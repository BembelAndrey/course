using project.Models;

namespace project.Services
{
    // Singleton pattern for managing the current logged-in user
    public class SessionManager
    {
        private static SessionManager _instance;
        private static readonly object _lock = new object();

        public User CurrentUser { get; private set; }

        private SessionManager() { }

        public static SessionManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SessionManager();
                    }
                    return _instance;
                }
            }
        }

        public void Login(User user)
        {
            CurrentUser = user;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public bool IsLoggedIn => CurrentUser != null;
        public bool IsAdmin => IsLoggedIn && CurrentUser.Role == Role.Admin;
    }
}
