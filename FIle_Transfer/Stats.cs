using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Transfer
{
    internal class Stats
    {
        /// <summary>
        /// total size transferred in bytes
        /// </summary>
        public long SizeTransferred { get; set; }
        /// <summary>
        /// count of files transferred
        /// </summary>
        public long FilesTransferred { get; set; }

        /// <summary>
        /// size of compressed files
        /// </summary>
        public long CompressedSize { get; set; }

        /// <summary>
        /// time it took to transfer
        /// </summary>
        public Stopwatch Time { get; set; }

        public Stats() 
        {
            SizeTransferred= 0;
            FilesTransferred= 0;
            CompressedSize= 0;
            Time = new Stopwatch();
        }

        /// <summary>
        /// overrides To String() to provide all stats
        /// </summary>
        /// <returns>string of stats</returns>
        public override string ToString()
        {
            StringBuilder results = new StringBuilder();
            results.AppendFormat("Execution time: {0}\n", Time.Elapsed.ToString());
            results.AppendFormat("Number of Files Transferred: {0} \n", FilesTransferred);
            results.AppendFormat("Total size transferred: {0} MB\n", SizeToMB(SizeTransferred));
            if(CompressedSize != 0)
            {
                results.AppendFormat("Compressed Zip File Size: {0} MB\n", SizeToMB(CompressedSize));
            }

            return results.ToString();
        }

        /// <summary>
        /// converts SizeTransferred from bytes to MB
        /// </summary>
        /// <returns></returns>
        private long SizeToMB(long value)
        {
           return value / 1000000;
        }
    }
}
