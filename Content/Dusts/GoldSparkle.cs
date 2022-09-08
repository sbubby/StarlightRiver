﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using System;

namespace StarlightRiver.Content.Dusts
{
    class GoldSparkle : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 14, 14);
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(7, 7) * dust.scale;
                dust.customData = 1;
            }

            if (dust.alpha % 64 == 56)
                dust.frame.Y+= 14;

            Lighting.AddLight(dust.position, Color.Gold.ToVector3() * 0.02f);

            dust.alpha += 8;

            if (dust.alpha > 255)
                dust.active = false;

            dust.position += dust.velocity;
            return false;
        }
    }
}
