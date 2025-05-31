using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Scripts.Simulation
{
    public enum DataGroup
    {
        // [EnumMember(Value = "controller")]
        Controller,
        // [EnumMember(Value = "general")]
        General,
        // [EnumMember(Value = "hidden")]
        Hidden
    }
    public enum DataType
    {
        // [EnumMember(Value = "text")]
        Text,
        // [EnumMember(Value = "select")]
        Select
    }
    
    public enum FileType
    {
        Tooltip,
        Document,
        [EnumMember(Value = "3D model")]
        Model3D,
        Model,
        Image,
        [EnumMember(Value = "PDF_public")]
        PDF
    }

    public class Controller
    {
        [JsonProperty("id")]
        public string ExperimentId { get; set; }

        [JsonProperty(PropertyName = "controller")]
        public string Name { get; set; }

        public ExperimentData ExperimentData { get; set; }
    }
    
    public class ExperimentData
    {
        [JsonProperty(PropertyName = "inputs")] 
        public List<InputParameter> InputParameters { get; set; } = new List<InputParameter>();

        [JsonProperty(PropertyName = "files")] 
        public List<File> Files { get; set; } = new List<File>();
    }

    public class InputParameter
    {
        [JsonProperty(PropertyName = "id")] 
        public int Order { get; set; } // or use "order" if your API provides it

        [JsonProperty(PropertyName = "name")]
        public string SchemaVar { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "group")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DataGroup Group { get; set; }
        
        [JsonProperty(PropertyName = "type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DataType Type { get; set; }

        [JsonProperty(PropertyName = "input")]
        public List<InputValue> DefaultValue { get; set; }
    }

    public class InputValue
    {
        [JsonProperty(PropertyName = "name")] 
        public string Name { get; set; }
        [JsonProperty(PropertyName = "value")] 
        public string Value { get; set; }
    }

    public class File
    {
        [JsonProperty(PropertyName = "filename")] 
        public string FileName { get; set; }
        [JsonProperty(PropertyName = "url")] 
        public string URL { get; set; }
        [JsonProperty(PropertyName = "public_url")] 
        public string PublicURL { get; set; }
        [JsonProperty(PropertyName = "visibility")] 
        public string Visibility { get; set; }
        [JsonProperty(PropertyName = "filetype")]
        public FileInfo FileInfo { get; set; }
    }

    public class FileInfo
    {
        [JsonProperty(PropertyName = "file_type")] 
        public FileType FileType { get; set; }
        [JsonProperty(PropertyName = "file_path")] 
        public string FilePath { get; set; }
    }
}