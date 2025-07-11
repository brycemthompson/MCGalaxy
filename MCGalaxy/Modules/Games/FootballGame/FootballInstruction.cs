using MCGalaxy.Bots;
using MCGalaxy.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCGalaxy.Modules.Games.FootballGame {

    public sealed class Metadata {
        public bool reversing;
        public int strength;
        public DateTime lastReflectionTime;
        public Position endPosition;
    }
    sealed class FootballInstruction : BotInstruction {
        FootballGame game;

        public FootballInstruction(FootballGame game) {
            Name = "football";
            this.game = game;
        }

        public override bool Execute(PlayerBot bot, InstructionData data) {
            Metadata meta = (Metadata)data.Metadata;
            game.BallBot = bot;

            GetKicked(bot, meta.strength, meta);

            if (bot.movementSpeed > 0) {
                Step(bot, meta);
                bot.movementSpeed--;
            }

            if (!game.cooldown) {
                if (game.IsInPandasGoal(bot)) {
                    game.HomerTeam.Score++;
                    game.AnnounceGoal(game.HomerTeam.Name);
                    StartCooldown();
                }
                else if (game.IsInHomersGoal(bot)) {
                    game.PandaTeam.Score++;
                    game.AnnounceGoal(game.PandaTeam.Name);
                    StartCooldown();
                }
            }

            return true;
        }

        void StartCooldown() {
            game.cooldown = true;
            Server.MainScheduler.QueueOnce(_task =>
            {
                game.RespawnBall();
                game.cooldown = false;
            }, null, TimeSpan.FromSeconds(3));
        }

        void Step(PlayerBot bot, Metadata meta) {
            bot.movement = true;
            Vec3F32 dir = DirUtils.GetDirVector(bot.Rot.RotY, 0);

            bot.TargetPos.X = bot.Pos.X + (int)(dir.X * bot.movementSpeed);
            bot.TargetPos.Z = bot.Pos.Z + (int)(dir.Z * bot.movementSpeed);

            // Apply reflection when the bot hits a wall, but only if there is no active cooldown
            if (IsNextToSolidBlock(bot)) {
                if ((DateTime.Now - meta.lastReflectionTime).TotalMilliseconds > 500) { // 500ms cooldown
                    meta.reversing = true;
                }
            }
            else {
                meta.reversing = false;
            }

            if (meta.reversing) {
                meta.reversing = false;
                meta.lastReflectionTime = DateTime.Now;
                Vec3F32 reflectedDir = ReflectDirection(bot);

                meta.endPosition = bot.Pos;
                meta.endPosition.X = bot.Pos.X + (int)(reflectedDir.X * (bot.movementSpeed * 10));
                meta.endPosition.Z = bot.Pos.Z + (int)(reflectedDir.Z * (bot.movementSpeed * 10));

                Orientation rot = bot.Rot;
                DirUtils.GetYawPitch(reflectedDir, out rot.RotY, out rot.HeadX);
                bot.Rot = rot;
            }
        }

        Vec3F32 ReflectDirection(PlayerBot bot) {
            Vec3F32 dir = DirUtils.GetDirVector(bot.Rot.RotY, 0);
            Vec3F32 wallNormal = new Vec3F32(0, 0, 0);

            if (IsNextToSolidBlock(bot)) {
                if (bot.level.GetBlock((ushort)(bot.Pos.BlockX + 1), (ushort)bot.Pos.BlockY, (ushort)bot.Pos.BlockZ) != Block.Air)
                    wallNormal = new Vec3F32(-1, 0, 0); // West
                else if (bot.level.GetBlock((ushort)(bot.Pos.BlockX - 1), (ushort)bot.Pos.BlockY, (ushort)bot.Pos.BlockZ) != Block.Air)
                    wallNormal = new Vec3F32(1, 0, 0); // East
                else if (bot.level.GetBlock((ushort)bot.Pos.BlockX, (ushort)bot.Pos.BlockY, (ushort)(bot.Pos.BlockZ + 1)) != Block.Air)
                    wallNormal = new Vec3F32(0, 0, -1); // South
                else if (bot.level.GetBlock((ushort)bot.Pos.BlockX, (ushort)bot.Pos.BlockY, (ushort)(bot.Pos.BlockZ - 1)) != Block.Air)
                    wallNormal = new Vec3F32(0, 0, 1); // North
            }

            float dotProduct = dir.X * wallNormal.X + dir.Y * wallNormal.Y + dir.Z * wallNormal.Z;
            Vec3F32 reflectedDirection = new Vec3F32(
                dir.X - 2 * dotProduct * wallNormal.X,
                dir.Y - 2 * dotProduct * wallNormal.Y,
                dir.Z - 2 * dotProduct * wallNormal.Z
            );
            return reflectedDirection;
        }

        private bool IsNextToSolidBlock(PlayerBot bot) {
            ushort x = (ushort)bot.Pos.BlockX, y = (ushort)bot.Pos.BlockY, z = (ushort)bot.Pos.BlockZ;
            Level lvl = bot.level;

            return
                lvl.GetBlock((ushort)(x - 1), y, z) != Block.Air ||
                lvl.GetBlock((ushort)(x + 1), y, z) != Block.Air ||
                lvl.GetBlock(x, y, (ushort)(z - 1)) != Block.Air ||
                lvl.GetBlock(x, y, (ushort)(z + 1)) != Block.Air;
        }

        void GetKicked(PlayerBot bot, int strength, Metadata meta) {
            int closestDist = int.MaxValue;
            Player[] players = PlayerInfo.Online.Items;
            Player closest = null;

            foreach (Player p in players) {
                if (p.level != bot.level || p.invincible || p.hidden) continue;

                int dx = Math.Abs(p.Pos.X - bot.Pos.X);
                int dy = Math.Abs(p.Pos.Y - bot.Pos.Y);
                int dz = Math.Abs(p.Pos.Z - bot.Pos.Z);
                if (dx > 16 || dy > 16 || dz > 16) continue;

                int dist = dx + dy + dz;
                if (dist < closestDist) {
                    closestDist = dist;
                    closest = p;
                }
            }

            if (closest == null) return;
            bot.SetYawPitch(closest.Rot.RotY, closest.Rot.HeadX);
            bot.movementSpeed = strength;

            Vec3F32 dir = DirUtils.GetDirVector(bot.Rot.RotY, 0);
            meta.endPosition = bot.Pos;
            meta.endPosition.X = bot.Pos.X + (int)(dir.X * (bot.movementSpeed * 10));
            meta.endPosition.Z = bot.Pos.Z + (int)(dir.Z * (bot.movementSpeed * 10));

            Step(bot, meta);
        }

        public override InstructionData Parse(string[] args) {
            InstructionData data = default(InstructionData);
            data.Metadata = new Metadata();
            Metadata meta = (Metadata)data.Metadata;
            meta.strength = 30;
            meta.reversing = false;
            return data;
        }

        public override void Output(Player p, string[] args, TextWriter w) {
            w.WriteLine(Name + " [strength]");
        }

        public override string[] Help { get { return help; } }
        static string[] help = new string[] {
            "&T/BotAI add [name] football <strength>",
            "&HCauses the bot to get kicked around when players touch it.",
            "&H[strength] is how much a power a 'kick' does to the ball",
            "&H  <strength> defaults to 20.",
        };
    }
}
