using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Client.GameObjects
{
    public enum StateCommand
    {
        None,

        GameObjectPosition, // Vector3 position

        TankRotation,   // float hullYaw, float turretYaw, float turretPitch
        TankInput,      // float throttle, float turn
    
        TankFire,   // fire a bullet

        //GameObjectUnload,   // should be an ObjectManager function instead, probably
    }
}
