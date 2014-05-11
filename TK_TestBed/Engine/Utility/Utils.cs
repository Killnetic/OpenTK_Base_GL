using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TK_TestBed.Engine.Utility
{
    public static class Utils
    {
        public static string J(params string[] a)
        {
            return string.Join("\n", a);
        }
    }
}
