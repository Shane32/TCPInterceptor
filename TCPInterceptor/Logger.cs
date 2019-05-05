using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace TCPInterceptor
{
    class Logger
    {
        private readonly string _folderName;
        private readonly int _timerDelay;
        private readonly SemaphoreSlim _semaphoreSlim;

        public Logger(string folderName, int timerDelay)
        {
            _folderName = folderName;
            _semaphoreSlim = new SemaphoreSlim(1, 1);
            _timerDelay = timerDelay;
        }

        public DelayedSemaphore GetDelayedSemaphore()
        {
            return new DelayedSemaphore(_semaphoreSlim, _timerDelay);
        }

        int lastId = -1;
        int lastPort = 0;
        bool lastOutbound = false;
        int logId = -1;

        public async Task Log(int id, int port, bool outbound, byte[] buffer, int offset, int count) {
            bool append = (lastId == id && lastPort == port && lastOutbound == outbound);
            int thisLogId = append ? logId : ++logId;
            char outboundChar = outbound ? '+' : '-';
            string filename = $"{_folderName}\\{id:00000}{outboundChar}{port:00000} {DateTime.Now:HH.mm.ss.fffffff}.txt";
            using (var fileStream = new FileStream(filename, FileMode.Append, FileAccess.ReadWrite, FileShare.Read))
            {
                await fileStream.WriteAsync(buffer, offset, count);
            }
        }

    }
}
