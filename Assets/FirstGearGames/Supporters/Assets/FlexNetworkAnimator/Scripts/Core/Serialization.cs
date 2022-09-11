﻿using Mirror;
using System;

namespace FirstGearGames.Mirrors.Assets.FlexNetworkAnimators
{



    public static class Serialization
    {

        public static NetworkWriterPooled SerializeAnimatorUpdate(AnimatorUpdate au)
        {
            using (NetworkWriterPooled writer = NetworkWriterPool.Get())
            {
                //NetworkIdentity.                
                Compressions.WriteCompressedUInt(writer, au.NetworkIdentity);
                //ComponentIndex.
                writer.WriteByte(au.ComponentIndex);
                //Data.
                Compressions.WriteCompressedInt(writer, au.Data.Count);
                if (au.Data.Array.Length > 0)
                    writer.WriteBytes(au.Data.Array, 0, au.Data.Count);

                return writer;
            }
        }


        public static void DeserializeAnimatorUpdate(ref AnimatorUpdate au, ref int readPosition, ArraySegment<byte> data)
        {
            using (NetworkReaderPooled reader = NetworkReaderPool.Get(data))
            {
                reader.Position = readPosition;

                //NetworkIdentity.
                au.NetworkIdentity = Compressions.ReadCompressedUInt(reader);
                //ComponentIndex.
                au.ComponentIndex = reader.ReadByte();
                //Data.
                int dataLength = Compressions.ReadCompressedInt(reader);
                if (dataLength > 0)
                    au.Data = reader.ReadBytesSegment(dataLength);

                readPosition = reader.Position;
           }
        }



    }


}