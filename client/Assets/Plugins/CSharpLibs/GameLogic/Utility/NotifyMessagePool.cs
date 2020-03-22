﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;
using Proto;


namespace GameLogic.Utility
{
    public class NotifyMessagePool
    {
        public class Frame
        {
            public int Index;
            public float time;

            private List<IMessage> notifys = new List<IMessage>();

            public void SetNotify(IMessage[] notifys)
            {
                this.notifys = notifys.ToList();
            }

            public void LoadFormBytes(BinaryReader br)
            {
                notifys.Clear();
                time = br.ReadSingle();
                var count = br.ReadInt32();
                while (count-- > 0)
                {
                    var typeIndex = br.ReadInt32();
                    int len = br.ReadInt32();
                    var bytes = br.ReadBytes(len);
                    if (MessageTypeIndexs.TryGetType(typeIndex, out Type type))
                    {
                        var t = Activator.CreateInstance(type) as IMessage;
                        t.MergeFrom(bytes);
                        notifys.Add(t);
                    }
                }
            }

            public void ToBytes(BinaryWriter bw)
            {
                bw.Write(time);
                bw.Write(notifys.Count);
                foreach (var i in notifys)
                {
                    if (MessageTypeIndexs.TryGetIndex(i.GetType(), out int index))
                    {
                        bw.Write(index);
                        var bytes = i.ToByteArray();
                        bw.Write(bytes.Length);
                        bw.Write(bytes);
                    }
                }

            }

            public IMessage[] GetNotify()
            {
                return notifys.ToArray();
            }

       }

        private int frame;

        public int TotalFrame { get { return frames.Count; }}

        public Queue<Frame> frames = new Queue<Frame>();

        public void AddFrame(IMessage[] notify,float time)
        {
            var f = new Frame { Index = this.frame++,time =time};
            f.SetNotify(notify);
            frames.Enqueue(f);
        }

        public void LoadFormBytes(byte[] bytes)
        {
            frame = 0;
            frames = new Queue<Frame>();
            using (var mem = new MemoryStream(bytes))
            {
                using (var br = new BinaryReader(mem))
                {
                    var count = br.ReadInt32();
                    while (count-- > 0)
                    {
                        var f = new Frame() { Index = frame++ };
                        f.LoadFormBytes(br);
                        frames.Enqueue(f);
                    }
                }
            }
        }

        public byte[] ToBytes()
        {
            using (var mem = new MemoryStream())
            {
                using (var bw = new BinaryWriter(mem))
                {
                    bw.Write(frames.Count);
                    foreach (var i in frames)
                    {
                        i.ToBytes(bw);
                    }
                }
                return mem.ToArray();
            }
        }

        public bool NextFrame(out Frame frame)
        {
            frame = null;
            if (frames.Count > 0)
            {
                frame = frames.Dequeue();
                return true;
            }
            return false;
        }
    }
}

