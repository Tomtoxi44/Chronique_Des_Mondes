// -----------------------------------------------------------------------
// <copyright file="ImagePolicyTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Tests.Services;

using Cdm.Common.Services;
using Xunit;

/// <summary>
/// Tests des limites par usage : les portraits (photo de profil, personnage, PNJ) sont
/// bridés en poids et en dimensions, les illustrations de campagne ou de chapitre non.
/// </summary>
public class ImagePolicyTests
{
    /// <summary>PNG minimal dont l'en-tête IHDR annonce les dimensions demandées.</summary>
    private static byte[] PngOf(int width, int height)
    {
        var bytes = new byte[24];
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        signature.CopyTo(bytes, 0);
        WriteBigEndian(bytes, 16, width);
        WriteBigEndian(bytes, 20, height);
        return bytes;
    }

    private static void WriteBigEndian(byte[] bytes, int offset, int value)
    {
        bytes[offset] = (byte)(value >> 24);
        bytes[offset + 1] = (byte)(value >> 16);
        bytes[offset + 2] = (byte)(value >> 8);
        bytes[offset + 3] = (byte)value;
    }

    [Theory]
    [InlineData("avatars")]
    [InlineData("portraits")]
    public void For_PortraitCategories_UsesTighterLimits(string category)
    {
        var policy = ImagePolicy.For(category);

        Assert.Equal(ImagePolicy.Portrait, policy);
        Assert.Equal(2 * 1024 * 1024, policy.MaxFileSizeBytes);
        Assert.Equal(2048, policy.MaxDimension);
    }

    [Theory]
    [InlineData("maps")]
    [InlineData("session")]
    [InlineData("misc")]
    [InlineData(null)]
    public void For_OtherCategories_LeavesDimensionsFree(string? category)
    {
        var policy = ImagePolicy.For(category);

        Assert.Equal(5 * 1024 * 1024, policy.MaxFileSizeBytes);
        Assert.Equal(0, policy.MaxDimension);
    }

    [Fact]
    public void TryReadDimensions_Png_ReadsHeader()
    {
        Assert.True(ImageValidation.TryReadDimensions(PngOf(1920, 1080), "image/png", out var w, out var h));
        Assert.Equal(1920, w);
        Assert.Equal(1080, h);
    }

    [Fact]
    public void TryValidate_PortraitTooLarge_IsRejectedWithItsSize()
    {
        var ok = ImageValidation.TryValidate(PngOf(4000, 3000), ImagePolicy.Portrait, out _, out _, out var error);

        Assert.False(ok);
        Assert.Contains("4000", error);
        Assert.Contains("2048", error);
    }

    [Fact]
    public void TryValidate_SameImageAsFreeIllustration_IsAccepted()
    {
        // Une carte de 4000 px reste légitime : seule la catégorie portrait est bridée.
        var ok = ImageValidation.TryValidate(PngOf(4000, 3000), ImagePolicy.Default, out _, out _, out var error);

        Assert.True(ok);
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_PortraitWithinLimits_IsAccepted()
    {
        Assert.True(ImageValidation.TryValidate(PngOf(512, 512), ImagePolicy.Portrait, out _, out _, out var error));
        Assert.Null(error);
    }

    [Fact]
    public void Describe_MentionsBothLimitsForPortraits()
    {
        var description = ImagePolicy.Portrait.Describe();

        Assert.Contains("2 Mo", description);
        Assert.Contains("2048", description);
    }

    [Fact]
    public void Describe_OmitsDimensionsWhenFree()
    {
        var description = ImagePolicy.Default.Describe();

        Assert.Contains("5 Mo", description);
        Assert.DoesNotContain("px", description);
    }
}
