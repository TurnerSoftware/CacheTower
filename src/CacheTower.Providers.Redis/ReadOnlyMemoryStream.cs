using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CacheTower.Providers.Redis
{
	class ReadOnlyMemoryStream : Stream
	{
		private ReadOnlyMemory<byte> ReadOnlyMemory { get; }

		public ReadOnlyMemoryStream(ReadOnlyMemory<byte> readOnlyMemory)
		{
			ReadOnlyMemory = ReadOnlyMemory;
		}

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => false;

		public override long Length => ReadOnlyMemory.Length;

		public override long Position { get; set; }

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var memorySpan = ReadOnlyMemory.Span;
			var bytesRead = (int)Math.Min(Length - Position, Position + count);
			var dataSource = memorySpan.Slice((int)Position, bytesRead);

			var bufferSpan = buffer.AsSpan().Slice(offset, count);
			dataSource.CopyTo(bufferSpan);

			return bytesRead;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			Position += offset;
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}
	}
}
