using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acrobit.AcroFS
{
    public  class RepositoryNotFoundException : Exception
    {
        public RepositoryNotFoundException() :base(ErrorMessages.REPOSITORY_ROOT_NOT_FOUND)
        {

        }
        
    }
}
