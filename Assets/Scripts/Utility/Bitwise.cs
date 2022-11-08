using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SampleCode
{
    public static class Bitwise
    {
        public static bool Check(int a_, int b_)
        {
            return (a_ & b_) > 0; 
        }

        public static bool Equals(int a_, int b_)
        {
            return a_ == b_;
        }

        public static bool Contains(int a_, int b_)
        {
            return (a_ & b_) == b_;
        }

        public static int And(int a_, int b_)
        {
            return a_ & b_;
        }

        public static int Or(int a_, int b_)
        {
            return a_ | b_;
        }

        public static int Add(int a_, int b_)
        {
            return a_ |= b_;
        }

        public static void AddRef(ref int a_, int b_)
        {
            a_ |= b_;
        }

        public static int Sub(int a_, int b_)
        {
            return a_ &= (~b_);
        }

        public static void SubRef(ref int a_, int b_)
        {
            a_ &= (~b_);
        }
    }
}
