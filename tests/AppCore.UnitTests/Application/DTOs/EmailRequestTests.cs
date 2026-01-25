using AppCore.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.DTOs;

public class EmailRequestTests {
    [Fact]
    public void Properties_ShouldSetAndGetCorrectly() {
        // Arrange & Act
        var emailRequest = new EmailRequest {
            To = "recipient@example.com",
            Subject = "Test Subject",
            Body = "Test body content",
            From = "sender@example.com",
            AttachmentFiles = new List<AttachmentFile>()
        };

        // Assert
        emailRequest.To.Should().Be("recipient@example.com");
        emailRequest.Subject.Should().Be("Test Subject");
        emailRequest.Body.Should().Be("Test body content");
        emailRequest.From.Should().Be("sender@example.com");
        emailRequest.AttachmentFiles.Should().BeEmpty();
    }

    [Fact]
    public void AttachmentFiles_ShouldSupportMultipleFiles() {
        // Arrange
        var attachment1 = new AttachmentFile {
            FileBase64 = "base64content1",
            FileName = "file1.pdf",
            ContentType = "application/pdf"
        };
        var attachment2 = new AttachmentFile {
            FileBase64 = "base64content2",
            FileName = "file2.txt",
            ContentType = "text/plain"
        };

        // Act
        var emailRequest = new EmailRequest {
            To = "test@example.com",
            Subject = "Test",
            Body = "Body",
            From = "from@example.com",
            AttachmentFiles = new List<AttachmentFile> { attachment1, attachment2 }
        };

        // Assert
        emailRequest.AttachmentFiles.Should().HaveCount(2);
        emailRequest.AttachmentFiles[0].FileName.Should().Be("file1.pdf");
        emailRequest.AttachmentFiles[1].FileName.Should().Be("file2.txt");
    }
}
