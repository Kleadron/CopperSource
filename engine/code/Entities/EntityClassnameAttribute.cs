using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CopperSource.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityClassnameAttribute : Attribute
    {
        public string name;

        public EntityClassnameAttribute(string name)
        {
            this.name = name;
        }
    }
}
