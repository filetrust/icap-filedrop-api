using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

// ReSharper disable All

namespace Glasswall.Core.Engine.Common
{
    //Casing in these classes must not be changed as this is the data contract between us and the Shared Object / DLL

    [ExcludeFromCodeCoverage]
    [DataContract(Namespace = "")]
    public class config
    { 
        [DataMember]
        public pdfConfig pdfConfig { get; set; }
        [DataMember]
        public pptConfig pptConfig { get; set; }
        [DataMember]
        public xlsConfig xlsConfig { get; set; }
        [DataMember]
        public wordConfig wordConfig { get; set; }
        [DataMember]
        public tiffConfig tiffConfig { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [DataContract(Namespace = "")]
    public class pdfConfig
    {
        [DataMember]
        public contentManagementFlag acroform { get; set; }
        [DataMember]
        public contentManagementFlag actions_all { get; set; }
        [DataMember]
        public contentManagementFlag internal_hyperlinks { get; set; }
        [DataMember]
        public contentManagementFlag external_hyperlinks { get; set; }
        [DataMember]
        public contentManagementFlag embedded_files { get; set; }
        [DataMember]
        public contentManagementFlag embedded_images { get; set; }
        [DataMember]
        public contentManagementFlag javascript { get; set; }
        [DataMember]
        public contentManagementFlag metadata { get; set; }
        [DataMember]
        public string watermark { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [DataContract(Namespace = "")]
    public class wordConfig
    {
        [DataMember]
        public contentManagementFlag embedded_files { get; set; }
        [DataMember]
        public contentManagementFlag embedded_images { get; set; }
        [DataMember]
        public contentManagementFlag internal_hyperlinks { get; set; }
        [DataMember]
        public contentManagementFlag external_hyperlinks { get; set; }
        [DataMember]
        public contentManagementFlag macros { get; set; }
        [DataMember]
        public contentManagementFlag metadata { get; set; }
        [DataMember]
        public contentManagementFlag review_comments { get; set; }
        [DataMember]
        public contentManagementFlag dynamic_data_exchange { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [DataContract(Namespace = "")]
    public class xlsConfig
    {
        [DataMember]
        public contentManagementFlag embedded_files { get; set; }
        [DataMember]
        public contentManagementFlag embedded_images { get; set; }
        [DataMember]
        public contentManagementFlag internal_hyperlinks { get; set; }
        [DataMember]
        public contentManagementFlag external_hyperlinks { get; set; }
        [DataMember]
        public contentManagementFlag macros { get; set; }
        [DataMember]
        public contentManagementFlag metadata { get; set; }
        [DataMember]
        public contentManagementFlag review_comments { get; set; }
        [DataMember]
        public contentManagementFlag dynamic_data_exchange { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [DataContract(Namespace = "")]
    public class pptConfig
    {
        [DataMember]
        public contentManagementFlag embedded_files { get; set; }
        [DataMember]
        public contentManagementFlag embedded_images { get; set; }
        [DataMember]
        public contentManagementFlag internal_hyperlinks { get; set; }
        [DataMember]
        public contentManagementFlag external_hyperlinks { get; set; }
        [DataMember]
        public contentManagementFlag macros { get; set; }
        [DataMember]
        public contentManagementFlag metadata { get; set; }
        [DataMember]
        public contentManagementFlag review_comments { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [DataContract(Namespace = "")]
    public class rtfConfig
    {
        [DataMember]
        public contentManagementFlag embedded_files { get; set; }
        [DataMember]
        public contentManagementFlag hyperlinks { get; set; }
        [DataMember]
        public contentManagementFlag metadata { get; set; }
    }

    [DataContract(Namespace = "")]
    public enum contentManagementFlag
    {
        [EnumMember]
        disallow,
        [EnumMember]
        allow,
        [EnumMember]
        sanitise
    }

    [ExcludeFromCodeCoverage]
    [DataContract(Namespace = "")]
    public class tiffConfig
    {
        [DataMember]
        public contentManagementFlag geotiff { get; set; }
    }
}
