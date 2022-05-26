//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;
//using KSoft.Client.Common;

//namespace KSoft.Client.GFX
//{
//    public static class TankModel
//    {
//        static bool initialized = false;

//        static GraphicsMesh meshHull;
//        static GraphicsMesh meshTurret;
//        static GraphicsMesh meshCannon;

//        public static readonly float hullOffset = 0f;
//        public static readonly float turretOffset = 0.65f;
//        public static readonly float cannonOffset = 0.65f;
//        public static readonly float floorOffset = 0.25f;

//        public static void Load()
//        {
//            if (initialized)
//                return;
            
//            GraphicsMeshBuilder mb = Engine.I.modelBuilder;
//            ModelData md = Engine.I.assets.Load<ModelData>("models/tank.obj");//new ModelData("Content/models/tank.obj");
//            mb.SetDataSource(md);

//            // Hull
//            /*
//             * hull_frontpanel
//             * hull_main
//             * hull_turret_ring
//             * hull_exhaust
//             * tread_left
//             * tread_right
//             */
//            Matrix hullTransform = Matrix.CreateTranslation(0, -hullOffset, 0);
//            mb.SetAddTransform(hullTransform);
//            mb.AddObject("hull_frontpanel");
//            mb.AddObject("hull_main");
//            mb.AddObject("hull_turret_ring");
//            mb.AddObject("hull_exhaust");
//            mb.AddObject("tread_left");
//            mb.AddObject("tread_right");
//            meshHull = mb.Build();

//            // Turret
//            /*
//             * turret_antenna
//             * turret_main
//             */
//            Matrix turretTransform = Matrix.CreateTranslation(0, -turretOffset, 0);
//            mb.SetAddTransform(turretTransform);
//            mb.AddObject("turret_antenna");
//            mb.AddObject("turret_main");
//            meshTurret = mb.Build();

//            // Cannon
//            /*
//             * cannon_hinge
//             * cannon_main
//             */
//            Matrix cannonTransform = Matrix.CreateTranslation(0, -cannonOffset, 0);
//            mb.SetAddTransform(cannonTransform);
//            mb.AddObject("cannon_hinge");
//            mb.AddObject("cannon_main");
//            meshCannon = mb.Build();

//            ModelCache.Put(ModelNames.TankHull, meshHull); 
//            ModelCache.Put(ModelNames.TankTurret, meshTurret);
//            ModelCache.Put(ModelNames.TankCannon, meshCannon);

//            initialized = true;
//        }

//        public static void Unload()
//        {
//            if (!initialized)
//                return;

//            initialized = false;
//        }

//        public static void Draw(Camera camera, Vector3 position, float hullYaw, float turretPitch, float turretYaw, float scaleXZ, float scaleY)
//        {
//            // apply last
//            Matrix translation = Matrix.CreateTranslation(0, floorOffset, 0) 
//                * Matrix.CreateScale(scaleXZ, scaleY, scaleXZ) 
//                * Matrix.CreateTranslation(position.X, position.Y, position.Z);

//            // hull only rotates Y
//            Matrix hullTransform = Matrix.CreateRotationY(MathHelper.ToRadians(hullYaw)) // rotate Y
//                * Matrix.CreateTranslation(0, hullOffset, 0);                            // offset

//            // turret only rotates Y
//            Matrix turretTransform = Matrix.CreateRotationY(MathHelper.ToRadians(turretYaw)) // rotate Y
//                * Matrix.CreateTranslation(0, turretOffset, 0);                              // offset

//            // cannon rotates X then Y
//            Matrix cannonTransform = Matrix.CreateRotationX(MathHelper.ToRadians(turretPitch)) // rotate X
//                * Matrix.CreateRotationY(MathHelper.ToRadians(turretYaw))                      // rotate Y
//                * Matrix.CreateTranslation(0, cannonOffset, 0);                                // offset

//            meshHull.Draw(hullTransform * translation, camera);
//            meshTurret.Draw(turretTransform * translation, camera);
//            meshCannon.Draw(cannonTransform * translation, camera);
//        }

//        public static void DrawShadow(Camera camera, Vector3 position, float hullYaw, float turretPitch, float turretYaw, float scaleXZ, float scaleY)
//        {
//            // apply last
//            Matrix translation = Matrix.CreateTranslation(0, floorOffset, 0)
//                * Matrix.CreateScale(scaleXZ, scaleY, scaleXZ)
//                * Matrix.CreateTranslation(position.X, position.Y, position.Z);

//            // hull only rotates Y
//            Matrix hullTransform = Matrix.CreateRotationY(MathHelper.ToRadians(hullYaw)) // rotate Y
//                * Matrix.CreateTranslation(0, hullOffset, 0);                            // offset

//            // turret only rotates Y
//            Matrix turretTransform = Matrix.CreateRotationY(MathHelper.ToRadians(turretYaw)) // rotate Y
//                * Matrix.CreateTranslation(0, turretOffset, 0);                              // offset

//            // cannon rotates X then Y
//            Matrix cannonTransform = Matrix.CreateRotationX(MathHelper.ToRadians(turretPitch)) // rotate X
//                * Matrix.CreateRotationY(MathHelper.ToRadians(turretYaw))                      // rotate Y
//                * Matrix.CreateTranslation(0, cannonOffset, 0);                                // offset

//            Matrix shadow = Matrix.CreateShadow(
//                Lighting.LightSourceDirection, 
//                new Plane(Vector3.UnitY, 0));

//            meshHull.DrawShadow((hullTransform * translation) * shadow, camera);
//            meshTurret.DrawShadow((turretTransform * translation) * shadow, camera);
//            meshCannon.DrawShadow((cannonTransform * translation) * shadow, camera);
//        }
//    }
//}
