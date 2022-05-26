//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using KSoft.Client.GFX;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Graphics;
//using MH = Microsoft.Xna.Framework.MathHelper;
//using KSoft.Client.Physics;
//using System.IO;

//namespace KSoft.Client.GameObjects.Characters
//{
//    public class Tank : Character
//    {
//        // in degrees
//        public float hullYaw = 0f;
//        public float turretYaw = 0f;
//        public float turretPitch = 0f;
//        public float modelScale = 0.45f;

//        SoundEffect sfxSpawn;
//        SoundEffect sfxDespawn;
//        SoundEffect sfxFire;
//        SoundEffect sfxMotor;

//        bool spawnAnimating = true;
//        float spawnAnimProgress = 0f;
//        float spawnAnimWidthMultiplier = 0f;
//        float spawnAnimHeightMultiplier = 2f;
//        float spawnAnimSpeed = 3f;

//        float lookTargetDistance = 0f;

//        SoundEffectInstance motorSoundInstance;

//        float throttle;
//        float turn;
//        bool trigger;

//        float maxMotorSpeed = 2f;
//        float motorPower = 0f;
//        float maxMotorPitch = -0.5f;
//        float minMotorPitch = -1f;
//        float motorAccel = 2f;  // per second
//        float motorBrake = 3f;

//        float maxTurnSpeed = 180f;
//        float turnPower = 0f;
//        float turnAccel = 4f;
//        float turnBrake = 8f;
//        float maxTurnPitch = 0.5f;

//        bool firedCannon;

//        public struct InputChunk
//        {
//            public float throttle;
//            public float turn;
//            public float turretYaw;

//            public InputChunk(float throttle, float turn, float turretYaw)
//            {
//                this.throttle = throttle;
//                this.turn = turn;
//                this.turretYaw = turretYaw;
//            }
//        }

//        List<InputChunk> inputRecording = new List<InputChunk>();
//        bool recording = false;
//        bool playing = false;
//        int recordIndex = 0;

//        public Tank()
//        {
//            //flags = GameObjectFlags.ShouldDraw | GameObjectFlags.ShouldUpdate;
//            boundingWidth = modelScale*2f;
//            boundingHeight = modelScale*2f;
//        }

//        // keep a list of all tanks active in the level
//        public static new List<Tank> all = new List<Tank>();

//        public override void Load()
//        {
//            base.Load();

//            // ensure the tank model is loaded when this object is loaded
//            TankModel.Load();

//            sfxSpawn = Assets.Load<SoundEffect>("sounds/teleport_in.wav");
//            sfxDespawn = Assets.Load<SoundEffect>("sounds/teleport_out.wav");
//            sfxFire = Assets.Load<SoundEffect>("sounds/weap_mg_light.wav");
//            sfxMotor = Assets.Load<SoundEffect>("sounds/vehicle_motor.wav");

//            // tank spawned
//            //sfxSpawn.Play(0.1f, 0f, 0f);
//            all.Add(this);

//            motorSoundInstance = sfxMotor.CreateInstance();
//            motorSoundInstance.IsLooped = true;
//            motorSoundInstance.Pitch = -1f;
//            motorSoundInstance.Volume = 0.1f;
//            motorSoundInstance.Play();
//        }

//        public override void Unload()
//        {
//            base.Unload();

//            all.Remove(this);
//            if (motorSoundInstance != null)
//            {
//                motorSoundInstance.Stop();
//                motorSoundInstance.Dispose();
//            }
//        }

//        float GetScaledCannonHeight()
//        {
//           return (TankModel.floorOffset + TankModel.cannonOffset) * modelScale;
//        }

//        Vector3 GetCannonDirection()
//        {
//            Vector3 forward = Vector3.UnitZ;
//            return Vector3.Transform(forward, Matrix.CreateRotationY(MH.ToRadians(turretYaw)));
//        }

//        Vector3 lastPosition;

//        bool touchedWall = false;
//        // kinda sucks but I don't really care =]
//        public Vector3 ValidateMove(Vector3 position, Vector3 wishDir, float spacing)
//        {
//            int tileX;
//            int tileY = (int)Math.Floor(position.Y);
//            int tileZ;

//            Vector3 newPosition = position;
//            touchedWall = false;

