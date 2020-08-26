using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTrader.Common
{
    public class ReturnSignatureAttribute: Attribute
    {
        public ReturnSignatureAttribute(params Type[] argumentTypes)
        {

        }
    }
}
