using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ImagePreview.Classification
{
    internal class ClassificationTypeDefinitions
    {
        public const string ImageReference = "ImageReference";

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ImageReference)]
        public static ClassificationTypeDefinition ImageReferenceDefinition { get; set; }

        [Name(ImageReference)]
        [UserVisible(false)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = ImageReference)]
        [Order(Before = Priority.Default)]
        public sealed class ImageReferenceFormat : ClassificationFormatDefinition
        {
            public ImageReferenceFormat()
            {
                IsItalic = true;
            }
        }
    }
}
