using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Prototype
{
    [StructLayout(LayoutKind.Explicit)]
    public partial struct Message
    {
        [FieldOffset(0)]
        public MessageEnum messageEnum;
    }

    public enum MessageEnum : short
    {
        None,
        PickupChest,
        MoneyTransaction
    }
}