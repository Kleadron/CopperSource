using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Client.Common
{
    public class RumbleEffect
    {
        // total time the rumble effect lasts for
        public float time;

        // when time is below this value, power gets multiplied from 1 to 0 by how much time is remaining
        public float fade;

        // power to left and right rumble motors
        public float leftPower;
        public float rightPower;

        public RumbleEffect(float time, float fade, float left, float right)
        {
            this.time = time;
            this.fade = fade;
            this.leftPower = left;
            this.rightPower = right;
        }
    }
}