//            // right
//            tileX = (int)Math.Floor(position.X + wishDir.X + spacing);
//            tileZ = (int)Math.Floor(position.Z + wishDir.Z);
//            if (World.IsSolid(tileX, tileY, tileZ))
//            {
//                //wishDir.X = 0f;
//                newPosition.X = tileX - spacing;
//                touchedWall = true;
//            }

//            // left
//            tileX = (int)Math.Floor(position.X + wishDir.X - spacing);
//            tileZ = (int)Math.Floor(position.Z + wishDir.Z);
//            if (World.IsSolid(tileX, tileY, tileZ))
//            {
//                //wishDir.X = 0f;
//                newPosition.X = tileX + 1f + spacing;
//                touchedWall = true;
//            }

//            // front
//            tileX = (int)Math.Floor(position.X + wishDir.X);
//            tileZ = (int)Math.Floor(position.Z + wishDir.Z + spacing);
//            if (World.IsSolid(tileX, tileY, tileZ))
//            {
//                //wishDir.Z = 0f;
//                newPosition.Z = tileZ - spacing;
//                touchedWall = true;
//            }

//            // back
//            tileX = (int)Math.Floor(position.X + wishDir.X);
//            tileZ = (int)Math.Floor(position.Z + wishDir.Z - spacing);
//            if (World.IsSolid(tileX, tileY, tileZ))
//            {
//                //wishDir.Z = 0f;
//                newPosition.Z = tileZ + 1f + spacing;
//                touchedWall = true;
//            }

//            return newPosition + wishDir;
//        }

//        KeyboardState oldKbs;

//        public void HandleInput(float delta, float total, Camera camera)
//        {
//            //Vector3 forward = Vector3.TransformNormal(Vector3.UnitZ,
//            //    Matrix.CreateRotationY(MathHelper.ToRadians(hullYaw)));
//            //Vector3 right = Vector3.TransformNormal(-Vector3.UnitX,
//            //    Matrix.CreateRotationY(MathHelper.ToRadians(hullYaw)));

//            KeyboardState kbs = Keyboard.GetState();

//            if (kbs.IsKeyDown(Keys.F8) && oldKbs.IsKeyUp(Keys.F8))
//            {
//                FileStream fs = File.Open("demo.bin", FileMode.Create);
//                BinaryWriter bw = new BinaryWriter(fs);

//                bw.Write(inputRecording.Count);

//                for (int i = 0; i < inputRecording.Count; i++)
//                {
//                    bw.Write(inputRecording[i].throttle);
//                    bw.Write(inputRecording[i].turn);
//                    bw.Write(inputRecording[i].turretYaw);
//                }

//                bw.Close();
//                fs.Close();
//            }

//            if (kbs.IsKeyDown(Keys.F7) && oldKbs.IsKeyUp(Keys.F7))
//            {
//                if (File.Exists("demo.bin"))
//                {
//                    FileStream fs = File.Open("demo.bin", FileMode.Open);
//                    BinaryReader br = new BinaryReader(fs);

//                    int chunks = br.ReadInt32();

//                    for (int i = 0; i < chunks; i++)
//                    {
//                        inputRecording.Add(new InputChunk(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
//                    }

//                    br.Close();
//                    fs.Close();
//                }
//            }

//            if (kbs.IsKeyDown(Keys.F6) && oldKbs.IsKeyUp(Keys.F6))
//            {
//                recording = !recording;
//                recordIndex = 0;
//                if (recording)
//                {
//                    inputRecording.Clear();
//                }
//            }

//            if (kbs.IsKeyDown(Keys.F5) && oldKbs.IsKeyUp(Keys.F5))
//            {
//                playing = !playing;
//                recordIndex = 0;
//            }

//            if (kbs.IsKeyDown(Keys.A))
//            {
//                turn = 1f;
//            }
//            else if (kbs.IsKeyDown(Keys.D))
//            {
//                turn = -1f;
//            }
//            else
//            {
//                turn = 0f;
//            }

//            //Vector3 wishDir = Vector3.Zero;

//            if (kbs.IsKeyDown(Keys.W))
//                throttle = 1f;
//            else if (kbs.IsKeyDown(Keys.S))
//                throttle = -1f;
//            else
//                throttle = 0f;

//            MouseState ms = Mouse.GetState();

//            trigger = ms.LeftButton == ButtonState.Pressed;

//            Vector3 nearPoint = new Vector3(ms.X, ms.Y, 0);
//            Vector3 farPoint = new Vector3(ms.X, ms.Y, 1);

