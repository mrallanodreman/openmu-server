// <copyright file="GlobalPacketHeaderTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Network.Packets.Tests;

using MUnique.OpenMU.Network.Packets.ServerToClient;
using NUnit.Framework;

/// <summary>
/// Verifies wire headers which cannot be validated by field-boundary tests alone.
/// </summary>
[TestFixture]
public class GlobalPacketHeaderTests
{
    /// <summary>
    /// Ensures that writing the object id does not overwrite the C2 subcode.
    /// </summary>
    [Test]
    public void AddCharacterToScopeGlobalPreservesSubCodeWhenIdIsWritten()
    {
        var data = new byte[AddCharacterToScopeGlobalRef.GetRequiredSize(0)];
        var packet = new AddCharacterToScopeGlobalRef(data);

        packet.Id = 0x1234;

        Assert.That(data[0], Is.EqualTo(0xC2));
        Assert.That(data[3], Is.EqualTo(0x12));
        Assert.That(data[4], Is.EqualTo(0xD6));
        Assert.That(data[5], Is.EqualTo(0x34));
        Assert.That(data[6], Is.EqualTo(0x12));
    }

    /// <summary>
    /// Ensures that the fixed companion snapshot advertises its complete C4 length.
    /// </summary>
    [Test]
    public void CompanionStateUsesTwoByteLengthAndPreservesSubCode()
    {
        var data = new byte[CompanionStateRef.Length];
        var packet = new CompanionStateRef(data);

        packet.CompanionId = 0x12345678;

        Assert.That(data[0], Is.EqualTo(0xC4));
        Assert.That(data[1], Is.EqualTo(CompanionStateRef.Length >> 8));
        Assert.That(data[2], Is.EqualTo(CompanionStateRef.Length & 0xFF));
        Assert.That(data[3], Is.EqualTo(0xF3));
        Assert.That(data[4], Is.EqualTo(0x60));
        Assert.That(data[5], Is.EqualTo(0x78));
        Assert.That(data[6], Is.EqualTo(0x56));
        Assert.That(data[7], Is.EqualTo(0x34));
        Assert.That(data[8], Is.EqualTo(0x12));
    }

    /// <summary>
    /// Ensures that state and inventory packets have distinct dispatch keys.
    /// </summary>
    [Test]
    public void CompanionPacketsUseDistinctSubCodes()
    {
        Assert.That(CompanionStateRef.SubCode, Is.EqualTo(0x60));
        Assert.That(CompanionInventoryRef.SubCode, Is.EqualTo(0x62));
    }
}
