using OrionSoft.AppCore.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.DTOs;

public class AttachmentFileTests {
    [Fact]
    public void Properties_ShouldSetAndGetCorrectly() {
        // Arrange & Act
        var attachment = new AttachmentFile {
            FileBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            FileName = "test-image.png",
            ContentType = "image/png"
        };

        // Assert
        attachment.FileBase64.Should().NotBeNullOrEmpty();
        attachment.FileName.Should().Be("test-image.png");
        attachment.ContentType.Should().Be("image/png");
    }

    [Fact]
    public void FileBase64_ShouldStoreBase64EncodedData() {
        // Arrange
        var base64Data = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });

        // Act
        var attachment = new AttachmentFile {
            FileBase64 = base64Data,
            FileName = "data.bin",
            ContentType = "application/octet-stream"
        };

        // Assert
        attachment.FileBase64.Should().Be(base64Data);
    }
}