//            Viewport viewport = GraphicsDevice.Viewport;
//            nearPoint = viewport.Unproject(nearPoint, camera.proj, camera.view, Matrix.Identity);
//            farPoint = viewport.Unproject(farPoint, camera.proj, camera.view, Matrix.Identity);

//            Vector3 direction = farPoint - nearPoint;
//            direction.Normalize();

//            Ray ray = new Ray(nearPoint, direction);
//            // I hope this is correct
//            Plane plane = new Plane(Vector3.UnitY, -(position.Y + GetScaledCannonHeight()));

//            float? distance = ray.Intersects(plane);
//            if (distance.HasValue)
//            {
//                Vector3 location = ray.Position + (ray.Direction * distance.Value);
//                location.Y -= GetScaledCannonHeight();
//                Vector3 normal = location - position;
//                normal.Normalize();
//                float angle = (float)Math.Atan2(normal.X, normal.Z);
//                turretYaw = MathHelper.ToDegrees(angle);
//                //Matrix transform = Matrix.CreateWorld(playerTank.position, location, Vector3.UnitY);

//                lookTargetDistance = Vector3.Distance(
//                    location, position);

//                if (kbs.IsKeyDown(Keys.Space) && oldKbs.IsKeyUp(Keys.Space))
//                {
//                    World.AddObject(new Tank(), location.X, 0f, location.Z);
//                }

//                if (kbs.IsKeyDown(Keys.V) && oldKbs.IsKeyUp(Keys.V))
//                {
//                    position.X = location.X;
//                    position.Z = location.Z;
//                }

//            }

//            if (recording)
//            {
//                inputRecording.Add(new InputChunk(throttle, turn, turretYaw));
//            }

//            if (playing)
//            {
//                throttle = inputRecording[recordIndex].throttle;
//                turn = inputRecording[recordIndex].turn;
//                turretYaw = inputRecording[recordIndex].turretYaw;
//                recordIndex++;
//                if (recordIndex >= inputRecording.Count)
//                {
//                    playing = false;
//                    recordIndex = 0;
//                }
//            }

//            oldKbs = kbs;
//        }

//        void RunPhysics(float delta, float total)
//        {
//            if (turnPower < turn)
//            {
//                // raise to speed
//                if (turnPower < 0f)
//                {
//                    // brake
//                    turnPower = MH.Clamp(turnPower + turnBrake * delta, -1f, 0f);
//                }
//                else
//                {
//                    // accelerate
//                    turnPower = MH.Clamp(turnPower + turnAccel * delta, 0f, 1f);
//                }
//            }
//            else if (turnPower > turn)
//            {
//                // raise to speed
//                if (turnPower > 0f)
//                {
//                    // brake
//                    turnPower = MH.Clamp(turnPower - turnBrake * delta, 0f, 1f);
//                }
//                else
//                {
//                    // accelerate
//                    turnPower = MH.Clamp(turnPower - turnAccel * delta, -1f, 0f);
//                }
//            }

//            hullYaw += turnPower * maxTurnSpeed * delta;

//            // motor calculation
//            if (motorPower < throttle)
//            {
//                // raise to speed
//                if (motorPower < 0f)
//                {
//                    // brake
//                    motorPower = MH.Clamp(motorPower + motorBrake * delta, -1f, 0f);
//                }
//                else
//                {
//                    // accelerate
//                    motorPower = MH.Clamp(motorPower + motorAccel * delta, 0f, 1f);
//                }
//            }
//            else if (motorPower > throttle)
//            {
//                // raise to speed
//                if (motorPower > 0f)
//                {
//                    // brake
//                    motorPower = MH.Clamp(motorPower - motorBrake * delta, 0f, 1f);
//                }
//                else
//                {
//                    // accelerate
//                    motorPower = MH.Clamp(motorPower - motorAccel * delta, -1f, 0f);
//                }
//            }

//            Vector3 wishDir = new Vector3(0, 0, motorPower);

//            if (wishDir.Length() > 0)
//            {
//                wishDir =
//                    Vector3.Transform(wishDir, Matrix.CreateRotationY(MH.ToRadians(hullYaw)))
//                     * delta * maxMotorSpeed;

//                // grinding against wall
//                if (touchedWall)
//                    wishDir *= 0.75f;
//                position = ValidateMove(position, wishDir, modelScale);
//            }

