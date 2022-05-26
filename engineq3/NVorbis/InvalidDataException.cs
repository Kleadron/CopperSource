using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NVorbis
{
#if XBOX
    class InvalidDataException : Exception
    {
        public InvalidDataException()
            : base()
        {

        }

        public InvalidDataException(string bla)
            : base(bla)
        {

        }
    }
#endif
}
