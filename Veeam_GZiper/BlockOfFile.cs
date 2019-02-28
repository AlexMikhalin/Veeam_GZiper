namespace Veeam_GZiper
{
    /// <summary>
    /// The structure describes the block of compressed or decompressed file
    /// </summary>
    /// <param name="Number">Number of block</param>
    /// <param name="Bytes">Size of block</param>
    public struct BlockOfFile
    {
        public ushort Number { get; }
        public byte[] Bytes { get; }
        
        /// <summary>
        /// Creates the instanse of BlockOfFile
        /// </summary>
        /// <param name="number">Number of block</param>
        /// <param name="bytes">Size of block</param>
        public BlockOfFile(ushort number, byte[] bytes)
        {
            Number = number;
            Bytes = bytes;
        }
    }
}