//            if (motorSoundInstance != null)
//            {
//                float absPower = Math.Abs(motorPower) + Math.Abs(turnPower) * maxTurnPitch;

//                float pitch = MH.Lerp(minMotorPitch, maxMotorPitch, absPower);
//                pitch = MH.Clamp(pitch, -1f, 1f);
//                motorSoundInstance.Pitch = pitch;

//                float volume = MH.Lerp(0.05f, 0.15f, absPower);
//                volume = MH.Clamp(volume, 0f, 1f);
//                motorSoundInstance.Volume = volume;
//            }

//            foreach (Tank tank in all)
//            {
//                if (tank == this)
//                    continue;

//                if (!Intersects(tank))
//                    continue;

//                float distance = Vector3.Distance(position, tank.position);
//                distance -= tank.boundingWidth;
//                distance -= boundingWidth;

//                if (distance < 0)
//                {
//                    distance = Math.Abs(distance);

//                    //if (distance > 0.1f)
//                    //distance = 0.1f;

//                    // might be incorrectly labeled but it works so
//                    Vector3 directionToTank = position - tank.position;
//                    Vector3 directionFromTank = tank.position - position;

//                    directionToTank.Normalize();
//                    directionFromTank.Normalize();

//                    position -= directionFromTank * distance / 2f;
//                    tank.position -= directionToTank * distance / 2f;
//                }
//            }

//            position = ValidateMove(position, Vector3.Zero, modelScale);

//            if (position.X < 0 || position.Z < 0 || position.X > World.Width || position.Z > World.Length)
//            {
//                Console.WriteLine("TANK OUTSIDE OF BOUNDS: " + position.ToString());
//                World.RemoveObject(this);
//            }
//        }

//        public override void Update(float delta, float total)
//        {
//            if (spawnAnimating)
//            {
//                spawnAnimProgress += delta * spawnAnimSpeed;
//                if (spawnAnimProgress > 1f)
//                    spawnAnimating = false;
//            }

//            RunPhysics(delta, total);

//            if (trigger && !firedCannon)
//            {
//                World.LaunchProjectile(this,
//                    new Vector3(position.X, position.Y + GetScaledCannonHeight(), position.Z), 
//                    GetCannonDirection(), 0f, 20f, 0, 1, Color.Yellow);

//                sfxFire.Play(0.25f, 0f, 0f);
//                firedCannon = true;
//            }
//            else if (!trigger && firedCannon)
//            {
//                firedCannon = false;
//            }

//            lastPosition = position;
//        }

//        #region Gizmos

//        void DrawCollisionSquare(Camera camera)
//        {
//            GraphicsDevice.DepthStencilState = DepthStencilState.None;

//            Vector3 p1 = position + new Vector3(-modelScale, TankModel.floorOffset * modelScale, -modelScale);
//            Vector3 p2 = position + new Vector3(modelScale, TankModel.floorOffset * modelScale, -modelScale);
//            Vector3 p3 = position + new Vector3(modelScale, TankModel.floorOffset * modelScale, modelScale);
//            Vector3 p4 = position + new Vector3(-modelScale, TankModel.floorOffset * modelScale, modelScale);

//            DebugDraw.DrawLine(camera, p1, p2, Color.Yellow, Color.LightYellow);
//            DebugDraw.DrawLine(camera, p2, p3, Color.Yellow, Color.LightYellow);
//            DebugDraw.DrawLine(camera, p3, p4, Color.Yellow, Color.LightYellow);
//            DebugDraw.DrawLine(camera, p4, p1, Color.Yellow, Color.LightYellow);

//            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
//        }

//        void DrawDirectionalArrow(Camera camera)
//        {
//            GraphicsDevice.DepthStencilState = DepthStencilState.None;

//            Vector3 arrowP1 = new Vector3(-1.0f, TankModel.floorOffset * modelScale, 0.0f);
//            Vector3 arrowP2 = new Vector3(0.0f, TankModel.floorOffset * modelScale, 0.5f);
//            Vector3 arrowP3 = new Vector3(1.0f, TankModel.floorOffset * modelScale, 0.0f);

//            Matrix arrowWorld = Matrix.CreateScale(0.5f, 1f, 0.5f)
//                * Matrix.CreateTranslation(0, 0, 0.55f) 
//                * Matrix.CreateRotationY(MH.ToRadians(hullYaw)) 
//                * Matrix.CreateTranslation(position);

