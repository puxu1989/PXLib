using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PXLib.Zip.BZip2
{
    public static class BZip2
    {
        /// <summary>
        /// Decompress the <paramref name="inStream">input</paramref> writing 
        /// uncompressed data to the <paramref name="outStream">output stream</paramref>
        /// </summary>
        /// <param name="inStream">The readable stream containing data to decompress.</param>
        /// <param name="outStream">The output stream to receive the decompressed data.</param>
        /// <param name="isStreamOwner">Both streams are closed on completion if true.</param>
        public static void Decompress(Stream inStream, Stream outStream, bool isStreamOwner=true)
        {
            if (inStream == null || outStream == null)
            {
                throw new Exception("Null Stream");
            }
            try
            {
                using (BZip2InputStream bZip2InputStream = new BZip2InputStream(inStream))
                {
                    bZip2InputStream.IsStreamOwner = isStreamOwner;
                    StreamHelper.Copy(bZip2InputStream, outStream, new byte[4096]);
                }
            }
            finally
            {
                if (isStreamOwner)
                {
                    outStream.Close();
                }
            }
        }
        /// <summary>
        /// Compress the <paramref name="inStream">input stream</paramref> sending 
        /// result data to <paramref name="outStream">output stream</paramref>
        /// </summary>
        /// <param name="inStream">The readable stream to compress.</param>
        /// <param name="outStream">The output stream to receive the compressed data.</param>
        /// <param name="isStreamOwner">Both streams are closed on completion if true.</param>
        /// <param name="level">Block size acts as compression level (1 to 9) with 1 giving 
        /// the lowest compression and 9 the highest.</param>
        public static void Compress(Stream inStream, Stream outStream, int level, bool isStreamOwner=true)
        {
            if (inStream == null || outStream == null)
            {
                throw new Exception("Null Stream");
            }
            try
            {
                using (BZip2OutputStream bZip2OutputStream = new BZip2OutputStream(outStream, level))
                {
                    bZip2OutputStream.IsStreamOwner = isStreamOwner;
                    StreamHelper.Copy(inStream, bZip2OutputStream, new byte[4096]);
                }
            }
            finally
            {
                if (isStreamOwner)
                {
                    inStream.Close();
                }
            }
        }
    }
}
