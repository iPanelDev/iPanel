using System;

namespace iPanel.Core.Service
{
    internal struct Session
    {
        public Session()
        {
            ExpirationTime = DateTime.Now.AddHours(1);
            ID = Guid.NewGuid().ToString("N");
        }

        public readonly DateTime ExpirationTime;

        public readonly string ID;
    }
}