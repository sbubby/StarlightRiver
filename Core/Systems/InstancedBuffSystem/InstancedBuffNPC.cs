using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	internal class InstancedBuffNPC : GlobalNPC
	{
		/// <summary>
		/// The instanced buffs that exist on this NPC
		/// </summary>
		public readonly List<InstancedBuff> buffInstances = new();

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			On_NPC.UpdateNPC_BuffSetFlags += UpdateInstancedBuffs;
		}

		/// <summary>
		/// Gets the instance of a given instanced buff inflicted on an NPC
		/// </summary>
		/// <typeparam name="T">The type of the instanced buff to get</typeparam>
		/// <param name="npc">The NPC to check for this buff on</param>
		/// <returns>The inflicted instance, or null if one does not exist</returns>
		public static T? GetInstance<T>(NPC npc) where T : InstancedBuff
		{
			return npc.GetGlobalNPC<InstancedBuffNPC>().buffInstances.FirstOrDefault(n => n is T) as T;
		}

		/// <summary>
		/// Gets the instanced buff with the given internal name on an NPC
		/// </summary>
		/// <param name="npc">the NPC to check for this buff on</param>
		/// <param name="name">the internal name of the buff to check for</param>
		/// <returns>The inflicted instance, or null if one does not exist</returns>
		public static InstancedBuff? GetInstance(NPC npc, string name)
		{
			return npc.GetGlobalNPC<InstancedBuffNPC>().buffInstances.FirstOrDefault(n => n.Name == name);
		}

		/// <summary>
		/// Updates the standard update loop for instanced buffs
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="lowerBuffTime"></param>
		private void UpdateInstancedBuffs(On_NPC.orig_UpdateNPC_BuffSetFlags orig, NPC self, bool lowerBuffTime)
		{
			orig(self, lowerBuffTime);

			if (self.active && self.type > 0)
			{
				try
				{
					self.GetGlobalNPC<InstancedBuffNPC>().buffInstances.ForEach(n => n.UpdateNPC(self));
				}
				catch
				{
					Mod.Logger.Error($"Tried to get a nonexistant global to update instanced buffs! WhoAmI: {self.whoAmI}, Name: {self.FullName}, Type: {self.type}");
				}
			}
		}

		/// <summary>
		/// Handles removing all expired buff instances
		/// </summary>
		/// <param name="npc"></param>
		public override void PostAI(NPC npc)
		{
			buffInstances.RemoveAll(n => !npc.HasBuff(n.BackingType));
		}

		public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			binaryWriter.Write(buffInstances.Count);

			for (int k = 0; k < buffInstances.Count; k++)
			{
				InstancedBuff buff = buffInstances[k];
				binaryWriter.Write(buff.Name);
				buff.Serialize(binaryWriter);

				Mod.Logger.Info($"({npc.FullName} {npc.whoAmI}): sending an instanced buff as part of sync: {buff.Name}");
			}
		}

		public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
		{
			int count = binaryReader.ReadInt32();

			for (int k = 0; k < count; k++)
			{
				string name = binaryReader.ReadString();

				Mod.Logger.Info($"({npc.FullName} {npc.whoAmI}): recieved an instanced buff as part of sync: {name}");

				InstancedBuff buff = GetInstance(npc, name);

				// re-inflict if its not there or lost
				if (buff is null)
				{
					Mod.Logger.Info($"({npc.FullName} {npc.whoAmI}): inflicting an instanced buff as part of sync: {name}");
					buff = InstancedBuff.samples[name].Clone();
					buffInstances.Add(buff);
				}

				buff.Deserialize(binaryReader);
			}
		}
	}
}