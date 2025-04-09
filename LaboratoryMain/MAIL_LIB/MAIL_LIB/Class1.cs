using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MAIL_LIB
{
    public class Checker
    {
        public static bool mail_check(string mail)
        {
            string check = @"(\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)";
            Regex listener = new Regex(check);

            if (listener.IsMatch(mail))
            {
                return true;
            }

            return false;
        }
        public static bool password_check(string password)
        {
            string check = @"(?=.{8,})(?=(.*\d){1,})(?=(.*\W){1,})(?=.*[a-z])(?=.*[A-Z])";
            Regex listener = new Regex(check);

            if (listener.IsMatch(password))
            {
                return true;
            }

            return false;


        }
        public static bool login_check(string login)
        {
            string check = @"[^а-яА-Я]{6,}";
            Regex listener = new Regex(check);

            if (listener.IsMatch(login))
            {
                return true;
            }

            return false;
        }
    }
}
