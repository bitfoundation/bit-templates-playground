using Bit.TemplatePlayground.Server.Api.Models.Attachments;

namespace Bit.TemplatePlayground.Server.Api.Data.Configurations.Attachments;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.HasKey(attachment => new { attachment.Id, attachment.Kind });
    }
}
