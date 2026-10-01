using System;

namespace bibliotek
{
    public class User
    {
        public int User_ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool Role {  get; set; }
    }
}