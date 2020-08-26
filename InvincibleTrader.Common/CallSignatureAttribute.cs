using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTrader.Common
{
    public class CallSignatureAttribute: Attribute
    {
        public CallSignatureAttribute(params Type[] argumentTypes)
        {

        }
    }
}