//            Vector3 p1 = Vector3.Transform(arrowP1, arrowWorld);
//            Vector3 p2 = Vector3.Transform(arrowP2, arrowWorld);
//            Vector3 p3 = Vector3.Transform(arrowP3, arrowWorld);

//            DebugDraw.DrawLine(camera, p1, p2, Color.Green, Color.LightGreen);
//            DebugDraw.DrawLine(camera, p2, p3, Color.Green, Color.LightGreen);
//            DebugDraw.DrawLine(camera, p3, p1, Color.Green, Color.LightGreen);

//            arrowWorld = Matrix.CreateScale(0.5f, 1f, 0.5f)
//                * Matrix.CreateTranslation(0, 0, 0.55f)
//                * Matrix.CreateRotationY(MH.ToRadians(hullYaw + 180f))
//                * Matrix.CreateTranslation(position);

//            // backward arrow
//            p1 = Vector3.Transform(arrowP1, arrowWorld);
//            p2 = Vector3.Transform(arrowP2, arrowWorld);
//            p3 = Vector3.Transform(arrowP3, arrowWorld);

//            DebugDraw.DrawLine(camera, p1, p2, Color.Red, Color.Pink);
//            DebugDraw.DrawLine(camera, p2, p3, Color.Red, Color.Pink);
//            DebugDraw.DrawLine(camera, p3, p1, Color.Red, Color.Pink);

//            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
//        }

//        void DrawCollisionProbe(Camera camera)
//        {
//            GraphicsDevice.DepthStencilState = DepthStencilState.None;

//            Vector3 bulletSpawnPosition = position;
//            bulletSpawnPosition.Y += GetScaledCannonHeight();
//            Vector3 bulletDir =
//                    Vector3.TransformNormal(Vector3.UnitZ,
//                    Matrix.CreateRotationY(MH.ToRadians(turretYaw)));

//            Vector3 rayPos = bulletSpawnPosition;
//            Vector3 rayDir = bulletDir;
//            float lengthLeft = lookTargetDistance;
//            RayResult hit;
//            while (World.Raycast(new Ray(rayPos, rayDir), out hit))
//            {
//                if (hit.distance > lengthLeft)
//                {
//                    DebugDraw.DrawLine(camera, rayPos, rayPos + (rayDir * lengthLeft), Color.Red, Color.Yellow);
//                    break;
//                }

//                DebugDraw.DrawLine(camera, rayPos, rayPos + (rayDir * hit.distance), Color.Blue, Color.LightBlue);
//                // bounce
//                rayPos += rayDir * (hit.distance - 0.01f);
//                rayDir = Vector3.Reflect(rayDir, hit.surfaceNormal);
//                lengthLeft -= hit.distance - 0.01f;
//                //    DebugDraw.DrawLine(camera, hit.hitPosition, hit.hitPosition + (newDirection * (lookTargetDistance - hit.distance)), Color.Blue, Color.LightBlue);
//            }

//            //bulletSpawnPosition += bulletDir * modelScale;

//            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
//        }

//        #endregion Gizmos

//        public override void Draw(Camera camera, float delta, float total)
//        {
//            float modelScaleWidth = modelScale;
//            float modelScaleHeight = modelScale;

//            #region Spawn Animation
//            if (spawnAnimating)
//            {
//                modelScaleWidth = MathHelper.Lerp(spawnAnimWidthMultiplier, modelScale, spawnAnimProgress);
//                modelScaleHeight = MathHelper.Lerp(spawnAnimHeightMultiplier, modelScale, spawnAnimProgress);
//            }
//            #endregion

//            TankModel.Draw(camera, position, hullYaw, turretPitch, turretYaw, modelScaleWidth, modelScaleHeight);
//            TankModel.DrawShadow(camera, position, hullYaw, turretPitch, turretYaw, modelScaleWidth, modelScaleHeight);
            

//            //DrawCollisionSquare(camera);
//            //DrawDirectionalArrow(camera);
//            //DrawCollisionProbe(camera);
            
//        }

//        public override void DrawDebugOverlay(Camera camera, float delta, float total)
//        {
//            //DrawCollisionSquare(camera);
//        }

//        public void DrawPlayerOverlay(Camera camera)
//        {
//            DrawDirectionalArrow(camera);
//            //DrawCollisionProbe(camera);
//        }
//    }
//}
