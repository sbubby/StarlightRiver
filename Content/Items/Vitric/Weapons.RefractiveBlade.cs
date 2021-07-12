using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;

namespace StarlightRiver.Content.Items.Vitric
{
    public class RefractiveBlade : ModItem
    {
        public int combo;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override bool AltFunctionUse(Player player) => true;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Refractive Blade");
            Tooltip.SetDefault("Swing in any direction \nHold down to launch a laser");
        }

        public override void SetDefaults()
        {
            item.damage = 56;
            item.width = 60;
            item.height = 60;
            item.useTime = 30;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.melee = true;
            item.noMelee = true;
            item.knockBack = 7;
            item.useTurn = false;
            item.value = Item.sellPrice(0, 2, 20, 0);
            item.rare = ItemRarityID.Orange;
            item.shoot = ProjectileType<RefractiveBladeProj>();
            item.shootSpeed = 0.1f;
            item.noUseGraphic = true;
        }

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
            if(player.altFunctionUse == 2)
			{
                Projectile.NewProjectile(position, new Vector2(speedX, speedY), ProjectileType<RefractiveBladeLaser>(), damage, knockBack, player.whoAmI);

                return false;
			}

            Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, 0, combo);
            combo++;

            if (combo > 1)
                combo = 0;

            return false;
		}
	}

    public class RefractiveBladeProj : ModProjectile, IDrawPrimitive
    {
        int direction = 0;
        float maxTime = 0;
        float maxAngle = 0;

		private List<Vector2> cache;
        private Trail trail;

        public override string Texture => AssetDirectory.VitricItem + "RefractiveBlade";

        public ref float StoredAngle => ref projectile.ai[0];
        public ref float Combo => ref projectile.ai[1];

        public float Timer => 300 - projectile.timeLeft;
        public Player Owner => Main.player[projectile.owner];
        public float SinProgress => (float)Math.Sin((1 - Timer / maxTime) * 3.14f);

        public sealed override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.melee = true;
            projectile.width = projectile.height = 2;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.alpha = 255;

            projectile.timeLeft = 300;
        }

		public override void AI()
		{
            if (Timer == 0)
            {
                StoredAngle = projectile.velocity.ToRotation();
                projectile.velocity *= 0;

                Helper.PlayPitched("Effects/FancySwoosh", 1, Combo);

                switch(Combo)
				{
                    case 0:
                        direction = 1;
                        maxTime = 30;
                        maxAngle = 4;
                        break;
                    case 1:
                        direction = -1;
                        maxTime = 15;
                        maxAngle = 2;
                        break;
				}
            }

            float targetAngle = StoredAngle + (-(maxAngle / 2) + Helper.BezierEase(Timer / maxTime) * maxAngle) * Owner.direction * direction;

            projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(targetAngle) * (70 + (float)Math.Sin(Helper.BezierEase(Timer / maxTime) * 3.14f) * 40);
            projectile.rotation = targetAngle + 1.57f * 0.5f;

            ManageCaches();
            ManageTrail();

            var color = new Color(255, 140 + (int)(40 * SinProgress), 105);

            Lighting.AddLight(projectile.Center, color.ToVector3() * SinProgress);

            if(Main.rand.Next(2) == 0)
                Dust.NewDustPerfect(projectile.Center, DustType<Glow>(), Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.2f);

            if (Timer >= maxTime)
                projectile.timeLeft = 0;
		}

		public override bool? CanHitNPC(NPC target)
		{
            
            if (target.active && target.immune[0] <= 0 && !target.friendly && Helper.CheckLinearCollision(Owner.Center, projectile.Center, target.Hitbox, out Vector2 hitPoint))
			{
                target.StrikeNPC(projectile.damage, projectile.knockBack, Owner.Center.X > target.Center.X ? -1 : 1);
                target.immune[0] = 15;

                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDustPerfect(hitPoint, DustType<Glow>(), Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.25f) * Main.rand.NextFloat(5), 0, new Color(255, 105, 105), 0.5f);

                    Dust.NewDustPerfect(hitPoint, DustID.Blood, Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(2, 8), 0, default, Main.rand.NextFloat(1, 2));
                    Dust.NewDustPerfect(hitPoint, DustID.Blood, Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(3, 15), 0, default, Main.rand.NextFloat(1, 2));
                }
            }

            return false;
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
            var tex = GetTexture(Texture);
            var texGlow = GetTexture(Texture + "Glow");

            float targetAngle = StoredAngle + (-(maxAngle / 2) + Helper.BezierEase(Timer / maxTime) * maxAngle) * Owner.direction * direction;
            var pos = Owner.Center + Vector2.UnitX.RotatedBy(targetAngle) * ((float)Math.Sin(Helper.BezierEase(Timer / maxTime) * 3.14f) * 20) - Main.screenPosition;

            spriteBatch.Draw(tex, pos, null, lightColor, projectile.rotation, new Vector2(0, tex.Height), 1, 0, 0);
            spriteBatch.Draw(texGlow, pos, null, Color.White, projectile.rotation, new Vector2(0, texGlow.Height), 1, 0, 0);

            return false;
		}

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 10; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            cache.Add(projectile.Center);

            while (cache.Count > 10)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => factor * (20 + 40 * Timer / maxTime), factor =>
            {
                if (factor.X >= 0.8f)
                    return Color.White * 0;

                return new Color(255, 160 + (int)(factor.X * 60), 105) * (factor.X * SinProgress );
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/EnergyTrail"));

            trail?.Render(effect);
        }
    }

	public class RefractiveBladeLaser : ModProjectile, IDrawAdditive
	{


		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			
		}
	}
}