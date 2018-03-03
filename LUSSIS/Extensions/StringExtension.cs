﻿using System;

namespace LUSSIS.Extensions
{
    //Authors: Ong Xin Ying
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}