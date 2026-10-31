using System.Text.Json.Serialization;
using dikit.Infrastructure;

namespace dikit.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextBlock), "text")]
[JsonDerivedType(typeof(ImageBlock), "image")]
[JsonDerivedType(typeof(LinkBlock), "link")]
[JsonDerivedType(typeof(ChecklistBlock), "checklist")]
public abstract class ContentBlock : ObservableObject
{
}
