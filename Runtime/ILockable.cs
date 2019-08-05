using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nebukam.JobAssist
{
    public interface ILockable
    {
        bool locked { get; }
        void Lock();
        void Unlock();
    }
}
