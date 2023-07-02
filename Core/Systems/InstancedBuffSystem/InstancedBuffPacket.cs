using NetEasy;
using System;
using System.IO;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	[Serializable]
	public class InstancedBuffPacket : Module
	{
		Stream data = new MemoryStream();

		BinaryWriter writer;
		BinaryReader reader;

		public InstancedBuffPacket()
		{
			writer = new BinaryWriter(data);
			reader = new BinaryReader(data);
		}

		public InstancedBuffPacket(Stream data)
		{
			this.data = data;
			writer = new BinaryWriter(data);
			reader = new BinaryReader(data);
		}

		protected override void Receive()
		{
			throw new NotImplementedException();
		}
	}
}
