using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAsyncUdp
{
    public abstract class Singleton<T> where T : new()
    {
        private static readonly object padlock_ = new object();
        private static T instance_ = default(T);

        public static T Instance
        {
            get
            {
                lock (padlock_)
                {
                    if (instance_ == null)
                    {
                        instance_ = new T();
                    }
                    return instance_;
                }
            }
        }
    }
}
