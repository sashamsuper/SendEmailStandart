using System;
using System.Collections.Generic;

namespace PostalService
{
    public class ProgramBase
    {
        public static void Main()
        {
            GetNewEmail();
            Console.WriteLine("Выполнено");
        }

        public static void GetNewEmail()
        {
            GetSettings getSettings = new()
            {
            };
            GetEmail getEmail = new(getSettings);
            getEmail.SaveNewEmailToBaseAndAttachments();
        }
    }
}