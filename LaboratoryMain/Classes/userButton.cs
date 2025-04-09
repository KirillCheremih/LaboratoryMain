using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryMain.Classes
{
    class userButton
    {
        string buttonContent;
        int[] role;
        public Func<int> func;
        public string Button { get { return buttonContent; } }
        public int[] Role { get { return role; } }

        public userButton(string buttonContent, int[] role, Func<int> func = null)
        {
            
            this.buttonContent = buttonContent;
            this.role = role;
            this.func = func;
        }

        public void execute()
        {
            if (func != null)
            {
                func();
            }
        }
    }
}
