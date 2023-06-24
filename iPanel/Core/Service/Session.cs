using System;

namespace iPanel.Core.Service
{
    internal struct Session
    {
        public Session()
        {
            CreateTime = DateTime.Now;
            ID = Guid.NewGuid().ToString("N");
        }

        public readonly DateTime CreateTime;

        public readonly string ID;
    }
}