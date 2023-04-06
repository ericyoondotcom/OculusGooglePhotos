// Copyright (c) Drew Noakes and contributors. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace MetadataExtractor.Formats.Exif.Makernotes
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class FlirMakernoteDescriptor : TagDescriptor<FlirMakernoteDirectory>
    {
        public FlirMakernoteDescriptor(FlirMakernoteDirectory directory)
            : base(directory)
        {
        }
    }
}
