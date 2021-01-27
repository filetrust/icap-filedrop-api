using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Glasswall.Core.Engine.Common;
using Glasswall.Core.Engine.Common.PolicyConfig;

namespace Glasswall.Core.Engine
{
    public class GlasswallConfigurationAdaptor: IAdaptor<ContentManagementFlags, string>
    {
        public string Adapt(ContentManagementFlags contentManagementFlags)
        {
            if (contentManagementFlags == null) throw new ArgumentNullException(nameof(contentManagementFlags));
            if (contentManagementFlags.PdfContentManagement == null) throw new ArgumentNullException(nameof(contentManagementFlags.PdfContentManagement));
            if (contentManagementFlags.WordContentManagement == null) throw new ArgumentNullException(nameof(contentManagementFlags.WordContentManagement));
            if (contentManagementFlags.ExcelContentManagement == null) throw new ArgumentNullException(nameof(contentManagementFlags.ExcelContentManagement));
            if (contentManagementFlags.PowerPointContentManagement == null) throw new ArgumentNullException(nameof(contentManagementFlags.PowerPointContentManagement));

            var config = CreateConfig(contentManagementFlags);
            var serializer = new DataContractSerializer(typeof(config));
            var stringWriter = new Utf8StringWriter();

            using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
            {
                serializer.WriteObject(xmlWriter, config);
            }

            var generatedXmlConfig = stringWriter.ToString();

            return generatedXmlConfig;
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        private config CreateConfig(ContentManagementFlags contentManagementFlags)
        {
            return new config
            {
                pdfConfig = new pdfConfig
                {
                    acroform = contentManagementFlags.PdfContentManagement.Acroform.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    actions_all = contentManagementFlags.PdfContentManagement.ActionsAll.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    internal_hyperlinks = contentManagementFlags.PdfContentManagement.InternalHyperlinks.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    external_hyperlinks = contentManagementFlags.PdfContentManagement.ExternalHyperlinks.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    embedded_files = contentManagementFlags.PdfContentManagement.EmbeddedFiles.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    embedded_images = contentManagementFlags.PdfContentManagement.EmbeddedImages.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    javascript = contentManagementFlags.PdfContentManagement.Javascript.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    metadata = contentManagementFlags.PdfContentManagement.Metadata.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag()
                },
                pptConfig = new pptConfig
                {
                    embedded_files = contentManagementFlags.PowerPointContentManagement.EmbeddedFiles.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    embedded_images = contentManagementFlags.PowerPointContentManagement.EmbeddedImages.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    internal_hyperlinks = contentManagementFlags.PowerPointContentManagement.InternalHyperlinks.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    external_hyperlinks = contentManagementFlags.PowerPointContentManagement.ExternalHyperlinks.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    macros = contentManagementFlags.PowerPointContentManagement.Macros.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    metadata = contentManagementFlags.PowerPointContentManagement.Metadata.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    review_comments = contentManagementFlags.PowerPointContentManagement.ReviewComments.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag()
                },
                xlsConfig = new xlsConfig
                {
                    embedded_files = contentManagementFlags.ExcelContentManagement.EmbeddedFiles.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    embedded_images = contentManagementFlags.ExcelContentManagement.EmbeddedImages.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    internal_hyperlinks = contentManagementFlags.ExcelContentManagement.InternalHyperlinks.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    external_hyperlinks = contentManagementFlags.ExcelContentManagement.ExternalHyperlinks.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    macros = contentManagementFlags.ExcelContentManagement.Macros.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    metadata = contentManagementFlags.ExcelContentManagement.Metadata.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    review_comments = contentManagementFlags.ExcelContentManagement.ReviewComments.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    dynamic_data_exchange = contentManagementFlags.ExcelContentManagement.DynamicDataExchange.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag()
                },
                wordConfig = new wordConfig
                {
                    embedded_files = contentManagementFlags.WordContentManagement.EmbeddedFiles.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    embedded_images = contentManagementFlags.WordContentManagement.EmbeddedImages.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    internal_hyperlinks = contentManagementFlags.WordContentManagement.InternalHyperlinks.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    external_hyperlinks = contentManagementFlags.WordContentManagement.ExternalHyperlinks.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    macros = contentManagementFlags.WordContentManagement.Macros.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    metadata = contentManagementFlags.WordContentManagement.Metadata.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    review_comments = contentManagementFlags.WordContentManagement.ReviewComments.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag(),
                    dynamic_data_exchange = contentManagementFlags.WordContentManagement.DynamicDataExchange.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag()
                },
                tiffConfig = new tiffConfig
                {
                    geotiff = contentManagementFlags.TiffContentManagement.Geotiff.GetValueOrDefault().ToGlasswallConfigurationContentManagementFlag()

                }
            };
        }
    }
}