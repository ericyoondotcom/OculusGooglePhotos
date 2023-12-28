﻿using System;
using System.IO;

namespace Sharpen
{
    /// <summary>
    /// http://grepcode.com/file_/repository.grepcode.com/java/root/jdk/openjdk/6-b14/java/io/PushbackReader.java/?v=source
    /// </summary>
    public class PushbackReader : StreamReader
    {
        private readonly object _lock = new object();
        private readonly char[] _buf;
        private int _pos;

        public PushbackReader(StreamReader stream, int size)
            : base(stream.BaseStream)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "size <= 0");

            _buf = new char[size];
            _pos = size;
        }

        public override int Read()
        {
            lock (_lock)
            {
                return _pos < _buf.Length
                    ? _buf[_pos++]
                    : base.Read();
            }
        }

        public override int Read(char[] buffer, int off, int len)
        {
            lock (_lock)
            {
                try
                {
                    if (len <= 0)
                    {
                        if (len < 0)
                            throw new ArgumentException();
                        if (off < 0 || off > buffer.Length)
                            throw new ArgumentException();
                        return 0;
                    }

                    var avail = _buf.Length - _pos;

                    if (avail > 0)
                    {
                        if (len < avail)
                            avail = len;
                        Array.Copy(_buf, _pos, buffer, off, avail);
                        _pos += avail;
                        off += avail;
                        len -= avail;
                    }

                    if (len > 0)
                    {
                        len = base.Read(buffer, off, len);
                        return len != -1
                            ? avail + len
                            : avail == 0
                                ? -1
                                : avail;
                    }

                    return avail;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new ArgumentException();
                }
            }
        }

        public void Unread(char[] buffer, int off, int len)
        {
            lock (_lock)
            {
                if (len > _pos)
                    throw new IOException("Pushback buffer overflow");
                _pos -= len;
                Array.Copy(buffer, off, _buf, _pos, len);
            }
        }
    }
}